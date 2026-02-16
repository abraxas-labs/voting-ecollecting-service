// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin;
using Voting.ECollecting.Citizen.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Common;

namespace Voting.ECollecting.Citizen.Core.Permissions;

internal static class CollectionPermissionPermissions
{
    public static IQueryable<CollectionPermissionEntity> WhereIsPendingAndCanReadWithToken(
        this IQueryable<CollectionPermissionEntity> q,
        IPermissionService permissionService,
        UrlToken token)
    {
        return q.Where(x =>
            x.State == CollectionPermissionState.Pending
            && x.Token == token
            && x.TokenExpiry >= permissionService.Now);
    }

    public static IQueryable<CollectionPermissionEntity> WhereCanDeletePermission(
        this IQueryable<CollectionPermissionEntity> q,
        IPermissionService permissionService)
    {
        return q.Where(x =>
            x.Role != CollectionPermissionRole.Owner
            && x.IamUserId != permissionService.UserId);
    }

    public static CollectionPermissionUserPermissions Build(CollectionPermissionEntity permission)
    {
        return new CollectionPermissionUserPermissions(CanResend(permission));
    }

    private static bool CanResend(CollectionPermissionEntity permission)
    {
        return permission.State is CollectionPermissionState.Pending or CollectionPermissionState.Rejected or CollectionPermissionState.Expired;
    }
}
