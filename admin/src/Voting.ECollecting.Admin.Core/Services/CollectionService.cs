// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Core.Mappings;
using Voting.ECollecting.Admin.Core.Permissions;
using Voting.ECollecting.Admin.Core.Services.Crypto;
using Voting.ECollecting.Admin.Core.Utils;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Core.Resources;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Domain.Models;
using ICollection = Voting.ECollecting.Admin.Domain.Models.ICollection;
using IPermissionService = Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam.IPermissionService;

namespace Voting.ECollecting.Admin.Core.Services;

public class CollectionService : ICollectionService
{
    private const string SecureIdNumberUniqueConstraintName = "IX_Collections_SecureIdNumber";
    private const int MaxSecretIdNumbersRetries = 10;

    private readonly ICollectionMessageRepository _messageRepository;
    private readonly IPermissionService _permissionService;
    private readonly IUserNotificationService _userNotificationService;
    private readonly ICollectionRepository _collectionRepository;
    private readonly CollectionCryptoService _collectionCryptoService;
    private readonly ICollectionMessageRepository _collectionMessageRepository;
    private readonly IUserNotificationService _coreUserNotificationService;
    private readonly IDataContext _db;
    private readonly TimeProvider _timeProvider;
    private readonly ICollectionMunicipalityRepository _collectionMunicipalityRepository;
    private readonly AccessControlListService _accessControlListService;
    private readonly IDecreeRepository _decreeRepository;
    private readonly IInitiativeRepository _initiativeRepository;
    private readonly ICollectionSignatureSheetRepository _collectionSignatureSheetRepository;

    public CollectionService(
        ICollectionMessageRepository messageRepository,
        IPermissionService permissionService,
        IUserNotificationService userNotificationService,
        ICollectionRepository collectionRepository,
        CollectionCryptoService collectionCryptoService,
        ICollectionMessageRepository collectionMessageRepository,
        IUserNotificationService coreUserNotificationService,
        IDataContext db,
        TimeProvider timeProvider,
        ICollectionMunicipalityRepository collectionMunicipalityRepository,
        AccessControlListService accessControlListService,
        IDecreeRepository decreeRepository,
        IInitiativeRepository initiativeRepository,
        ICollectionSignatureSheetRepository collectionSignatureSheetRepository)
    {
        _messageRepository = messageRepository;
        _permissionService = permissionService;
        _userNotificationService = userNotificationService;
        _collectionRepository = collectionRepository;
        _collectionCryptoService = collectionCryptoService;
        _collectionMessageRepository = collectionMessageRepository;
        _coreUserNotificationService = coreUserNotificationService;
        _db = db;
        _timeProvider = timeProvider;
        _collectionMunicipalityRepository = collectionMunicipalityRepository;
        _accessControlListService = accessControlListService;
        _decreeRepository = decreeRepository;
        _initiativeRepository = initiativeRepository;
        _collectionSignatureSheetRepository = collectionSignatureSheetRepository;
    }

    public async Task<(List<CollectionMessageEntity> Messages, bool InformalReviewRequested)> ListMessages(Guid collectionId)
    {
        var collection = await _collectionRepository.Query()
                             .WhereCanReadMessages(_permissionService)
                             .FirstOrDefaultAsync(x => x.Id == collectionId)
                         ?? throw new EntityNotFoundException(typeof(CollectionBaseEntity), collectionId);

        var messages = await _messageRepository.Query()
            .AsSplitQuery()
            .Where(x => x.CollectionId == collectionId)
            .Include(x => x.Collection)
            .OrderBy(x => x.AuditInfo.CreatedAt)
            .ToListAsync();

        return (messages, collection.InformalReviewRequested);
    }

