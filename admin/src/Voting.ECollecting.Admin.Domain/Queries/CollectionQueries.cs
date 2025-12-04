// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Admin.Domain.Queries;

public static class CollectionQueries
{
    public static IQueryable<T> IncludeMunicipalities<T>(this IQueryable<T> q, AclBfsLists aclBfsLists)
        where T : CollectionBaseEntity
    {
        return q.Include(x => x.Municipalities!.Where(y => aclBfsLists.BfsMunicipalities.Contains(y.Bfs)));
    }

    public static IIncludableQueryable<TEntity, IEnumerable<CollectionMunicipalityEntity>> ThenIncludeMunicipalities<TEntity, TPreviousProperty>(
        this IIncludableQueryable<TEntity, IEnumerable<TPreviousProperty>> q,
        AclBfsLists aclBfsLists)
        where TEntity : class
        where TPreviousProperty : CollectionBaseEntity
    {
        return q.ThenInclude(x => x.Municipalities!.Where(y => aclBfsLists.BfsMunicipalities.Contains(y.Bfs)));
    }

    public static IQueryable<T> IncludeAcceptedDeputyPermissions<T>(this IQueryable<T> q)
        where T : CollectionBaseEntity
    {
        return q.Include(x => x.Permissions!.Where(p =>
            p.State == CollectionPermissionState.Accepted
            && p.Role == CollectionPermissionRole.Deputy));
    }
}
