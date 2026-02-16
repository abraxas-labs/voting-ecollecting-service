// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Scheduler;

namespace Voting.ECollecting.Admin.Core.Services;

public class InitiativeCommitteeMemberExpiryJob : IScheduledJob
{
    private readonly IServiceProvider _serviceProvider;

    public InitiativeCommitteeMemberExpiryJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Run(CancellationToken ct)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IDataContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<InitiativeCommitteeMemberExpiryJob>>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
        var now = timeProvider.GetUtcNowDateTime();
        var repo = scope.ServiceProvider.GetRequiredService<IInitiativeCommitteeMemberRepository>();

        scope.ServiceProvider.GetRequiredService<IPermissionService>().SetAbraxasAuthIfNotAuthenticated();

        try
        {
            // Process highest indices first, so shifting doesn't affect the indices of the remaining members
            // if there are multiple members to expire in the same initiative.
            var membersToExpire = await db.InitiativeCommitteeMembers
                .Where(x => x.ApprovalState == InitiativeCommitteeMemberApprovalState.Requested && x.TokenExpiry < now)
                .OrderByDescending(x => x.SortIndex)
                .ToListAsync(ct);

            await using var transaction = await db.BeginTransaction(ct);

            foreach (var member in membersToExpire)
            {
                await repo.AuditedUpdateRange(
                    q => q.Where(x => x.InitiativeId == member.InitiativeId && x.SortIndex > member.SortIndex).OrderBy(y => y.SortIndex),
                    x => --x.SortIndex);
            }

            var memberIds = membersToExpire.ConvertAll(t => t.Id);

            var expiredCount = await repo.AuditedUpdateRange(
                q => q.Where(x => memberIds.Contains(x.Id)),
                x =>
                {
                    x.ApprovalState = InitiativeCommitteeMemberApprovalState.Expired;
                    x.SortIndex = null;
                });

            await transaction.CommitAsync(ct);

            if (expiredCount > 0)
            {
                logger.LogInformation("Expired {Count} initiative committee members.", expiredCount);
            }
            else
            {
                logger.LogDebug("No initiative committee members expired.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while expiring initiative committee members.");
        }
    }
}
