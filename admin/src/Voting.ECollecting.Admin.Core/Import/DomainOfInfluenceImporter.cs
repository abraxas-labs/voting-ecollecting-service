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
using ImportType = Voting.ECollecting.Shared.Domain.Enums.ImportType;

namespace Voting.ECollecting.Admin.Core.Import;

/// <summary>
/// Domain of influence access control list import service from VOTING Basis.
/// </summary>
public class DomainOfInfluenceImporter : IDomainOfInfluenceImporter
{
    private readonly DomainOfInfluenceImportEntityValidator _importEntityValidator = new();

    private readonly TimeProvider _timeProvider;
    private readonly IVotingBasisAdapter _votingBasisAdapter;
    private readonly IDomainOfInfluenceRepository _domainOfInfluenceRepository;
    private readonly IImportStatisticRepository _importStatisticRepo;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<DomainOfInfluenceImporter> _logger;
    private readonly IDataContext _dataContext;
    private readonly ImportConfig _config;
    private readonly List<Guid> _entityIdsWithValidationErrors = new();

    private readonly List<DomainOfInfluenceEntity> _allImportList = new();
    private readonly List<DomainOfInfluenceEntity> _newEntityList = new();
    private readonly List<DomainOfInfluenceEntity> _updateEntityList = new();
    private readonly List<DomainOfInfluenceEntity> _deleteEntityList = new();

    private readonly Stopwatch _stopwatch = new();
    private Guid _importStatisticId;

    public DomainOfInfluenceImporter(
        TimeProvider timeProvider,
        IDomainOfInfluenceRepository domainOfInfluenceRepository,
        IImportStatisticRepository importStatisticRepo,
        IVotingBasisAdapter votingBasisAdapter,
        ILogger<DomainOfInfluenceImporter> logger,
        IPermissionService permissionService,
        IDataContext dataContext,
        ImportConfig config)
    {
        _timeProvider = timeProvider;
        _domainOfInfluenceRepository = domainOfInfluenceRepository;
        _importStatisticRepo = importStatisticRepo;
        _votingBasisAdapter = votingBasisAdapter;
        _logger = logger;
        _permissionService = permissionService;
        _dataContext = dataContext;
        _config = config;
    }

