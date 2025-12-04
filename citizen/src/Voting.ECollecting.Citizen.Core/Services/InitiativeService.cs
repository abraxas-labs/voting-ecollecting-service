// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin;
using Voting.ECollecting.Citizen.Abstractions.Core.Services;
using Voting.ECollecting.Citizen.Core.Configuration;
using Voting.ECollecting.Citizen.Core.Exceptions;
using Voting.ECollecting.Citizen.Core.Mappings;
using Voting.ECollecting.Citizen.Core.Permissions;
using Voting.ECollecting.Citizen.Core.Services.Signature;
using Voting.ECollecting.Citizen.Core.Services.Validation;
using Voting.ECollecting.Citizen.Domain.Exceptions;
using Voting.ECollecting.Citizen.Domain.Models;
using Voting.ECollecting.Citizen.Domain.Queries;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.ECollecting.Shared.Domain.Queries;

namespace Voting.ECollecting.Citizen.Core.Services;

public class InitiativeService : IInitiativeService
{
    private readonly IInitiativeSubTypeRepository _initiativeSubTypeRepository;
    private readonly IInitiativeRepository _initiativeRepository;
    private readonly IAccessControlListDoiRepository _accessControlListDoiRepository;
    private readonly TimeProvider _timeProvider;
    private readonly CoreAppConfig _config;
    private readonly IPermissionService _permissionService;
    private readonly CollectionService _collectionService;
    private readonly IDataContext _dataContext;
    private readonly CollectionFilesService _collectionFilesService;
    private readonly InitiativeValidationService _initiativeValidationService;
    private readonly InitiativeSignService _initiativeSignatureService;

    public InitiativeService(
        IInitiativeSubTypeRepository initiativeSubTypeRepository,
        IInitiativeRepository initiativeRepository,
        IAccessControlListDoiRepository accessControlListDoiRepository,
        TimeProvider timeProvider,
        CoreAppConfig config,
        IPermissionService permissionService,
        CollectionService collectionService,
        IDataContext dataContext,
        CollectionFilesService collectionFilesService,
        InitiativeValidationService initiativeValidationService,
        InitiativeSignService initiativeSignatureService)
    {
        _initiativeSubTypeRepository = initiativeSubTypeRepository;
        _initiativeRepository = initiativeRepository;
        _accessControlListDoiRepository = accessControlListDoiRepository;
        _timeProvider = timeProvider;
        _config = config;
        _permissionService = permissionService;
        _collectionService = collectionService;
        _dataContext = dataContext;
        _collectionFilesService = collectionFilesService;
        _initiativeValidationService = initiativeValidationService;
        _initiativeSignatureService = initiativeSignatureService;
    }

    public async Task<List<Initiative>> ListMy()
    {
        var entities = await _initiativeRepository.Query()
            .WhereCanRead(_permissionService)
            .IncludePermission(_permissionService.UserId)
            .Include(x => x.CollectionCount)
            .OrderByDescending(x => x.AuditInfo.CreatedAt)
            .ToListAsync();

        var initiatives = Mapper.MapToInitiatives(entities);
        SetStatesAndPermissions(initiatives);
        return initiatives;
    }

    public Task<List<InitiativeSubTypeEntity>> ListSubTypes()
    {
        return _initiativeSubTypeRepository.Query()
            .OrderBy(x => x.DomainOfInfluenceType)
            .ThenBy(x => x.Description)
            .ToListAsync();
    }

