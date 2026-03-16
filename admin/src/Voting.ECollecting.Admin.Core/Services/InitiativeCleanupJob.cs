// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Admin.Core.Services.Crypto;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Database.Postgres.Locking;
using Voting.Lib.Scheduler;

namespace Voting.ECollecting.Admin.Core.Services;

public class InitiativeCleanupJob : IScheduledJob
{
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<InitiativeCleanupJob> _logger;
    private readonly CollectionCleanupJobConfig _config;
    private readonly ICollectionRepository _collectionRepository;
    private readonly CollectionCryptoService _collectionCryptoService;
    private readonly IDataContext _dataContext;
    private readonly IPermissionService _permissionService;

    public InitiativeCleanupJob(
        TimeProvider timeProvider,
        ILogger<InitiativeCleanupJob> logger,
        CollectionCleanupJobConfig config,
        ICollectionRepository collectionRepository,
        CollectionCryptoService collectionCryptoService,
        IDataContext dataContext,
        IPermissionService permissionService)
    {
        _timeProvider = timeProvider;
        _logger = logger;
        _config = config;
        _collectionRepository = collectionRepository;
        _collectionCryptoService = collectionCryptoService;
        _dataContext = dataContext;
        _permissionService = permissionService;
    }

    public async Task Run(CancellationToken ct)
    {
        _permissionService.SetAbraxasAuthIfNotAuthenticated();

        var now = _timeProvider.GetUtcNowDateTime();
        var deleteWarningThreshold = now - _config.NotificationPeriod;

        try
        {
            var toDelete = await _collectionRepository.Query()
                .Where(x =>
                    x.Type == CollectionType.Initiative
                    && x.State == CollectionState.InPreparation
                    && !x.CollectionStartDate.HasValue
                    && x.CleanupWarningSentAt.HasValue
                    && x.CleanupWarningSentAt.Value < deleteWarningThreshold)
                .Select(x => x.Id)
                .ToListAsync(ct);

            _logger.LogInformation("Deleting {Count} collections (warned for deletion before {Threshold}).", toDelete.Count, deleteWarningThreshold);
            foreach (var collectionId in toDelete)
            {
                await Delete(collectionId, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up collections.");
        }
    }

    private async Task Delete(Guid collectionId, CancellationToken ct)
    {
        try
        {
            await using var transaction = await _dataContext.BeginTransaction(ct);
            var collection = await _collectionRepository.Query()
                .ForUpdate()
                .FirstOrDefaultAsync(x => x.Id == collectionId, ct);
            if (collection == null)
            {
                await transaction.RollbackAsync(ct);
                return;
            }

            await _collectionCryptoService.DeleteKeys(collection);
            await _collectionRepository.AuditedDelete(collection);
            await transaction.CommitAsync(ct);

            _logger.LogInformation("Successfully deleted collection {Id}", collectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete collection {Id}", collectionId);
        }
    }
}