    public async Task<CollectionMessageEntity> AddMessage(Guid collectionId, string content)
    {
        var collection = await _collectionRepository.Query()
                             .WhereCanCreateMessage(_permissionService)
                             .Include(x => x.Permissions)
                             .FirstOrDefaultAsync(x => x.Id == collectionId)
                         ?? throw new EntityNotFoundException(typeof(CollectionBaseEntity), collectionId);
        var msg = new CollectionMessageEntity { Content = content, CollectionId = collectionId };
        _permissionService.SetCreated(msg);
        await _messageRepository.Create(msg);
        await _userNotificationService.ScheduleNotification(collection, UserNotificationType.MessageAdded);
        return msg;
    }

    public async Task NotifyPreparingForCollection()
    {
        await using var transaction = await _db.BeginTransaction();

        var collections = await _collectionRepository.FetchAndLockPreparingForCollection();

        await _collectionRepository.AuditedUpdateRange(collections, async collection =>
        {
            EnsureCanGenerateKeys(collection);

            var success = await PrepareForCollection(collection);
            collection.State = success ? CollectionState.EnabledForCollection : CollectionState.PreparingForCollection;
        });
        await transaction.CommitAsync();
    }

    public async Task<IReadOnlyDictionary<DomainOfInfluenceType, CollectionsGroup>> ListForDeletionByDoiType(
        IReadOnlySet<DomainOfInfluenceType>? doiTypes,
        string? bfs,
        CollectionControlSignFilter filter)
    {
        var decrees = await LoadDecreesForDeletionByDoiType(doiTypes, bfs, filter);
        var initiatives = await ListInitiativesForDeletionByDoiType(doiTypes, bfs, filter);

        if (doiTypes == null || doiTypes.Count == 0)
        {
            doiTypes = Enum.GetValues<DomainOfInfluenceType>()
                .Where(x => x != DomainOfInfluenceType.Unspecified)
                .ToHashSet();
        }

        return doiTypes.ToDictionary(x => x, x => new CollectionsGroup(
                initiatives.GetValueOrDefault(x) ?? [],
                decrees.GetValueOrDefault(x) ?? []));
    }

