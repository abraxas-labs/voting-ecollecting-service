// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.Database.Postgres.Locking;
using Voting.Lib.Scheduler;
using Voting.Lib.UserNotifications;

namespace Voting.ECollecting.Admin.Core.Services.UserNotifications;

public class UserNotificationSenderJob : IScheduledJob
{
    private readonly IUserNotificationRepository _notificationRepo;
    private readonly IServiceProvider _serviceProvider;

    public UserNotificationSenderJob(IUserNotificationRepository notificationRepo, IServiceProvider serviceProvider)
    {
        _notificationRepo = notificationRepo;
        _serviceProvider = serviceProvider;
    }

    public async Task Run(CancellationToken ct)
    {
        var recipients = await _notificationRepo.Query()
            .Where(x => x.State == UserNotificationState.Pending
                        && (x.TemplateBag.NotificationType == UserNotificationType.MessageAdded ||
                            x.TemplateBag.NotificationType == UserNotificationType.StateChanged))
            .Select(x => x.RecipientEMail)
            .Distinct()
            .ToListAsync(ct);

        var sendTasks = recipients.Select(x => Send(_serviceProvider, x, ct));
        await Task.WhenAll(sendTasks);
    }

    private async Task Send(IServiceProvider sp, string recipient, CancellationToken ct)
    {
        await using var scope = sp.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IDataContext>();
        var repo = scope.ServiceProvider.GetRequiredService<IUserNotificationRepository>();
        var renderer = scope.ServiceProvider.GetRequiredService<GroupedUserNotificationRenderer>();
        var sender = scope.ServiceProvider.GetRequiredService<IUserNotificationSender>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<UserNotificationSenderJob>>();
        var config = scope.ServiceProvider.GetRequiredService<UserNotificationsJobConfig>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

        await using var transaction = await db.BeginTransaction(ct);

        IReadOnlyList<Guid> notificationIds = [];
        try
        {
            var notifications = await repo.Query()
                .Where(x =>
                    x.RecipientEMail == recipient
                    && x.State == UserNotificationState.Pending
                    && (x.TemplateBag.NotificationType == UserNotificationType.MessageAdded ||
                        x.TemplateBag.NotificationType == UserNotificationType.StateChanged))
                .ForUpdateSkipLocked()
                .ToListAsync(ct);

            notificationIds = notifications.ConvertAll(x => x.Id);

            var message = renderer.Render(recipient, notifications);
            await sender.Send(message, ct);

            logger.LogInformation("User notification {ids} sent.", notificationIds);

            await repo.Query()
                .Where(x => notificationIds.Contains(x.Id))
                .ExecuteUpdateAsync(
                    x => x
                        .SetProperty(y => y.State, UserNotificationState.Sent)
                        .SetProperty(y => y.SentTimestamp, timeProvider.GetUtcNowDateTime()),
                    ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Sending user notifications {ids} failed.", notificationIds);

            await repo.Query()
                .Where(x => notificationIds.Contains(x.Id))
                .ExecuteUpdateAsync(
                    x => x
                        .SetProperty(y => y.FailureCounter, y => y.FailureCounter + 1)
                        .SetProperty(y => y.LastError, $"{ex.GetType().Name}: {ex.Message}{Environment.NewLine}{Environment.NewLine}{ex.StackTrace}")
                        .SetProperty(y => y.State, y => y.FailureCounter + 1 >= config.MaxRetries ? UserNotificationState.Failed : UserNotificationState.Pending)
                        .SetProperty(y => y.SentTimestamp, timeProvider.GetUtcNowDateTime()),
                    ct);
        }
        finally
        {
            await transaction.CommitAsync(ct);
        }
    }
}
