// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Domain.Queries;

public static class DecreeQueries
{
    public static IIncludableQueryable<DecreeEntity, IEnumerable<ReferendumEntity>> IncludeFilteredReferendums(this IQueryable<DecreeEntity> q, AclBfsLists aclBfsLists, DateOnly today)
    {
        return q.Include<DecreeEntity, IEnumerable<ReferendumEntity>>(x => x.Collections.Where(y =>
            y.Bfs != null && (aclBfsLists.BfsInclChildren.Contains(y.Bfs) ||
                              (aclBfsLists.ParentsBfs.Contains(y.Bfs) && y.CollectionStartDate <= today))));
    }
}
