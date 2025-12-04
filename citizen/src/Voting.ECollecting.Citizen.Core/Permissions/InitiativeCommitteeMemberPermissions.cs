// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Common;

namespace Voting.ECollecting.Citizen.Core.Permissions;

internal static class InitiativeCommitteeMemberPermissions
{
    public static IQueryable<InitiativeCommitteeMemberEntity> WhereIsRequestedAndCanReadWithToken(
        this IQueryable<InitiativeCommitteeMemberEntity> q,
        IPermissionService permissionService,
        UrlToken token)
    {
        return q.Where(x =>
            x.ApprovalState == InitiativeCommitteeMemberApprovalState.Requested
            && x.Token == token
            && x.TokenExpiry >= permissionService.Now);
    }
}
