// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingBasis;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Admin.Core.Validators;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using AclDomainOfInfluenceType = Voting.ECollecting.Shared.Domain.Enums.AclDomainOfInfluenceType;
using ImportType = Voting.ECollecting.Shared.Domain.Enums.ImportType;

namespace Voting.ECollecting.Admin.Core.Import;

/// <summary>
/// Domain of influence access control list import service from VOTING Basis.
/// </summary>
public class AccessControlListImporter : IAccessControlListImporter
{
    private readonly AccessControlListEntityValidator _aclEntityValidator = new();

    private readonly TimeProvider _timeProvider;
    private readonly IVotingBasisAdapter _votingBasisAdapter;
    private readonly IAccessControlListDoiRepository _aclDoiRepo;
    private readonly IImportStatisticRepository _importStatisticRepo;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<AccessControlListImporter> _logger;
    private readonly IDataContext _dataContext;
    private readonly ImportConfig _config;
    private readonly List<Guid> _entityIdsWithValidationErrors = new();

    private readonly List<AccessControlListDoiEntity> _allAclImportList = new();
    private readonly List<AccessControlListDoiEntity> _newAclEntityList = new();
    private readonly List<AccessControlListDoiEntity> _updateAclEntityList = new();
    private readonly List<AccessControlListDoiEntity> _deleteAclEntityList = new();

    private readonly Stopwatch _stopwatch = new();
    private Guid _importStatisticId;

    public AccessControlListImporter(
        TimeProvider timeProvider,
        IAccessControlListDoiRepository aclDoiRepo,
        IImportStatisticRepository importStatisticRepo,
        IVotingBasisAdapter votingBasisAdapter,
        ILogger<AccessControlListImporter> logger,
        IPermissionService permissionService,
        IDataContext dataContext,
        ImportConfig config)
    {
        _timeProvider = timeProvider;
        _aclDoiRepo = aclDoiRepo;
        _importStatisticRepo = importStatisticRepo;
        _votingBasisAdapter = votingBasisAdapter;
        _logger = logger;
        _permissionService = permissionService;
        _dataContext = dataContext;
        _config = config;
    }

    /// <inheritdoc/>
    public async Task ImportAcl(
        IReadOnlySet<Canton> allowedCantons,
        IReadOnlySet<string> ignoredBfs)
    {
        _stopwatch.Start();
        _logger.LogDebug("Start importing domain of influence based access control list from VOTING Basis.");

        try
        {
            _importStatisticId = (await CreateImportStatistics()).Id;

            _allAclImportList.AddRange(await GetFilteredAccessControlList(allowedCantons, ignoredBfs));

            var aclsFromRepo = await _aclDoiRepo.Query().AsNoTracking().ToListAsync();
            _deleteAclEntityList.AddRange(aclsFromRepo);

            foreach (var aclFromImport in _allAclImportList)
            {
                var existingAclFromRepo = aclsFromRepo.Find(x => x.Id.Equals(aclFromImport.Id));

                if (existingAclFromRepo != null)
                {
                    _deleteAclEntityList.Remove(existingAclFromRepo);

                    if (!IsEntityUpdateRequired(aclFromImport, existingAclFromRepo))
                    {
                        continue;
                    }

                    UpdateEntity(aclFromImport, existingAclFromRepo);
                }
                else
                {
                    CreateEntity(aclFromImport);
                }
            }

            await UpdateDatabase();
            await UpdateImportStatistics();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An error occurred while importing domain of influence based access control list from VOTING Basis.");

            if (_importStatisticId != Guid.Empty)
            {
                await _importStatisticRepo.UpdateFinishedWithProcessingErrors(
                    _importStatisticId,
                    ex.Message,
                    new(),
                    _entityIdsWithValidationErrors,
                    null,
                    _timeProvider.GetUtcNowDateTime());
            }
        }

        _logger.LogDebug("Finished importing domain of influence based access control list from VOTING Basis");
    }

    private async Task<ImportStatisticEntity> CreateImportStatistics()
    {
        var entity = new ImportStatisticEntity
        {
            ImportStatus = ImportStatus.Running,
            ImportType = ImportType.Acl,
            SourceSystem = ImportSourceSystem.VotingBasis,
            WorkerName = _config.WorkerName,
        };

        _permissionService.SetCreated(entity);
        entity.StartedDate = entity.AuditInfo.CreatedAt;
        await _importStatisticRepo.CreateAndUpdateIsLatest(entity);
        return entity;
    }

