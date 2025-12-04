// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin;
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
}
