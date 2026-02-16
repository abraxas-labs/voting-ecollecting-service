// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Admin;
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

public class ReferendumService : IReferendumService
{
    private readonly IReferendumRepository _referendumRepository;
    private readonly IPermissionService _permissionService;
    private readonly CoreAppConfig _config;
    private readonly CollectionService _collectionService;
    private readonly IDataContext _dataContext;
    private readonly ReferendumValidationService _validationService;
    private readonly CollectionFilesService _collectionFilesService;
    private readonly IDecreeRepository _decreeRepository;
    private readonly TimeProvider _timeProvider;
    private readonly IAdminAdapter _admin;
    private readonly ReferendumSignService _referendumSignService;

    public ReferendumService(
        IReferendumRepository referendumRepository,
        IPermissionService permissionService,
        CoreAppConfig config,
        CollectionService collectionService,
        IDataContext dataContext,
        ReferendumValidationService validationService,
        CollectionFilesService collectionFilesService,
        IDecreeRepository decreeRepository,
        TimeProvider timeProvider,
        IAdminAdapter admin,
        ReferendumSignService referendumSignService)
    {
        _referendumRepository = referendumRepository;
        _permissionService = permissionService;
        _config = config;
        _collectionService = collectionService;
        _dataContext = dataContext;
        _validationService = validationService;
        _collectionFilesService = collectionFilesService;
        _decreeRepository = decreeRepository;
        _timeProvider = timeProvider;
        _admin = admin;
        _referendumSignService = referendumSignService;
    }

    public async Task<Guid> Create(string description, Guid? decreeId)
    {
        var ownerPermission = new CollectionPermissionEntity
        {
            FirstName = _permissionService.UserFirstName,
            LastName = _permissionService.UserLastName,
            IamFirstName = _permissionService.UserFirstName,
            IamLastName = _permissionService.UserLastName,
            IamUserId = _permissionService.UserId,
            Email = _permissionService.UserEmail!,
            Role = CollectionPermissionRole.Owner,
            State = CollectionPermissionState.Accepted,
        };
        var referendum = new ReferendumEntity
        {
            Description = description,
            DecreeId = decreeId,
            CollectionCount = new CollectionCountEntity(),
            SignatureSheetTemplateGenerated = true,
            Permissions = [ownerPermission],
        };

        await ValidateReferendum(referendum);

        referendum.Type = CollectionType.Referendum;
        referendum.IsElectronicSubmission = true;
        referendum.State = CollectionState.InPreparation;

        await SetDecreeData(referendum);

        _permissionService.SetCreated(referendum);
        _permissionService.SetCreated(referendum.CollectionCount);
        _permissionService.SetCreated(ownerPermission);
        await _referendumRepository.Create(referendum);
        return referendum.Id;
    }

    public async Task<Guid> SetInPreparation(string secureIdNumber)
    {
        var referendum = await _referendumRepository.Query()
            .WhereDoiTypeIsEnabled(_config.EnabledDomainOfInfluenceTypes)
            .Where(x => secureIdNumber.Equals(x.SecureIdNumber) && !x.IsElectronicSubmission)
            .FirstOrDefaultAsync()
            ?? throw new ReferendumNotFoundException(secureIdNumber);

        await _referendumRepository.AuditedUpdate(
            referendum,
            () =>
            {
                _permissionService.SetCreated(referendum);
                var ownerPermission = new CollectionPermissionEntity
                {
                    FirstName = _permissionService.UserFirstName,
                    LastName = _permissionService.UserLastName,
                    IamFirstName = _permissionService.UserFirstName,
                    IamLastName = _permissionService.UserLastName,
                    IamUserId = _permissionService.UserId,
                    Email = _permissionService.UserEmail!,
                    Role = CollectionPermissionRole.Owner,
                    State = CollectionPermissionState.Accepted,
                };
                _permissionService.SetCreated(ownerPermission);
                referendum.Permissions ??= [];
                referendum.Permissions.Add(ownerPermission);

                if (referendum.State != CollectionState.PreRecorded)
                {
                    throw new ReferendumAlreadyInPreparationException(secureIdNumber);
                }

                referendum.State = CollectionState.InPreparation;
            },
            2); // permission and collection
        return referendum.Id;
    }

