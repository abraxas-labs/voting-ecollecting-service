// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Core.Permissions;

internal static class CollectionAclPermissions
{
    public static IQueryable<T> WhereCanWrite<T>(
        this IQueryable<T> q,
        IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return q.Where(x =>
            x.AuditInfo.CreatedById == permissionService.UserId
            || x.Permissions!.Any(p =>
                p.State == CollectionPermissionState.Accepted
                && p.IamUserId == permissionService.UserId
                && p.Role == CollectionPermissionRole.Deputy));
    }

    public static IQueryable<T> WhereCanRead<T>(
        this IQueryable<T> q,
        IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return q.Where(x =>
            x.AuditInfo.CreatedById == permissionService.UserId
            || x.Permissions!.Any(p =>
                p.State == CollectionPermissionState.Accepted
                && p.IamUserId == permissionService.UserId
                && (p.Role == CollectionPermissionRole.Deputy || p.Role == CollectionPermissionRole.Reader)));
    }
}
