// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Core.Permissions;

internal static class DecreePermissions
{
    public static IQueryable<DecreeEntity> WhereCanReadAnyCollection(
        this IQueryable<DecreeEntity> q,
        IPermissionService permissionService)
    {
        return q.Where(x => x.Collections.Any(c =>
            c.AuditInfo.CreatedById == permissionService.UserId
            || c.Permissions!.Any(p =>
                p.State == CollectionPermissionState.Accepted
                && p.IamUserId == permissionService.UserId
                && (p.Role == CollectionPermissionRole.Deputy || p.Role == CollectionPermissionRole.Reader))));
    }
}
