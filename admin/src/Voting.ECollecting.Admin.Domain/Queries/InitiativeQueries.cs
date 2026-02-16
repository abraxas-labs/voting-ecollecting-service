// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Admin.Domain.Queries;

public static class InitiativeQueries
{
    public static IQueryable<InitiativeEntity> IncludeApprovedOrRejectedCommitteeMember(
        this IQueryable<InitiativeEntity> q,
        Guid memberId)
    {
        return q.Include(x => x.CommitteeMembers
            .Where(m =>
                m.Id == memberId
                && (m.ApprovalState == InitiativeCommitteeMemberApprovalState.Approved
                    || m.ApprovalState == InitiativeCommitteeMemberApprovalState.Rejected)));
    }

    public static IQueryable<InitiativeEntity> IncludeRequestedOrSignedCommitteeMember(
        this IQueryable<InitiativeEntity> q,
        Guid memberId)
    {
        return q.Include(x => x.CommitteeMembers
            .Where(m =>
                m.Id == memberId
                && (m.ApprovalState == InitiativeCommitteeMemberApprovalState.Requested
                || m.ApprovalState == InitiativeCommitteeMemberApprovalState.Signed)));
    }
}
