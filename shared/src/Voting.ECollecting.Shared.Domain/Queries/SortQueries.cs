// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq.Expressions;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Shared.Domain.Queries;

public static class SortQueries
{
    public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, TKey>> keySelector,
        SortDirection sortDirection)
    {
        return sortDirection switch
        {
            SortDirection.Unspecified => source.OrderBy(keySelector),
            SortDirection.Ascending => source.OrderBy(keySelector),
            SortDirection.Descending => source.OrderByDescending(keySelector),
            _ => throw new ArgumentOutOfRangeException(nameof(sortDirection), sortDirection, null),
        };
    }
}