    /// <summary>
    /// Processes the validation of the acl entity. Potential validation errors are stored on the entity for reporting purposes.
    /// </summary>
    /// <param name="entity">The ACL entity to validate.</param>
    private void ValidateEntity(AccessControlListDoiEntity entity)
    {
        var validationResult = _aclEntityValidator.Validate(entity);
        if (validationResult.IsValid)
        {
            entity.IsValid = true;
            return;
        }

        _logger.LogWarning("Validation failed for access control list entity with id '{Id}'", entity.Id);

        var validationErrors = validationResult.ToDictionary();
        entity.IsValid = false;
        entity.ValidationErrors = JsonSerializer.Serialize(validationErrors);
        _entityIdsWithValidationErrors.Add(entity.Id);
    }

    private async Task UpdateImportStatistics()
    {
        var importEntity = await _importStatisticRepo.GetByKey(_importStatisticId)
                           ?? throw new EntityNotFoundException(typeof(ImportStatisticEntity), _importStatisticId);

        importEntity.ImportStatus = _entityIdsWithValidationErrors.Count > 0 ?
            ImportStatus.FinishedWithErrors :
            ImportStatus.FinishedSuccessfully;

        importEntity.ImportRecordsCountTotal = _allAclImportList.Count;
        importEntity.DatasetsCountCreated = _newAclEntityList.Count;
        importEntity.DatasetsCountUpdated = _updateAclEntityList.Count;
        importEntity.DatasetsCountDeleted = _deleteAclEntityList.Count;
        importEntity.FinishedDate = _timeProvider.GetUtcNowDateTime();
        importEntity.HasValidationErrors = _entityIdsWithValidationErrors.Count > 0;
        importEntity.EntitiesWithValidationErrors = _entityIdsWithValidationErrors;
        importEntity.TotalElapsedMilliseconds = _stopwatch.ElapsedMilliseconds;

        _permissionService.SetModified(importEntity);

        await _importStatisticRepo.Update(importEntity);
    }

    private AccessControlListDoiEntity MapToAclEntity(
        AccessControlListDoiEntity source,
        AccessControlListDoiEntity target)
    {
        target.Name = source.Name;
        target.Bfs = string.IsNullOrWhiteSpace(source.Bfs) ? null : source.Bfs;
        target.TenantName = source.TenantName;
        target.TenantId = source.TenantId;
        target.Type = source.Type;
        target.Canton = source.Canton;
        target.ParentId = source.ParentId;
        target.ImportStatisticId = _importStatisticId;
        target.ECollectingEnabled = source.ECollectingEnabled;
        target.ECollectingInitiativeMinSignatureCount = source.ECollectingInitiativeMinSignatureCount;
        target.ECollectingInitiativeMaxElectronicSignaturePercent = source.ECollectingInitiativeMaxElectronicSignaturePercent;
        target.ECollectingInitiativeNumberOfMembersCommittee = source.ECollectingInitiativeNumberOfMembersCommittee;
        target.ECollectingReferendumMinSignatureCount = source.ECollectingReferendumMinSignatureCount;
        target.ECollectingReferendumMaxElectronicSignaturePercent = source.ECollectingReferendumMaxElectronicSignaturePercent;
        target.ECollectingEmail = source.ECollectingEmail;
        target.SortNumber = source.SortNumber;
        target.NameForProtocol = source.NameForProtocol;

        return target;
    }

    /// <summary>
    /// Compares all relevant fields from the imported entity with the database entity and
    /// indicates whether the entity should be updated or not.
    /// </summary>
    /// <param name="aclFromImport">The imported ACL entity.</param>
    /// <param name="aclFromRepo">The existing ACL entity.</param>
    /// <returns>True if the entity should be updated.</returns>
    private bool IsEntityUpdateRequired(AccessControlListDoiEntity aclFromImport, AccessControlListDoiEntity aclFromRepo)
    {
        var compareFunctions = new List<Func<int>>
        {
            () => Comparer<string>.Default.Compare(aclFromImport.Name, aclFromRepo.Name),
            () => Comparer<string?>.Default.Compare(aclFromImport.Bfs, aclFromRepo.Bfs),
            () => Comparer<string>.Default.Compare(aclFromImport.TenantName, aclFromRepo.TenantName),
            () => Comparer<string>.Default.Compare(aclFromImport.TenantId, aclFromRepo.TenantId),
            () => Comparer<AclDomainOfInfluenceType>.Default.Compare(aclFromImport.Type, aclFromRepo.Type),
            () => Comparer<Canton>.Default.Compare(aclFromImport.Canton, aclFromRepo.Canton),
            () => Comparer<bool>.Default.Compare(aclFromImport.ECollectingEnabled, aclFromRepo.ECollectingEnabled),
            () => Comparer<int?>.Default.Compare(aclFromImport.ECollectingInitiativeMinSignatureCount, aclFromRepo.ECollectingInitiativeMinSignatureCount),
            () => Comparer<int?>.Default.Compare(aclFromImport.ECollectingInitiativeMaxElectronicSignaturePercent, aclFromRepo.ECollectingInitiativeMaxElectronicSignaturePercent),
            () => Comparer<int?>.Default.Compare(aclFromImport.ECollectingInitiativeNumberOfMembersCommittee, aclFromRepo.ECollectingInitiativeNumberOfMembersCommittee),
            () => Comparer<int?>.Default.Compare(aclFromImport.ECollectingReferendumMinSignatureCount, aclFromRepo.ECollectingReferendumMinSignatureCount),
            () => Comparer<int?>.Default.Compare(aclFromImport.ECollectingReferendumMaxElectronicSignaturePercent, aclFromRepo.ECollectingReferendumMaxElectronicSignaturePercent),
            () => Comparer<string>.Default.Compare(aclFromImport.ECollectingEmail, aclFromRepo.ECollectingEmail),
            () => Comparer<int>.Default.Compare(aclFromImport.SortNumber, aclFromRepo.SortNumber),
            () => Comparer<string>.Default.Compare(aclFromImport.NameForProtocol, aclFromRepo.NameForProtocol),
        };

        return compareFunctions.Any(compareFunction => compareFunction() != 0);
    }

