// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Core.Mappings;
using Voting.ECollecting.Admin.Core.Permissions;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Admin.Domain.Queries;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.ECollecting.Shared.Domain.Extensions;

namespace Voting.ECollecting.Admin.Core.Services;

public class InitiativeAdmissibilityDecisionService : IInitiativeAdmissibilityDecisionService
{
    private readonly IInitiativeRepository _initiativeRepository;
    private readonly IPermissionService _permissionService;
    private readonly TimeProvider _timeProvider;
    private readonly CollectionService _collectionService;
    private readonly IDataContext _dataContext;
    private readonly InitiativeService _initiativeService;

    public InitiativeAdmissibilityDecisionService(
        IInitiativeRepository initiativeRepository,
        IPermissionService permissionService,
        TimeProvider timeProvider,
        CollectionService collectionService,
        IDataContext dataContext,
        InitiativeService initiativeService)
    {
        _initiativeRepository = initiativeRepository;
        _permissionService = permissionService;
        _timeProvider = timeProvider;
        _collectionService = collectionService;
        _dataContext = dataContext;
        _initiativeService = initiativeService;
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
        initiatives.SetPeriodStates(_timeProvider.GetUtcNowDateTime());
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
        initiatives.SetPeriodStates(_timeProvider.GetUtcNowDateTime());
        _collectionService.LoadPermissions(initiatives);
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

        initiative.AdmissibilityDecisionState = state;
        initiative.GovernmentDecisionNumber = governmentDecisionNumber;
        await SetState(initiative, state);
        _permissionService.SetModified(initiative);
        await _dataContext.SaveChangesAsync();
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
        };

        await _initiativeService.ValidateGeneralInformation(initiative);
        _permissionService.SetCreated(initiative);
        await _initiativeRepository.Create(initiative);
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
        await _dataContext.SaveChangesAsync();
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
}
