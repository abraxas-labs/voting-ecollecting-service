// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Core.Exceptions;
using Voting.ECollecting.Admin.Core.Mappings;
using Voting.ECollecting.Admin.Core.Permissions;
using Voting.ECollecting.Admin.Core.Resources;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Admin.Domain.Queries;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.ECollecting.Shared.Domain.Extensions;

namespace Voting.ECollecting.Admin.Core.Services;

public class InitiativeAdmissibilityDecisionService : IInitiativeAdmissibilityDecisionService
{
    private const string GovernmentDecisionNumberUniqueConstraintName = "IX_Collections_GovernmentDecisionNumberLower";

    private readonly IInitiativeRepository _initiativeRepository;
    private readonly IPermissionService _permissionService;
    private readonly TimeProvider _timeProvider;
    private readonly CollectionService _collectionService;
    private readonly IDataContext _dataContext;
    private readonly InitiativeService _initiativeService;
    private readonly IAccessControlListDoiRepository _accessControlListDoiRepository;

    public InitiativeAdmissibilityDecisionService(
        IInitiativeRepository initiativeRepository,
        IPermissionService permissionService,
        TimeProvider timeProvider,
        CollectionService collectionService,
        IDataContext dataContext,
        InitiativeService initiativeService,
        IAccessControlListDoiRepository accessControlListDoiRepository)
    {
        _initiativeRepository = initiativeRepository;
        _permissionService = permissionService;
        _timeProvider = timeProvider;
        _collectionService = collectionService;
        _dataContext = dataContext;
        _initiativeService = initiativeService;
        _accessControlListDoiRepository = accessControlListDoiRepository;
    }

    public async Task<List<Initiative>> ListEligibleForAdmissibilityDecision()
    {
        var entities = await _initiativeRepository.Query()
            .WhereCanCreateLinkedAdmissibilityDecision(_permissionService)
            .Include(x => x.SubType)
            .IncludeMunicipalities(_permissionService.AclBfsLists)
            .OrderBy(x => x.Description)
            .ToListAsync();
        var initiatives = Mapper.MapToInitiatives(entities);
        initiatives.SetPeriodStates(_timeProvider.GetUtcTodayDateOnly());
        _collectionService.LoadPermissions(initiatives);
        return initiatives;
    }

    public async Task<List<Initiative>> ListAdmissibilityDecisions()
    {
        var entities = await _initiativeRepository.Query()
            .WhereCanReadAdmissibilityDecision(_permissionService)
            .Include(x => x.SubType)
            .IncludeMunicipalities(_permissionService.AclBfsLists)
            .Where(x => x.AdmissibilityDecisionState.HasValue)
            .OrderByDescending(x => x.CollectionStartDate)
            .ThenBy(x => x.Description)
            .ToListAsync();
        var initiatives = Mapper.MapToInitiatives(entities);
        initiatives.SetPeriodStates(_timeProvider.GetUtcTodayDateOnly());
        _collectionService.LoadPermissions(initiatives);
        await LoadDomainOfInfluenceNames(initiatives);
        return initiatives;
    }