    /// <inheritdoc/>
    public async Task ImportDomainOfInfluences(
        IReadOnlySet<Canton> allowedCantons,
        IReadOnlySet<string> ignoredBfs)
    {
        _stopwatch.Start();
        _logger.LogDebug("Start importing domain of influence based access control list from VOTING Basis.");

        try
        {
            _importStatisticId = (await CreateImportStatistics()).Id;

            _allImportList.AddRange(await GetFiltered(allowedCantons, ignoredBfs));

            var doisFromRepo = await _domainOfInfluenceRepository.Query().AsNoTracking().ToListAsync();
            var doisFromRepoById = doisFromRepo.ToDictionary(x => x.Id);
            _deleteEntityList.AddRange(doisFromRepo);

            foreach (var doiFromImport in _allImportList)
            {
                if (doisFromRepoById.TryGetValue(doiFromImport.Id, out var existingDoiFromRepo))
                {
                    _deleteEntityList.Remove(existingDoiFromRepo);

                    if (!IsEntityUpdateRequired(doiFromImport, existingDoiFromRepo))
                    {
                        continue;
                    }

                    UpdateEntity(doiFromImport, existingDoiFromRepo);
                }
                else
                {
                    CreateEntity(doiFromImport);
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
            ImportType = ImportType.DomainOfInfluences,
            SourceSystem = ImportSourceSystem.VotingBasis,
            WorkerName = _config.WorkerName,
        };

        _permissionService.SetCreated(entity);
        entity.StartedDate = entity.AuditInfo.CreatedAt;
        await _importStatisticRepo.CreateAndUpdateIsLatest(entity);
        return entity;
    }

    /// <summary>
    /// Processes the validation of the entity. Potential validation errors are stored on the entity for reporting purposes.
    /// </summary>
    /// <param name="entity">The DOI entity to validate.</param>
    private void ValidateEntity(DomainOfInfluenceEntity entity)
    {
        var validationResult = _importEntityValidator.Validate(entity);
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

        importEntity.ImportRecordsCountTotal = _allImportList.Count;
        importEntity.DatasetsCountCreated = _newEntityList.Count;
        importEntity.DatasetsCountUpdated = _updateEntityList.Count;
        importEntity.DatasetsCountDeleted = _deleteEntityList.Count;
        importEntity.FinishedDate = _timeProvider.GetUtcNowDateTime();
        importEntity.HasValidationErrors = _entityIdsWithValidationErrors.Count > 0;
        importEntity.EntitiesWithValidationErrors = _entityIdsWithValidationErrors;
        importEntity.TotalElapsedMilliseconds = _stopwatch.ElapsedMilliseconds;

        _permissionService.SetModified(importEntity);

        await _importStatisticRepo.Update(importEntity);
    }

    private DomainOfInfluenceEntity MapToEntity(
        DomainOfInfluenceEntity source,
        DomainOfInfluenceEntity target)
    {
        target.Name = source.Name;
        target.Bfs = string.IsNullOrWhiteSpace(source.Bfs) ? null : source.Bfs;
        target.TenantName = source.TenantName;
        target.TenantId = source.TenantId;
        target.Type = source.Type;
        target.BasisType = source.BasisType;
        target.Canton = source.Canton;
        target.ParentId = source.ParentId;
        target.ImportStatisticId = _importStatisticId;
        target.ECollectingEnabled = source.ECollectingEnabled;
        target.SortNumber = source.SortNumber;
        target.NameForProtocol = source.NameForProtocol;

        return target;
    }

    /// <summary>
    /// Compares all relevant fields from the imported entity with the database entity and
    /// indicates whether the entity should be updated or not.
    /// </summary>
    /// <param name="doiFromImport">The imported DOI entity.</param>
    /// <param name="doiFromRepo">The existing DOI entity.</param>
    /// <returns>True if the entity should be updated.</returns>
    private bool IsEntityUpdateRequired(DomainOfInfluenceEntity doiFromImport, DomainOfInfluenceEntity doiFromRepo)
    {
        var compareFunctions = new List<Func<int>>
        {
            () => Comparer<string>.Default.Compare(doiFromImport.Name, doiFromRepo.Name),
            () => Comparer<string?>.Default.Compare(doiFromImport.Bfs, doiFromRepo.Bfs),
            () => Comparer<string>.Default.Compare(doiFromImport.TenantName, doiFromRepo.TenantName),
            () => Comparer<string>.Default.Compare(doiFromImport.TenantId, doiFromRepo.TenantId),
            () => Comparer<BasisDomainOfInfluenceType>.Default.Compare(doiFromImport.BasisType, doiFromRepo.BasisType),
            () => Comparer<DomainOfInfluenceType>.Default.Compare(doiFromImport.Type, doiFromRepo.Type),
            () => Comparer<Canton>.Default.Compare(doiFromImport.Canton, doiFromRepo.Canton),
            () => Comparer<bool>.Default.Compare(doiFromImport.ECollectingEnabled, doiFromRepo.ECollectingEnabled),
            () => Comparer<int>.Default.Compare(doiFromImport.SortNumber, doiFromRepo.SortNumber),
            () => Comparer<string>.Default.Compare(doiFromImport.NameForProtocol, doiFromRepo.NameForProtocol),
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
    private void UpdateEntity(DomainOfInfluenceEntity source, DomainOfInfluenceEntity target)
    {
        var updatedEntity = MapToEntity(source, target);
        ValidateEntity(updatedEntity);
        _permissionService.SetModified(updatedEntity);
        _updateEntityList.Add(updatedEntity);
        _deleteEntityList.Remove(updatedEntity);
    }

    /// <summary>
    /// Creates a new entity from the import.
    /// </summary>
    /// <param name="entity">The new entity to be added.</param>
    private void CreateEntity(DomainOfInfluenceEntity entity)
    {
        ValidateEntity(entity);
        _permissionService.SetCreated(entity);
        _newEntityList.Add(entity);
    }

    private async Task UpdateDatabase()
    {
        await using var transaction = await _dataContext.BeginTransaction();

        if (_updateEntityList.Count > 0)
        {
            LogDatabaseUpdates("Update", _updateEntityList);
            await _domainOfInfluenceRepository.UpdateRange(_updateEntityList);
        }

        if (_deleteEntityList.Count > 0)
        {
            LogDatabaseUpdates("Delete", _deleteEntityList);
            foreach (var id in _deleteEntityList.Select(doi => doi.Id))
            {
                await _domainOfInfluenceRepository.DeleteByKeyIfExists(id);
            }
        }

        if (_newEntityList.Count > 0)
        {
            LogDatabaseUpdates("Create", _newEntityList);
            await _domainOfInfluenceRepository.CreateRange(_newEntityList);
        }

        await transaction.CommitAsync();
    }

    private void LogDatabaseUpdates(string operation, IReadOnlyCollection<DomainOfInfluenceEntity> entities)
    {
        if (entities.Count == 0)
        {
            return;
        }

        _logger.LogInformation("{Operation} domain of influence summary:", operation);
        foreach (var entity in entities)
        {
            _logger.LogInformation(
                " > {Operation} DOI '{Name}' with id {Id} of type {Type} (valid={IsValid})",
                operation,
                entity.Name,
                entity.Id,
                entity.Type,
                entity.IsValid);
        }
    }

    private async Task<IEnumerable<DomainOfInfluenceEntity>> GetFiltered(
        IReadOnlySet<Canton> allowedCantons,
        IReadOnlySet<string> ignoredBfs)
    {
        var accessControlList = await _votingBasisAdapter.GetAccessControlList(_importStatisticId);
        var rootById = new Dictionary<Guid, DomainOfInfluenceEntity>();
        var allById = accessControlList.ToDictionary(x => x.Id);

        return accessControlList.Where(a =>
            allowedCantons.Contains(a.Canton)
            && (a.Bfs == null || !ignoredBfs.Contains(a.Bfs))
            && GetRoot(a, rootById, allById).ECollectingEnabled);
    }

    private DomainOfInfluenceEntity GetRoot(
        DomainOfInfluenceEntity entity,
        Dictionary<Guid, DomainOfInfluenceEntity> rootById,
        Dictionary<Guid, DomainOfInfluenceEntity> allById)
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