    public async Task<Guid> Create(DomainOfInfluenceType domainOfInfluenceType, string description, Guid? subTypeId, string bfs)
    {
        var initiative = new InitiativeEntity
        {
            DomainOfInfluenceType = domainOfInfluenceType,
            Description = description,
            SubTypeId = subTypeId,
            Bfs = bfs,
            CollectionCount = new CollectionCountEntity(),
            SignatureSheetTemplateGenerated = true,
        };

        ValidateInitiative(initiative);

        initiative.Type = CollectionType.Initiative;
        initiative.State = CollectionState.InPreparation;
        initiative.IsElectronicSubmission = true;

        initiative.Bfs = initiative.DomainOfInfluenceType switch
        {
            DomainOfInfluenceType.Ct => await _accessControlListDoiRepository.GetSingleBfsForDoiType(AclDomainOfInfluenceType.Ct),
            DomainOfInfluenceType.Ch => await _accessControlListDoiRepository.GetSingleBfsForDoiType(AclDomainOfInfluenceType.Ch),
            _ => initiative.Bfs,
        };

        if (initiative.DomainOfInfluenceType is DomainOfInfluenceType.Ch or DomainOfInfluenceType.Ct)
        {
            var subType = await _initiativeSubTypeRepository.Query()
                .Where(x => x.Id == subTypeId!.Value && x.DomainOfInfluenceType == initiative.DomainOfInfluenceType)
                .FirstOrDefaultAsync()
                ?? throw new EntityNotFoundException(nameof(InitiativeSubTypeEntity), subTypeId!.Value);

            initiative.MinSignatureCount = subType.MinSignatureCount;
            initiative.MaxElectronicSignatureCount = subType.MaxElectronicSignatureCount;
        }
        else if (initiative.DomainOfInfluenceType == DomainOfInfluenceType.Mu)
        {
            var domainOfInfluence = await _accessControlListDoiRepository.Query()
                .Where(x => x.Bfs == initiative.Bfs)
                .FirstOrDefaultAsync();

            if (domainOfInfluence == null)
            {
                throw new ValidationException("No domain of influence with this municipality id found.");
            }

            if (!domainOfInfluence.ECollectingEnabled)
            {
                throw new ValidationException("Domain of Influence has eCollecting not enabled.");
            }

            initiative.MinSignatureCount = domainOfInfluence.ECollectingInitiativeMinSignatureCount.GetValueOrDefault();
            initiative.MaxElectronicSignatureCount = (int)Math.Round(initiative.MinSignatureCount * domainOfInfluence.ECollectingInitiativeMaxElectronicSignaturePercent.GetValueOrDefault() / 100.0);
        }

        _permissionService.SetCreated(initiative);
        _permissionService.SetCreated(initiative.CollectionCount);
        await _initiativeRepository.Create(initiative);
        return initiative.Id;
    }

    public async Task<Guid> SetInPreparation(string governmentDecisionNumber)
    {
        var existingInitiative = await _initiativeRepository.Query()
            .Where(x => x.GovernmentDecisionNumber.Equals(governmentDecisionNumber) && x.AdmissibilityDecisionState != null)
            .FirstOrDefaultAsync()
            ?? throw new InitiativeNotFoundException(governmentDecisionNumber);

        if (existingInitiative.State != CollectionState.PreRecorded)
        {
            throw new InitiativeAlreadyInPreparationException(governmentDecisionNumber);
        }

        if (existingInitiative.AdmissibilityDecisionState is AdmissibilityDecisionState.Rejected)
        {
            throw new InitiativeAdmissibilityDecisionRejectedException();
        }

        await _initiativeRepository.AuditedUpdate(existingInitiative, () =>
        {
            existingInitiative.State = CollectionState.InPreparation;
            existingInitiative.IsElectronicSubmission = true;

            _permissionService.SetCreated(existingInitiative);
        });

        return existingInitiative.Id;
    }

