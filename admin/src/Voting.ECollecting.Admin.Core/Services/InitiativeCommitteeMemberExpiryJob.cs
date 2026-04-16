// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Scheduler;

namespace Voting.ECollecting.Admin.Core.Services;

public class InitiativeCommitteeMemberExpiryJob : IScheduledJob
{
    private readonly IDataContext _db;
    private readonly ILogger<InitiativeCommitteeMemberExpiryJob> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly IInitiativeCommitteeMemberRepository _repo;
    private readonly IPermissionService _permissionService;

    public InitiativeCommitteeMemberExpiryJob(
        IDataContext db,
        ILogger<InitiativeCommitteeMemberExpiryJob> logger,
        TimeProvider timeProvider,
        IInitiativeCommitteeMemberRepository repo,
        IPermissionService permissionService)
    {
        _db = db;
        _logger = logger;
        _timeProvider = timeProvider;
        _repo = repo;
        _permissionService = permissionService;
    }

    public async Task Run(CancellationToken ct)
    {
        var now = _timeProvider.GetUtcNowDateTime();

        _permissionService.SetAbraxasAuthIfNotAuthenticated();

        try
        {
            // Process highest indices first, so shifting doesn't affect the indices of the remaining members
            // if there are multiple members to expire in the same initiative.
            var membersToExpire = await _db.InitiativeCommitteeMembers
                .Where(x => x.ApprovalState == InitiativeCommitteeMemberApprovalState.Requested && x.TokenExpiry < now)
                .OrderByDescending(x => x.SortIndex)
                .ToListAsync(ct);

            await using var transaction = await _db.BeginTransaction(ct);

            foreach (var member in membersToExpire)
            {
                await _repo.AuditedUpdateRange(
                    q => q.Where(x => x.InitiativeId == member.InitiativeId && x.SortIndex > member.SortIndex).OrderBy(y => y.SortIndex),
                    x => --x.SortIndex);
            }

            var memberIds = membersToExpire.ConvertAll(t => t.Id);

            var expiredCount = await _repo.AuditedUpdateRange(
                q => q
                    .Where(x => memberIds.Contains(x.Id))
                    .OrderBy(x => x.Id),
                x =>
                {
                    x.ApprovalState = InitiativeCommitteeMemberApprovalState.Expired;
                    x.SortIndex = null;
                });

            await transaction.CommitAsync(ct);

            if (expiredCount > 0)
            {
                _logger.LogInformation("Expired {Count} initiative committee members.", expiredCount);
            }
            else
            {
                _logger.LogDebug("No initiative committee members expired.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while expiring initiative committee members.");
        }
    }
}