    public async Task<Referendum> Get(
        Guid id,
        bool includeIsSigned = false)
    {
        var query = _referendumRepository.Query();
        if (includeIsSigned)
        {
            // load other collections of same decree to check existing signature on same decree
            query = query
                .AsNoTrackingWithIdentityResolution()
                .AsSplitQuery()
                .Include(x => x.Decree!.Collections.Where(y => y.State != CollectionState.InPreparation && y.State != CollectionState.PreparingForCollection));
        }
        else
        {
            query = query.Include(x => x.Decree);
        }

        var referendumEntity = await query
            .WhereCanReadOrIsPastRegistered(_permissionService)
            .WhereDoiTypeIsEnabled(_config.EnabledDomainOfInfluenceTypes)
            .Include(x => x.CollectionCount)
            .IncludePermission(_permissionService.UserId)

            // include files but not the file content
            .Include(x => x.Image)
            .Include(x => x.Logo)
            .Include(x => x.SignatureSheetTemplate)
            .FirstOrDefaultAsync(x => x.Id == id) ?? throw new EntityNotFoundException(nameof(ReferendumEntity), id);

        var referendum = Mapper.MapToReferendum(referendumEntity);
        referendum.SetPeriodState(_timeProvider.GetUtcTodayDateOnly());
        _collectionService.LoadPermission(referendum);
        _collectionService.SetCollectionCount(referendum);

        if (includeIsSigned)
        {
            (referendum.IsSigned, referendum.IsDecreeSigned, referendum.SignatureType) = await _referendumSignService.IsReferendumOrDecreeSigned(referendum);
            referendum.SignAcceptedAcrs = _config.Acr.SignCollection;
        }

        return referendum;
    }

    public async Task Update(Guid id, string description, string reason, CollectionAddress address, string membersCommittee, string link)
    {
        var existingReferendum = await _referendumRepository.Query()
                             .WhereCanEdit(_permissionService)
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(ReferendumEntity), id);

