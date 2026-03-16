// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Core.Mappings;
using Voting.ECollecting.Admin.Core.Permissions;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.Lib.Database.Postgres.Locking;
using IPermissionService = Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam.IPermissionService;

namespace Voting.ECollecting.Admin.Core.Services;

public class CollectionMunicipalityService : ICollectionMunicipalityService
{
    private readonly IPermissionService _permissionService;
    private readonly ICollectionRepository _collectionRepository;
    private readonly IDataContext _db;
    private readonly ICollectionMunicipalityRepository _collectionMunicipalityRepository;
    private readonly ICollectionSignatureSheetRepository _collectionSignatureSheetRepository;

    public CollectionMunicipalityService(
        IPermissionService permissionService,
        ICollectionRepository collectionRepository,
        IDataContext db,
        ICollectionMunicipalityRepository collectionMunicipalityRepository,
        ICollectionSignatureSheetRepository collectionSignatureSheetRepository)
    {
        _permissionService = permissionService;
        _collectionRepository = collectionRepository;
        _db = db;
        _collectionMunicipalityRepository = collectionMunicipalityRepository;
        _collectionSignatureSheetRepository = collectionSignatureSheetRepository;
    }

    public async Task<List<CollectionMunicipalityEntity>> List(Guid collectionId)
    {
        await EnsureCanCheckSamples(collectionId);
        var municipalities = await _collectionMunicipalityRepository.Query()
            .Where(x => x.CollectionId == collectionId)
            .OrderBy(x => x.MunicipalityName)
            .ToListAsync();

        foreach (var municipality in municipalities)
        {
            await SetSignatureSheetsCount(municipality);
        }

        return municipalities;
    }

    public async Task SetLocked(Guid collectionId, string bfs, bool locked)
    {
        await EnsureCanCheckSamples(collectionId);
        var collectionMunicipality = await _collectionMunicipalityRepository.Query()
                                         .AsTracking()
                                         .FirstOrDefaultAsync(x => x.CollectionId == collectionId && x.Bfs == bfs)
                                     ?? throw new EntityNotFoundException(nameof(CollectionMunicipalityEntity), new { collectionId, bfs });

        if (collectionMunicipality.IsLocked == locked)
        {
            throw new ValidationException($"Collection municipality is already in state locked: {locked}.");
        }

        collectionMunicipality.IsLocked = locked;
        _permissionService.SetModified(collectionMunicipality);
        await _db.SaveChangesAsync();
    }

    public async Task<SubmitMunicipalitySignatureSheetsResult> SubmitSignatureSheets(
        Guid collectionId,
        string bfs)
    {
        await EnsureCanCheckSamples(collectionId);
        await using var transaction = await _db.BeginTransaction();

        // lock municipality to ensure no other signature sheet is attested
        _ = await _collectionMunicipalityRepository.Query()
                .ForUpdate()
                .Where(x => x.CollectionId == collectionId && x.Bfs == bfs)
                .Select(_ => (int?)1)
                .FirstOrDefaultAsync()
            ?? throw new EntityNotFoundException(nameof(CollectionMunicipalityEntity), new { collectionId, bfs });

        await _collectionSignatureSheetRepository.AuditedUpdateRange(
            q => q
                .Where(x => x.CollectionMunicipality!.CollectionId == collectionId &&
                            x.CollectionMunicipality!.Bfs == bfs && x.State == CollectionSignatureSheetState.Attested)
                .OrderBy(x => x.Number),
            x =>
            {
                x.State = CollectionSignatureSheetState.Submitted;
                _permissionService.SetModified(x);
            });

        var collectionMunicipality = await _collectionMunicipalityRepository.Query()
                                         .FirstOrDefaultAsync(x => x.CollectionId == collectionId && x.Bfs == bfs)
                                     ?? throw new EntityNotFoundException(nameof(CollectionMunicipalityEntity), new { collectionId, bfs });
        await SetSignatureSheetsCount(collectionMunicipality);

        await transaction.CommitAsync();
        return new SubmitMunicipalitySignatureSheetsResult(collectionMunicipality);
    }

    public async Task<List<CollectionSignatureSheet>> ListSignatureSheets(Guid collectionId, string bfs)
    {
        await EnsureCanCheckSamples(collectionId);
        var entities = await _collectionSignatureSheetRepository.Query()
            .Include(x => x.CollectionMunicipality)
            .WhereCanCheckSamples(_permissionService)
            .Where(x => x.CollectionMunicipality!.CollectionId == collectionId && x.CollectionMunicipality!.Bfs == bfs)
            .OrderBy(x => x.Number)
            .ToListAsync();

        var sheets = Mapper.MapToCollectionSignatureSheets(entities);
        foreach (var sheet in sheets)
        {
            sheet.UserPermissions = CollectionSignatureSheetPermissions.Build(_permissionService, sheet);
        }

        return sheets;
    }

    private async Task EnsureCanCheckSamples(Guid collectionId)
    {
        var exists = await _collectionRepository.Query()
            .WhereCanCheckSamples(_permissionService)
            .AnyAsync(x => x.Id == collectionId);
        if (!exists)
        {
            throw new EntityNotFoundException(nameof(CollectionBaseEntity), collectionId);
        }
    }

    private async Task SetSignatureSheetsCount(CollectionMunicipalityEntity municipality)
    {
        var countsByState = await _collectionSignatureSheetRepository.Query()
            .Where(x => x.CollectionMunicipalityId == municipality.Id)
            .GroupBy(x => x.State)
            .ToDictionaryAsync(x => x.Key, x => x.Count());

        municipality.SignatureSheetsCount = new CollectionMunicipalitySignatureSheetsCount(
            countsByState.GetValueOrDefault(CollectionSignatureSheetState.Attested) + countsByState.GetValueOrDefault(CollectionSignatureSheetState.Submitted) + countsByState.GetValueOrDefault(CollectionSignatureSheetState.Confirmed),
            countsByState.GetValueOrDefault(CollectionSignatureSheetState.Submitted) + countsByState.GetValueOrDefault(CollectionSignatureSheetState.Confirmed),
            countsByState.GetValueOrDefault(CollectionSignatureSheetState.NotSubmitted),
            countsByState.GetValueOrDefault(CollectionSignatureSheetState.Confirmed));
    }
}
