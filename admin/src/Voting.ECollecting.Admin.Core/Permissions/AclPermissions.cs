// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Core.Permissions;

internal static class AclPermissions
{
    public static IQueryable<T> WhereCanAccessOwnBfsOrChildren<T>(this IQueryable<T> query, IPermissionService permissionService)
        where T : class, IHasBfs
    {
        return query.Where(x => x.Bfs != null && permissionService.AclBfsLists.BfsInclChildren.Contains(x.Bfs));
    }

    public static bool CanAccessOwnBfsOrChildren(IPermissionService permissionService, IHasBfs entity)
        => entity.Bfs != null && permissionService.AclBfsLists.BfsInclChildren.Contains(entity.Bfs);

    public static IQueryable<T> WhereCanAccessOwnBfsOrChildrenOrParents<T>(this IQueryable<T> query, IPermissionService permissionService)
        where T : class, IHasBfs
    {
        return query.Where(x => x.Bfs != null && permissionService.AclBfsLists.BfsInclChildrenAndParents.Contains(x.Bfs));
    }

    public static bool CanAccessOwnBfsOrChildrenOrParents(IPermissionService permissionService, IHasBfs entity)
        => entity.Bfs != null && permissionService.AclBfsLists.BfsInclChildrenAndParents.Contains(entity.Bfs);

    public static IQueryable<T> WhereCanAccessOwnBfs<T>(this IQueryable<T> query, IPermissionService permissionService)
        where T : class, IHasBfs
    {
        return query.Where(x => x.Bfs != null && permissionService.AclBfsLists.Bfs.Contains(x.Bfs));
    }

    public static bool CanAccessOwnBfs(IPermissionService permissionService, IHasBfs entity)
        => entity.Bfs != null && permissionService.AclBfsLists.Bfs.Contains(entity.Bfs);

    public static IQueryable<T> WhereCanAccessOwnMunicipalityBfsInclParents<T>(this IQueryable<T> query, IPermissionService permissionService)
        where T : class, IHasBfs
    {
        return query.Where(x => x.Bfs != null && permissionService.AclBfsLists.BfsMunicipalitiesInclParents.Contains(x.Bfs));
    }

    public static bool CanAccessOwnMunicipalityBfsInclParents(IPermissionService permissionService, IHasBfs entity)
    {
        return entity.Bfs != null && permissionService.AclBfsLists.BfsMunicipalitiesInclParents.Contains(entity.Bfs);
    }

    public static IQueryable<T> WhereHasRole<T>(
        this IQueryable<T> query,
        IPermissionService permissionService,
        string role)
    {
        return permissionService.HasRole(role)
            ? query
            : query.Where(_ => false);
    }

    public static bool HasRole(IPermissionService permissionService, string role)
        => permissionService.HasRole(role);

    public static IQueryable<T> WhereHasAnyRole<T>(
        this IQueryable<T> query,
        IPermissionService permissionService,
        string[] roles)
    {
        return roles.Any(permissionService.HasRole)
            ? query
            : query.Where(_ => false);
    }

    public static bool HasAnyRole(IPermissionService permissionService, string[] roles)
        => roles.Any(permissionService.HasRole);
}
