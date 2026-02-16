// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Domain.Queries;

public static class CollectionQueries
{
    public static IQueryable<T> WhereHasPendingOrRejectedOrExpiredPermission<T>(this IQueryable<T> q, Guid permissionId)
        where T : CollectionBaseEntity
    {
        return q.Where(p => p.Permissions!.Any(x =>
            x.Id == permissionId && (x.State == CollectionPermissionState.Pending ||
                                     x.State == CollectionPermissionState.Rejected ||
                                     x.State == CollectionPermissionState.Expired)));
    }

    public static IQueryable<T> IncludePermission<T>(this IQueryable<T> q, string userId)
        where T : CollectionBaseEntity
    {
        return q.Include(p => p.Permissions!.Where(x => x.IamUserId == userId));
    }

    public static IQueryable<T> IncludePendingOrRejectedOrExpiredPermission<T>(this IQueryable<T> q, Guid permissionId)
        where T : CollectionBaseEntity
    {
        return q.Include(p => p.Permissions!.Where(x =>
            x.Id == permissionId && (x.State == CollectionPermissionState.Pending ||
                                     x.State == CollectionPermissionState.Rejected ||
                                     x.State == CollectionPermissionState.Expired)));
    }

    public static IIncludableQueryable<TEntity, IEnumerable<CollectionPermissionEntity>> ThenIncludePermission<TEntity, TPreviousProperty>(
        this IIncludableQueryable<TEntity, IEnumerable<TPreviousProperty>> q,
        string userId)
        where TEntity : class
        where TPreviousProperty : CollectionBaseEntity
    {
        return q.ThenInclude(p => p.Permissions!.Where(x => x.IamUserId == userId));
    }
}