        await _referendumRepository.AuditedUpdate(existingReferendum, () =>
        {
            existingReferendum.Description = description;
            existingReferendum.Reason = reason;
            existingReferendum.Address = address;
            existingReferendum.MembersCommittee = membersCommittee;
            existingReferendum.Link = link;

            _permissionService.SetModified(existingReferendum);
        });
    }

    public async Task Submit(Guid id)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var referendum = await _referendumRepository.Query()
                             .WhereCanSubmit(_permissionService)
                             .Include(x => x.Permissions)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(ReferendumEntity), id);

        var validationSummary = await _validationService.ValidateForSubmission(referendum);
        validationSummary.EnsureIsValid();

        if (referendum.SignatureSheetTemplateGenerated)
        {
            await _collectionFilesService.GenerateSignatureSheetTemplate(referendum);
        }

        referendum.State = CollectionState.PreparingForCollection;

        _permissionService.SetModified(referendum);
        await _dataContext.SaveChangesAsync();

        await _collectionService.AddStateChangedMessage(referendum);

        await transaction.CommitAsync();

        await _admin.NotifyPreparingForCollection();
    }

    public async Task<(List<Decree> Decrees, List<Referendum> ReferendumsWithoutDecree)> ListMy()
    {
        var decreeEntities = await _decreeRepository.Query()
            .WhereCanReadAnyCollection(_permissionService)
            .WhereDoiTypeIsEnabled(_config.EnabledDomainOfInfluenceTypes)
            .IncludeReadableCollections(_permissionService.UserId)
            .Include(x => x.Collections)
            .ThenIncludePermission(_permissionService.UserId)
            .Include(x => x.Collections)
            .ThenInclude(x => x.CollectionCount)
            .OrderByDescending(x => x.CollectionStartDate)
            .ToListAsync();

        var decrees = Mapper.MapToDecrees(decreeEntities);
        SetDomainData(decrees);
        foreach (var decree in decrees)
        {
            foreach (var referendum in decree.Referendums)
            {
                // set mapped decree
                referendum.Decree = decree;
            }
        }

        var referendumEntities = await _referendumRepository.Query()
            .WhereCanRead(_permissionService)
            .Include(x => x.CollectionCount)
            .IncludePermission(_permissionService.UserId)
            .Include(x => x.Decree)
            .Where(x => !x.DecreeId.HasValue)
            .ToListAsync();

        var referendumsWithoutDecree = Mapper.MapToReferendums(referendumEntities);
        foreach (var referendum in referendumsWithoutDecree)
        {
            referendum.SetPeriodState(_timeProvider.GetUtcTodayDateOnly());
            _collectionService.LoadPermission(referendum);
            _collectionService.SetCollectionCount(referendum);
        }

        return (decrees, referendumsWithoutDecree);
    }

    public async Task<Dictionary<DomainOfInfluenceType, List<Decree>>> ListDecreesEligibleForReferendumByDoiType(IReadOnlySet<DomainOfInfluenceType>? doiTypes, string? bfs, bool includeReferendums)
    {
        var query = _decreeRepository
            .Query()
            .WhereInCollectionOrPublished(_timeProvider.GetUtcTodayDateOnly())
            .WhereDoiTypeIsEnabled(_config.EnabledDomainOfInfluenceTypes);

        if (doiTypes?.Count > 0)
        {
            query = query.Where(x => doiTypes.Contains(x.DomainOfInfluenceType));
        }

        if (!string.IsNullOrWhiteSpace(bfs))
        {
            query = query.Where(x => x.Bfs == bfs);
        }

        if (includeReferendums)
        {
            query = query
                .IncludeInCollectionAndReadableCollections(_permissionService.UserId)
                .Include(x => x.Collections)
                .ThenIncludePermission(_permissionService.UserId)
                .Include(x => x.Collections)
                .ThenInclude(x => x.CollectionCount);
        }

        var decreesByDoiType = await query
            .GroupBy(x => x.DomainOfInfluenceType)
            .ToDictionaryAsync(x => x.Key, x => x.OrderByDescending(y => y.CollectionStartDate).ToList());

        var referendumsCountByDecreeId = await _referendumRepository.Query()
            .Where(x => x.DecreeId.HasValue)
            .GroupBy(x => x.DecreeId)
            .ToDictionaryAsync(x => x.Key!.Value, x => x.Count());

        var decreeIdsWithReferendumFromUser = await _referendumRepository.Query()
            .Where(x => x.DecreeId.HasValue && x.AuditInfo.CreatedById == _permissionService.UserId)
            .Select(x => x.DecreeId!.Value)
            .ToHashSetAsync();

        return _config.EnabledDomainOfInfluenceTypes
            .OrderBy(x => x)
            .ToDictionary(x => x, x =>
            {
                var decrees = Mapper.MapToDecrees(decreesByDoiType.GetValueOrDefault(x) ?? []);
                SetDomainData(decrees);
                SetUserPermissions(decrees, referendumsCountByDecreeId, decreeIdsWithReferendumFromUser);
                return decrees;
            });
    }

    public async Task UpdateDecree(Guid id, Guid decreeId)
    {
        var existingReferendum = await _referendumRepository.Query()
                             .WhereCanEdit(_permissionService)
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(ReferendumEntity), id);

        await _referendumRepository.AuditedUpdate(existingReferendum, async () =>
        {
            if (existingReferendum.DecreeId != decreeId)
            {
                existingReferendum.DecreeId = decreeId;
                await ValidateReferendum(existingReferendum);
                await SetDecreeData(existingReferendum);
            }

            _permissionService.SetModified(existingReferendum);
        });
    }

    private async Task ValidateReferendum(ReferendumEntity referendum)
    {
        if (!referendum.DecreeId.HasValue)
        {
            return;
        }

        var query = _referendumRepository.Query().Where(x => x.DecreeId == referendum.DecreeId);
        var count = await query.CountAsync();
        if (count >= _config.MaxAllowedReferendumsPerDecree)
        {
            throw new MaxReferendumsOnDecreeReachedException(referendum.DecreeId.Value, _config.MaxAllowedReferendumsPerDecree);
        }

        if (await query.WhereCanWrite(_permissionService).AnyAsync())
        {
            throw new UserHasAlreadyAReferendumOnDecreeException(referendum.DecreeId.Value);
        }
    }

    private void SetDomainData(IEnumerable<Decree> decrees)
    {
        var today = _timeProvider.GetUtcTodayDateOnly();
        foreach (var decree in decrees)
        {
            decree.SetPeriodState(today);

            foreach (var referendum in decree.Referendums)
            {
                referendum.SetPeriodState(today);
                _collectionService.LoadPermission(referendum);
                _collectionService.SetCollectionCount(referendum);
            }
        }
    }

    private void SetUserPermissions(IEnumerable<Decree> decrees, Dictionary<Guid, int> referendumsCountByDecreeId, HashSet<Guid> decreeIdsWithReferendumFromUser)
    {
        foreach (var decree in decrees)
        {
            decree.UserPermissions = GetDecreeUserPermissions(decree, referendumsCountByDecreeId, decreeIdsWithReferendumFromUser);
        }
    }

    private DecreeUserPermissions GetDecreeUserPermissions(Decree decree, Dictionary<Guid, int> referendumsCountByDecreeId, HashSet<Guid> decreeIdsWithReferendumFromUser)
    {
        var hasMaximumReferendumsBeenReached = referendumsCountByDecreeId.TryGetValue(decree.Id, out var count) && count >= _config.MaxAllowedReferendumsPerDecree;
        var canCreateReferendum = !decreeIdsWithReferendumFromUser.Contains(decree.Id);

        return new DecreeUserPermissions(canCreateReferendum, hasMaximumReferendumsBeenReached);
    }

    private async Task SetDecreeData(ReferendumEntity referendum)
    {
        if (!referendum.DecreeId.HasValue)
        {
            return;
        }

        var decree = await _decreeRepository.GetByKey(referendum.DecreeId.Value)
            ?? throw new EntityNotFoundException(nameof(DecreeEntity), referendum.DecreeId);

        referendum.Bfs = decree.Bfs;
        referendum.DomainOfInfluenceType = decree.DomainOfInfluenceType;
        referendum.MaxElectronicSignatureCount = decree.MaxElectronicSignatureCount;
    }
}
