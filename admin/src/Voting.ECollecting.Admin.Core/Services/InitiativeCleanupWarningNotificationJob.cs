// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Database.Postgres.Locking;
using Voting.Lib.Scheduler;
using IPermissionService = Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam.IPermissionService;

namespace Voting.ECollecting.Admin.Core.Services;

public class InitiativeCleanupWarningNotificationJob : IScheduledJob
{
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<InitiativeCleanupWarningNotificationJob> _logger;
    private readonly CollectionCleanupJobConfig _config;
    private readonly ICollectionRepository _collectionRepository;
    private readonly IDataContext _dataContext;
    private readonly IUserNotificationService _userNotificationService;
    private readonly IPermissionService _permissionService;

    public InitiativeCleanupWarningNotificationJob(
        TimeProvider timeProvider,
        ILogger<InitiativeCleanupWarningNotificationJob> logger,
        CollectionCleanupJobConfig config,
        ICollectionRepository collectionRepository,
        IDataContext dataContext,
        IUserNotificationService userNotificationService,
        IPermissionService permissionService)
    {
        _timeProvider = timeProvider;
        _logger = logger;
        _config = config;
        _collectionRepository = collectionRepository;
        _dataContext = dataContext;
        _userNotificationService = userNotificationService;
        _permissionService = permissionService;
    }

    public async Task Run(CancellationToken ct)
    {
        _permissionService.SetAbraxasAuthIfNotAuthenticated();

        var now = _timeProvider.GetUtcNowDateTime();
        var notificationThreshold = now - _config.RetentionPeriod + _config.NotificationPeriod;

        try
        {
            var toNotify = await _collectionRepository.Query()
                .Where(x =>
                    x.Type == CollectionType.Initiative
                    && x.State == CollectionState.InPreparation
                    && !x.CollectionStartDate.HasValue
                    && !x.CleanupWarningSentAt.HasValue
                    && x.AuditInfo.CreatedAt < notificationThreshold)
                .Select(x => x.Id)
                .ToListAsync(ct);

            _logger.LogInformation("Found {Count} collections to warn about deletion (created before {Threshold}).", toNotify.Count, notificationThreshold);
            foreach (var collectionId in toNotify)
            {
                await SendNotification(collectionId, ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing collection cleanup warnings.");
        }
    }

    private async Task SendNotification(Guid collectionId, CancellationToken ct)
    {
        try
        {
            await using var transaction = await _dataContext.BeginTransaction(ct);

            var collection = await _collectionRepository.Query()
                .AsTracking()
                .ForUpdate()
                .FirstOrDefaultAsync(x => x.Id == collectionId && !x.CleanupWarningSentAt.HasValue, ct);

            if (collection == null)
            {
                await transaction.RollbackAsync(ct);
                return;
            }

            var recipients = await _dataContext.CollectionPermissions
                .Where(x => x.CollectionId == collectionId
                            && x.State == CollectionPermissionState.Accepted
                            && (x.Role == CollectionPermissionRole.Owner || x.Role == CollectionPermissionRole.Deputy))
                .Select(x => x.Email)
                .Distinct()
                .ToListAsync(ct);

            var now = _timeProvider.GetUtcNowDateTime();
            var cleanupDate = DateOnly.FromDateTime(collection.AuditInfo.CreatedAt.Add(_config.RetentionPeriod).ToLocalTime());

            await _userNotificationService.SendUserNotifications(
                recipients,
                true,
                UserNotificationType.CollectionCleanupWarning,
                collection: collection,
                collectionCleanupDate: cleanupDate,
                cancellationToken: ct);

            collection.CleanupWarningSentAt = now;
            await _dataContext.SaveChangesAsync();
            await transaction.CommitAsync(ct);

            _logger.LogInformation("Sent cleanup warning for collection {Id}", collectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send cleanup warning for collection {Id}", collectionId);
        }
    }
}
