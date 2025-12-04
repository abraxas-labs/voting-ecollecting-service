// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Domain.Queries;

public static class InitiativeQueries
{
    public static IQueryable<InitiativeEntity> WhereHasNoAdmissibilityDecision(this IQueryable<InitiativeEntity> q)
    {
        return q.Where(x => x.AdmissibilityDecisionState == null);
    }

    public static IQueryable<InitiativeEntity> IncludeRequestedCommitteeMember(
        this IQueryable<InitiativeEntity> q,
        Guid memberId)
    {
        return q.Include(x => x.CommitteeMembers
            .Where(m =>
                m.Id == memberId
                && m.ApprovalState == InitiativeCommitteeMemberApprovalState.Requested
                && m.MemberSignatureRequested));
    }
}
