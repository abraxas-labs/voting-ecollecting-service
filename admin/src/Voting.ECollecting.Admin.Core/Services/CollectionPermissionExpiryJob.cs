// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Scheduler;

namespace Voting.ECollecting.Admin.Core.Services;

public class CollectionPermissionExpiryJob : IScheduledJob
{
    private readonly IServiceProvider _serviceProvider;

    public CollectionPermissionExpiryJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Run(CancellationToken ct)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IDataContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CollectionPermissionExpiryJob>>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
        var now = timeProvider.GetUtcNowDateTime();

        try
        {
            var expiredCount = await db.CollectionPermissions
                .Where(x => x.State == CollectionPermissionState.Pending && x.TokenExpiry < now)
                .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, CollectionPermissionState.Expired), ct);

            if (expiredCount > 0)
            {
                logger.LogInformation("Expired {Count} collection permissions.", expiredCount);
            }
            else
            {
                logger.LogDebug("No collection permissions expired.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while expiring collection permissions.");
        }
    }
}