    public async Task<Initiative> Get(
        Guid id,
        bool includeCommitteeDescription = false,
        bool includeIsSigned = false)
    {
        IQueryable<InitiativeEntity> query = _initiativeRepository.Query()
            .WhereCanReadOrIsPastRegistered(_permissionService)
            .WhereDoiTypeIsEnabled(_config.EnabledDomainOfInfluenceTypes)
            .Include(x => x.CollectionCount)
            .Include(x => x.SubType)
            .IncludePermission(_permissionService.UserId)

            // include files but not the file content
            .Include(x => x.Image)
            .Include(x => x.Logo)
            .Include(x => x.SignatureSheetTemplate);

        if (includeCommitteeDescription)
        {
            query = query.Include(x => x.CommitteeMembers.Where(y => y.ApprovalState == InitiativeCommitteeMemberApprovalState.Approved).OrderBy(y => y.SortIndex));
        }

        var initiativeEntity = await query.FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), id);

        var initiative = Mapper.MapToInitiative(initiativeEntity);
        SetStateAndPermission(initiative);
        if (includeCommitteeDescription)
        {
            await SetCommitteeDescription(initiative);
        }

        if (includeIsSigned)
        {
            initiative.IsSigned = await _initiativeSignatureService.IsCollectionSigned(initiative);
            initiative.SignAcceptedAcrs = _config.Acr.SignCollection;
        }

        return initiative;
    }

    public async Task Update(Guid id, Guid? subTypeId, string description, string wording, string reason, CollectionAddress address, string link)
    {
        var existingInitiative = await _initiativeRepository.Query()
                             .WhereCanEdit(_permissionService)
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), id);

        if (existingInitiative.LockedFields.Wording && !existingInitiative.Wording.Equals(wording, StringComparison.Ordinal))
        {
            throw new CannotEditLockedFieldException(nameof(existingInitiative.Wording));
        }

        if (existingInitiative.LockedFields.Description && !existingInitiative.Description.Equals(description, StringComparison.Ordinal))
        {
            throw new CannotEditLockedFieldException(nameof(existingInitiative.Description));
        }

        if (!CollectionPermissions.CanEditSubType(existingInitiative) && existingInitiative.SubTypeId != subTypeId)
        {
            throw new CannotEditLockedFieldException(nameof(existingInitiative.SubTypeId));
        }

        await _initiativeRepository.AuditedUpdate(existingInitiative, async () =>
        {
            existingInitiative.SubTypeId = subTypeId;
            existingInitiative.Description = description;
            existingInitiative.Wording = wording;
            existingInitiative.Reason = reason;
            existingInitiative.Address = address;
            existingInitiative.Link = link;

            ValidateInitiative(existingInitiative);

            if (existingInitiative.DomainOfInfluenceType is DomainOfInfluenceType.Ch or DomainOfInfluenceType.Ct)
            {
                var hasSubType = await _initiativeSubTypeRepository.Query()
                                  .Where(x => x.Id == existingInitiative.SubTypeId!.Value && x.DomainOfInfluenceType == existingInitiative.DomainOfInfluenceType)
                                  .AnyAsync();
                if (!hasSubType)
                {
                    throw new EntityNotFoundException(nameof(InitiativeSubTypeEntity), existingInitiative.SubTypeId!.Value);
                }
            }

            _permissionService.SetModified(existingInitiative);
        });
    }

    public async Task Submit(Guid id)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var initiative = await _initiativeRepository.Query()
                             .Include(x => x.Permissions)
                             .WhereCanSubmit(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), id);

        var validationSummary = await _initiativeValidationService.ValidateForSubmission(initiative);
        validationSummary.EnsureIsValid();

        if (initiative.SignatureSheetTemplateGenerated)
        {
            await _collectionFilesService.GenerateSignatureSheetTemplate(initiative);
        }

        initiative.AdmissibilityDecisionState = AdmissibilityDecisionState.Open;
        initiative.State = initiative.State switch
        {
            CollectionState.InPreparation => CollectionState.Submitted,
            CollectionState.ReturnedForCorrection => CollectionState.UnderReview,
            _ => throw new InvalidOperationException($"Unexpected initiative state: {initiative.State}"),
        };

        _permissionService.SetModified(initiative);
        await _dataContext.SaveChangesAsync();

        await _collectionService.AddStateChangedMessage(initiative);

        await transaction.CommitAsync();
    }

    public async Task FlagForReview(Guid id)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var initiative = await _initiativeRepository.Query()
                             .Include(x => x.Permissions)
                             .WhereCanFlagForReview(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), id);

        var validationResults = await _initiativeValidationService.ValidateForSubmission(initiative);
        validationResults.EnsureIsValid();

        if (initiative.SignatureSheetTemplateGenerated)
        {
            await _collectionFilesService.GenerateSignatureSheetTemplate(initiative);
        }

        initiative.State = CollectionState.UnderReview;
        _permissionService.SetModified(initiative);
        await _dataContext.SaveChangesAsync();

        await _collectionService.AddStateChangedMessage(initiative);

        await transaction.CommitAsync();
    }

    public async Task Register(Guid id)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var initiative = await _initiativeRepository.Query()
                             .Include(x => x.Permissions)
                             .WhereCanRegister(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), id);

        initiative.State = CollectionState.Registered;
        _permissionService.SetModified(initiative);
        await _dataContext.SaveChangesAsync();

        await _collectionService.AddStateChangedMessage(initiative);

        await transaction.CommitAsync();
    }

    private void ValidateInitiative(InitiativeEntity initiative)
    {
        if (!_config.EnabledDomainOfInfluenceTypes.Contains(initiative.DomainOfInfluenceType!.Value))
        {
            throw new ValidationException($"Domain of influence type {initiative.DomainOfInfluenceType} is not enabled.");
        }

        if (initiative.DomainOfInfluenceType is DomainOfInfluenceType.Ch or DomainOfInfluenceType.Ct &&
            !initiative.SubTypeId.HasValue)
        {
            throw new ValidationException("Initiative on federal and cantonal level must have a sub-type.");
        }

        if (initiative.DomainOfInfluenceType == DomainOfInfluenceType.Mu &&
            string.IsNullOrEmpty(initiative.Bfs))
        {
            throw new ValidationException("Initiative on communal level must have a municipality id.");
        }
    }

    private void SetStatesAndPermissions(IEnumerable<Initiative> initiatives)
    {
        var utcNow = _timeProvider.GetUtcNowDateTime();
        foreach (var initiative in initiatives)
        {
            SetStateAndPermission(initiative, utcNow);
        }
    }

    private void SetStateAndPermission(Initiative initiative, DateTime? utcNow = null)
    {
        initiative.SetPeriodState(utcNow ?? _timeProvider.GetUtcNowDateTime());
        _collectionService.LoadPermission(initiative);
        _collectionService.SetCollectionCount(initiative);
    }

    private async Task SetCommitteeDescription(Initiative initiative)
    {
        if (initiative.CommitteeMembers.Count == 0)
        {
            initiative.CommitteeDescription = string.Empty;
            return;
        }

        var domainOfInfluencesByBfs = await _accessControlListDoiRepository.Query()
            .Where(x => !string.IsNullOrWhiteSpace(x.Bfs))
            .GroupBy(x => x.Bfs)
            .ToDictionaryAsync(x => x.Key!, x => x.First());

        var sb = new StringBuilder(initiative.CommitteeMembers.Count * 20);

        foreach (var member in initiative.CommitteeMembers)
        {
            sb.Append(member.PoliticalFirstName);
            sb.Append(' ');
            sb.Append(member.PoliticalLastName);

            if (!string.IsNullOrWhiteSpace(member.PoliticalDuty))
            {
                sb.Append(" (");
                sb.Append(member.PoliticalDuty);
                sb.Append(')');
            }

            if (domainOfInfluencesByBfs.TryGetValue(member.PoliticalBfs, out var domainOfInfluence))
            {
                sb.Append(", ");
                sb.Append(domainOfInfluence.Name);
            }

            sb.Append("; ");
        }

        // remove last '; '
        sb.Remove(sb.Length - 2, 2);
        initiative.CommitteeDescription = sb.ToString();
    }
}