    public async Task DeleteAdmissibilityDecision(Guid id)
    {
        var collection = await _initiativeRepository.Query()
                             .WhereCanDeleteAdmissibilityDecision(_permissionService)
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), id);

        await _initiativeRepository.AuditedDelete(collection);
    }

    public async Task CreateLinkedAdmissibilityDecision(
        Guid initiativeId,
        string governmentDecisionNumber,
        AdmissibilityDecisionState state)
    {
        var initiative = await _initiativeRepository.Query()
                             .IncludeAcceptedDeputyPermissions()
                             .WhereCanCreateLinkedAdmissibilityDecision(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == initiativeId)
                         ?? throw new EntityNotFoundException(typeof(InitiativeEntity), initiativeId);

        try
        {
            initiative.AdmissibilityDecisionState = state;
            initiative.GovernmentDecisionNumber = governmentDecisionNumber;
            _permissionService.SetModified(initiative);
            await SetState(initiative, state);
            await _dataContext.SaveChangesAsync();
        }
        catch (Exception e) when (e.InnerException is PostgresException { ConstraintName: GovernmentDecisionNumberUniqueConstraintName })
        {
            throw new DuplicatedGovernmentDecisionNumberException(initiative.GovernmentDecisionNumber);
        }
    }

    public async Task<Guid> CreateWithAdmissibilityDecision(CreateInitiativeParams reqParams)
    {
        var initiative = new InitiativeEntity
        {
            DomainOfInfluenceType = reqParams.DomainOfInfluenceType,
            SubTypeId = reqParams.SubTypeId,
            Description = reqParams.Description,
            Wording = reqParams.Wording,
            Address = reqParams.Address ?? new(),
            GovernmentDecisionNumber = reqParams.GovernmentDecisionNumber,
            AdmissibilityDecisionState = reqParams.AdmissibilityDecisionState,
            State = CollectionState.PreRecorded,
            IsElectronicSubmission = false,
            LockedFields = new InitiativeLockedFields { Description = true, Wording = true, },
        };

        await _initiativeService.ValidateGeneralInformation(initiative);
        _permissionService.SetCreated(initiative);
        try
        {
            await _collectionService.CreateWithSecretIdNumber(initiative);
        }
        catch (Exception e) when (e.InnerException is PostgresException { ConstraintName: GovernmentDecisionNumberUniqueConstraintName })
        {
            throw new DuplicatedGovernmentDecisionNumberException(initiative.GovernmentDecisionNumber);
        }

        return initiative.Id;
    }

    public async Task UpdateAdmissibilityDecision(
        Guid initiativeId,
        string? governmentDecisionNumber,
        AdmissibilityDecisionState state)
    {
        if (string.IsNullOrEmpty(governmentDecisionNumber) && state == AdmissibilityDecisionState.Open)
        {
            throw new ValidationException("Cannot update to Open state.");
        }

        await using var transaction = await _dataContext.BeginTransaction();
        var initiative = await _initiativeRepository.Query()
                             .IncludeAcceptedDeputyPermissions()
                             .WhereCanEditAdmissibilityDecision(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == initiativeId)
                         ?? throw new EntityNotFoundException(typeof(InitiativeEntity), initiativeId);

        await SetState(initiative, state);

        if (!string.IsNullOrEmpty(governmentDecisionNumber))
        {
            initiative.GovernmentDecisionNumber = governmentDecisionNumber;
        }

        if (state != AdmissibilityDecisionState.Open && string.IsNullOrEmpty(initiative.GovernmentDecisionNumber))
        {
            throw new ValidationException("Cannot set a state other than open without a government decision number.");
        }

        initiative.AdmissibilityDecisionState = state;
        _permissionService.SetModified(initiative);

        try
        {
            await _dataContext.SaveChangesAsync();
        }
        catch (Exception e) when (e.InnerException is PostgresException { ConstraintName: GovernmentDecisionNumberUniqueConstraintName })
        {
            throw new DuplicatedGovernmentDecisionNumberException(initiative.GovernmentDecisionNumber);
        }

        await transaction.CommitAsync();
    }

    private async Task SetState(InitiativeEntity initiative, AdmissibilityDecisionState state)
    {
        var originalState = initiative.State;
        initiative.State = (state, initiative.State) switch
        {
            (AdmissibilityDecisionState.Valid, CollectionState.UnderReview) => CollectionState.ReadyForRegistration,
            (AdmissibilityDecisionState.Rejected, CollectionState.UnderReview) => CollectionState.NotPassed,
            (AdmissibilityDecisionState.Valid, CollectionState.Submitted) => CollectionState.ReadyForRegistration,
            (AdmissibilityDecisionState.Rejected, CollectionState.Submitted) => CollectionState.NotPassed,
            _ => initiative.State,
        };

        if (originalState != initiative.State)
        {
            await _collectionService.AddStateChangedMessage(initiative);
        }
    }

    private async Task LoadDomainOfInfluenceNames(List<Initiative> initiatives)
    {
        var domainOfInfluenceNamesByBfs = await _accessControlListDoiRepository
            .Query()
            .Where(x => !string.IsNullOrWhiteSpace(x.Bfs) && x.Type == AclDomainOfInfluenceType.Mu)
            .GroupBy(x => x.Bfs)
            .ToDictionaryAsync(x => x.Key!, x => x.First().Name);
        foreach (var initiative in initiatives)
        {
            initiative.DomainOfInfluenceName = initiative.DomainOfInfluenceType switch
            {
                DomainOfInfluenceType.Mu => domainOfInfluenceNamesByBfs[initiative.Bfs!],
                DomainOfInfluenceType.Ct => Strings.DomainOfInfluenceName_Ct,
                DomainOfInfluenceType.Ch => Strings.DomainOfInfluenceName_Ch,
                _ => throw new ArgumentOutOfRangeException(nameof(initiative.DomainOfInfluenceType)),
            };
        }
    }
}