    public async Task DeleteWithdrawn(Guid collectionId)
    {
        var existingCollection = await _collectionRepository
            .Query()
            .WhereCanDeleteWithdrawn(_permissionService)
            .FirstOrDefaultAsync(x => x.Id == collectionId)
            ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), collectionId);

        await _collectionRepository.AuditedDelete(existingCollection);
    }

    public async Task<CollectionMessageEntity> FinishInformalReview(Guid collectionId)
    {
        await using var transaction = await _db.BeginTransaction();

        var collection = await _collectionRepository.Query()
                             .WhereCanEdit(_permissionService)
                             .AsTracking()
                             .Include(x => x.Permissions)
                             .FirstOrDefaultAsync(x => x.Id == collectionId)
                         ?? throw new EntityNotFoundException(typeof(CollectionBaseEntity), collectionId);

        if (!collection.InformalReviewRequested)
        {
            throw new ValidationException($"Informal review is already finished for this collection {collectionId}");
        }

        collection.InformalReviewRequested = false;
        _permissionService.SetModified(collection);
        await _db.SaveChangesAsync();

        var msg = await AddMessage(collection.Id, Strings.UserNotification_InformalReviewWithdrawn);

        await transaction.CommitAsync();
        return msg;
    }

    public async Task<List<CollectionPermission>> ListPermissions(Guid collectionId)
    {
        var collection = await _collectionRepository.Query()
                        .WhereCanReadPermissions(_permissionService)
                        .Include(x => x.Permissions!
                            .Where(p => p.State == CollectionPermissionState.Accepted
                                        && p.Role == CollectionPermissionRole.Deputy)
                            .OrderBy(p => p.LastName)
                            .ThenBy(p => p.FirstName))
                        .FirstOrDefaultAsync(x => x.Id == collectionId) ??
                    throw new EntityNotFoundException(nameof(CollectionBaseEntity), collectionId);

        var collectionPermissions = Mapper.MapToCollectionPermissions(collection.Permissions!);
        collectionPermissions.Insert(0, new CollectionPermission
        {
            FullName = collection.AuditInfo.CreatedByName,
            Email = collection.AuditInfo.CreatedByEmail ?? string.Empty,
            Role = CollectionPermissionRole.Owner,
        });

        return collectionPermissions;
    }

    public async Task<CollectionUserPermissions> SubmitSignatureSheets(Guid collectionId)
    {
        await using var transaction = await _db.BeginTransaction();

        var collection = await _collectionRepository.Query()
                             .WhereCanSubmitSignatureSheets(_permissionService)
                             .AsTracking()
                             .Include(x => x.Permissions)
                             .Include(x => x.Municipalities!.OrderBy(m => m.Bfs))
                             .FirstOrDefaultAsync(x => x.Id == collectionId)
                         ?? throw new EntityNotFoundException(typeof(CollectionBaseEntity), collectionId);

        collection.State = CollectionState.SignatureSheetsSubmitted;
        _permissionService.SetModified(collection);
        foreach (var municipality in collection.Municipalities!)
        {
            municipality.IsLocked = true;
            _permissionService.SetModified(municipality);
        }

        await _db.SaveChangesAsync();
        await _collectionSignatureSheetRepository.AuditedDeleteRange(
            q => q.Where(x => x.CollectionMunicipality!.CollectionId == collectionId && x.State == CollectionSignatureSheetState.Created));

        await AddStateChangedMessage(collection);

        await transaction.CommitAsync();
        collection.SetPeriodState(_timeProvider.GetUtcTodayDateOnly());
        return CollectionPermissions.Build(_permissionService, collection);
    }

    public async Task SetCommitteeAddress(Guid collectionId, CollectionAddress address)
    {
        await using var transaction = await _db.BeginTransaction();

        var collection = await _collectionRepository.Query()
            .WhereCanSetCommitteeAddress(_permissionService)
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == collectionId)
            ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), collectionId);

        if (collection.Address.IsComplete)
        {
            throw new ValidationException("Cannot set the address if it is already set.");
        }

        collection.Address = address;
        _permissionService.SetModified(collection);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    internal async Task CreateWithSecretIdNumber(CollectionBaseEntity collection)
    {
        Debug.Assert(!collection.IsElectronicSubmission, "Secret id numbers are only used for paper collections");

        // instead of prefetching all used number we rely on the database to enforce uniqueness
        // a collision is very unlikely anyway, but if it happens, we just retry with another number.
        var usedSecretIdNumbers = new HashSet<string>();
        for (var i = 0; i < MaxSecretIdNumbersRetries; i++)
        {
            collection.SecureIdNumber = RandomUtil.GenerateSecureIdNumber(usedSecretIdNumbers);

            try
            {
                await _collectionRepository.Create(collection);
                return;
            }
            catch (Exception e) when (e.InnerException is PostgresException { ConstraintName: SecureIdNumberUniqueConstraintName })
            {
                usedSecretIdNumbers.Add(collection.SecureIdNumber!);
            }
        }

        throw new InvalidOperationException("Could not create unique secret id number");
    }

    internal void LoadPermissions<T>(IEnumerable<T> collections)
        where T : CollectionBaseEntity, ICollection
    {
        foreach (var collection in collections)
        {
            LoadPermission(collection);
        }
    }

    internal void LoadPermission<T>(T collection)
        where T : CollectionBaseEntity, ICollection
    {
        collection.UserPermissions = CollectionPermissions.Build(_permissionService, collection);
    }

    /// <summary>
    /// Sets the collection count on the domain collection.
    /// It is set to the electronic count if the collection state is enabled or later and not withdrawn.
    /// If it has ended or is past enabled and the user has permissions, the total count is set too.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <typeparam name="T">The type of collection.</typeparam>
    internal void SetCollectionCount<T>(T collection)
        where T : CollectionBaseEntity, ICollection
    {
        if (collection.CollectionCount == null)
        {
            return;
        }

        if (!collection.State.IsEnabledForCollectionOrEnded())
        {
            collection.AttestedCollectionCount = null;
            return;
        }

        if (collection.UserPermissions?.CanReadTotalCount != true)
        {
            collection.AttestedCollectionCount = new NullableCollectionCount
            {
                Id = collection.CollectionCount.Id,
                CollectionId = collection.CollectionCount.CollectionId,
                ElectronicCitizenCount = collection.CollectionCount.ElectronicCitizenCount,
            };
            return;
        }

        collection.AttestedCollectionCount = new NullableCollectionCount
        {
            Id = collection.CollectionCount.Id,
            CollectionId = collection.CollectionCount.CollectionId,
            ElectronicCitizenCount = collection.CollectionCount.ElectronicCitizenCount,
            TotalCitizenCount = collection.CollectionCount.TotalCitizenCount,
        };
    }

    internal async Task AddStateChangedMessage(CollectionBaseEntity collection)
    {
        var msg = CreateCollectionMessageEntity(collection);
        await _collectionMessageRepository.Create(msg);
        await _coreUserNotificationService.ScheduleNotification(collection, UserNotificationType.StateChanged);
    }

    internal async Task AddStateChangedMessages(IReadOnlyCollection<CollectionBaseEntity> collections)
    {
        var messages = collections.Select(CreateCollectionMessageEntity).ToList();
        await _collectionMessageRepository.CreateRange(messages);

        foreach (var collection in collections)
        {
            await _coreUserNotificationService.ScheduleNotification(collection, UserNotificationType.StateChanged);
        }
    }

    internal async Task<bool> PrepareForCollection(CollectionBaseEntity collection)
    {
        var collectionKey = await _collectionCryptoService.GenerateKey(collection.Id);
        if (!collectionKey.Success)
        {
            return false;
        }

        collection.MacKeyId = collectionKey.MacKeyId;
        collection.EncryptionKeyId = collectionKey.EncryptionKeyId;

        if (collection is ReferendumEntity referendum)
        {
            var collectionPeriod = await _decreeRepository.Query()
                .Where(x => x.Id == referendum.DecreeId!.Value)
                .Select(x => new { x.CollectionStartDate, x.CollectionEndDate })
                .FirstOrDefaultAsync()
                ?? throw new EntityNotFoundException(nameof(Decree), referendum.DecreeId?.ToString() ?? "<no decree set>");
            collection.CollectionStartDate = collectionPeriod.CollectionStartDate;
            collection.CollectionEndDate = collectionPeriod.CollectionEndDate;
        }

        // A collection that was initially pre-recorded already have collection municipalities.
        var hasAnyCollectionMunicipalities = await _collectionMunicipalityRepository.Query().AnyAsync(x => x.CollectionId == collection.Id);
        if (!hasAnyCollectionMunicipalities)
        {
            await CreateCollectionMunicipalities(collection.Id, collection.Bfs!);
        }

        return true;
    }

    private CollectionMessageEntity CreateCollectionMessageEntity(CollectionBaseEntity collection)
    {
        var key = $"{collection.State.GetType().Name}.{collection.State.ToString()}";
        var localizedStateValue = Strings.ResourceManager.GetString(key);

        var content = string.Format(Strings.UserNotification_StateChanged, localizedStateValue);
        var msg = new CollectionMessageEntity { Content = content, CollectionId = collection.Id };
        _permissionService.SetCreated(msg);
        return msg;
    }

    private void EnsureCanGenerateKeys(CollectionBaseEntity collection)
    {
        if (collection.State != CollectionState.PreparingForCollection)
        {
            throw new InvalidOperationException($"Collection {collection.Id} state {collection.State} does not allow Pkcs11 key generation");
        }
    }

    private async Task<Dictionary<DomainOfInfluenceType, List<Decree>>> LoadDecreesForDeletionByDoiType(IReadOnlySet<DomainOfInfluenceType>? doiTypes, string? bfs, CollectionControlSignFilter filter)
    {
        var today = _timeProvider.GetUtcTodayDateOnly();
        var decreeQuery = _decreeRepository.Query();

        if (doiTypes?.Count > 0)
        {
            decreeQuery = decreeQuery.Where(x => doiTypes.Contains(x.DomainOfInfluenceType));
        }

        if (!string.IsNullOrEmpty(bfs))
        {
            decreeQuery = decreeQuery.Where(x => x.Bfs == bfs);
        }

        var decreeEntities = await decreeQuery
            .WhereCanAccessOwnBfs(_permissionService)
            .Where(x => filter == CollectionControlSignFilter.ReadyToDelete
                ? x.SensitiveDataExpiryDate <= today
                : x.SensitiveDataExpiryDate > today)
            .Include(x => x.Collections
                .OrderByDescending(y => y.CollectionEndDate).ThenBy(y => y.Description))
            .Include(x => x.Collections)
            .ThenInclude(x => x.CollectionCount)
            .GroupBy(x => x.DomainOfInfluenceType)
            .ToDictionaryAsync(x => x.Key, x => x.OrderByDescending(y => y.CollectionStartDate).ThenBy(y => y.Description).ToList());

        var decrees = decreeEntities.ToDictionary(
            x => x.Key,
            x => Mapper.MapToDecrees(x.Value));
        foreach (var decree in decrees.Values.SelectMany(x => x))
        {
            decree.SetPeriodState(today);

            foreach (var referendum in decree.Referendums)
            {
                referendum.SetPeriodState(today);
                LoadPermission(referendum);
                SetCollectionCount(referendum);
            }
        }

        return decrees;
    }

    private async Task<Dictionary<DomainOfInfluenceType, List<Initiative>>> ListInitiativesForDeletionByDoiType(
        IReadOnlySet<DomainOfInfluenceType>? doiTypes,
        string? bfs,
        CollectionControlSignFilter filter)
    {
        var today = _timeProvider.GetUtcTodayDateOnly();
        var initiativeQuery = _initiativeRepository
            .Query()
            .Include(x => x.CollectionCount)
            .WhereCanSetSensitiveDataExpiryDate(_permissionService)
            .Where(x => filter == CollectionControlSignFilter.ReadyToDelete
                ? x.SensitiveDataExpiryDate <= today
                : x.SensitiveDataExpiryDate > today);

        if (doiTypes?.Count > 0)
        {
            initiativeQuery = initiativeQuery.Where(x => doiTypes.Contains(x.DomainOfInfluenceType!.Value));
        }

        if (!string.IsNullOrEmpty(bfs))
        {
            initiativeQuery = initiativeQuery.Where(x => x.Bfs == bfs);
        }

        var initiativeEntities = await initiativeQuery
            .GroupBy(x => x.DomainOfInfluenceType)
            .ToDictionaryAsync(x => x.Key!.Value, x => x.OrderByDescending(y => y.CollectionEndDate).ThenBy(y => y.Description).ToList());

        var initiatives = initiativeEntities.ToDictionary(
            x => x.Key,
            x => Mapper.MapToInitiatives(x.Value));
        foreach (var initiative in initiatives.Values.SelectMany(x => x))
        {
            initiative.SetPeriodState(today);
            LoadPermission(initiative);
            SetCollectionCount(initiative);
        }

        return initiatives;
    }

    private async Task CreateCollectionMunicipalities(Guid collectionId, string bfs)
    {
        var municipalities = await _accessControlListService.GetMunicipalities(bfs);
        var collectionMunicipalities = municipalities.Select(x =>
        {
            var collectionMunicipality = new CollectionMunicipalityEntity
            {
                CollectionId = collectionId,
                Bfs = x.Bfs!,
                MunicipalityName = x.Name,
                NextSheetNumber = 1,
            };
            _permissionService.SetCreated(collectionMunicipality);
            return collectionMunicipality;
        });
        await _collectionMunicipalityRepository.CreateRange(collectionMunicipalities);
    }
}