    /// <summary>
    /// Updates an existing entity.
    /// Which means:
    /// <list type="bullet">
    ///     <item>Adding the entity to the update list.</item>
    ///     <item>Removing the entity from the deletion list.</item>
    /// </list>
    /// </summary>
    /// <param name="source">The source to update from.</param>
    /// <param name="target">The target to be updated from the source.</param>
    private void UpdateEntity(AccessControlListDoiEntity source, AccessControlListDoiEntity target)
    {
        var updatedEntity = MapToAclEntity(source, target);
        ValidateEntity(updatedEntity);
        _permissionService.SetModified(updatedEntity);
        _updateAclEntityList.Add(updatedEntity);
        _deleteAclEntityList.Remove(updatedEntity);
    }

    /// <summary>
    /// Creates a new entity from the import.
    /// </summary>
    /// <param name="entity">The new entity to be added.</param>
    private void CreateEntity(AccessControlListDoiEntity entity)
    {
        ValidateEntity(entity);
        _permissionService.SetCreated(entity);
        _newAclEntityList.Add(entity);
    }

    private async Task UpdateDatabase()
    {
        await using var transaction = await _dataContext.BeginTransaction();

        if (_updateAclEntityList.Count > 0)
        {
            LogDatabaseUpdates("Update", _updateAclEntityList);
            await _aclDoiRepo.UpdateRange(_updateAclEntityList);
        }

        if (_deleteAclEntityList.Count > 0)
        {
            LogDatabaseUpdates("Delete", _deleteAclEntityList);
            foreach (var id in _deleteAclEntityList.Select(acl => acl.Id))
            {
                await _aclDoiRepo.DeleteByKeyIfExists(id);
            }
        }

        if (_newAclEntityList.Count > 0)
        {
            LogDatabaseUpdates("Create", _newAclEntityList);
            await _aclDoiRepo.CreateRange(_newAclEntityList);
        }

        await transaction.CommitAsync();
    }

    private void LogDatabaseUpdates(string operation, IEnumerable<AccessControlListDoiEntity> entities)
    {
        if (!entities.Any())
        {
            return;
        }

        _logger.LogInformation("{operation} access control list domain of influence summary:", operation);
        foreach (var entity in entities)
        {
            _logger.LogInformation(
                " > {operation} ACL-DOI '{name}' with id {id} of type {type} (valid={isValid})",
                operation,
                entity.Name,
                entity.Id,
                entity.Type,
                entity.IsValid);
        }
    }

    private async Task<IEnumerable<AccessControlListDoiEntity>> GetFilteredAccessControlList(
        IReadOnlySet<Canton> allowedCantons,
        IReadOnlySet<string> ignoredBfs)
    {
        var accessControlList = await _votingBasisAdapter.GetAccessControlList(_importStatisticId);
        var rootById = new Dictionary<Guid, AccessControlListDoiEntity>();
        var allById = accessControlList.ToDictionary(x => x.Id);

        return accessControlList.Where(a =>
            allowedCantons.Contains(a.Canton)
            && (a.Bfs == null || !ignoredBfs.Contains(a.Bfs))
            && GetRoot(a, rootById, allById).ECollectingEnabled);
    }

    private AccessControlListDoiEntity GetRoot(
        AccessControlListDoiEntity entity,
        Dictionary<Guid, AccessControlListDoiEntity> rootById,
        Dictionary<Guid, AccessControlListDoiEntity> allById)
    {
        if (rootById.TryGetValue(entity.Id, out var rootEntity))
        {
            return rootEntity;
        }

        if (entity.ParentId == null)
        {
            rootById[entity.Id] = entity;
            return entity;
        }

        var root = GetRoot(allById[entity.ParentId.Value], rootById, allById);
        rootById[entity.Id] = root;
        return root;
    }
}
