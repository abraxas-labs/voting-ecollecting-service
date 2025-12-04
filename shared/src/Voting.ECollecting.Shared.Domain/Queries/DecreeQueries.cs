// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Shared.Domain.Queries;

public static class DecreeQueries
{
    public static IQueryable<DecreeEntity> WhereInPeriodState(
        this IQueryable<DecreeEntity> query,
        CollectionPeriodState periodState,
        DateTime utcNow)
    {
        return periodState switch
        {
            CollectionPeriodState.Published => query.Where(x => x.CollectionStartDate > utcNow),
            CollectionPeriodState.InCollection => query.Where(x => x.CollectionStartDate <= utcNow && x.CollectionEndDate >= utcNow),
            CollectionPeriodState.Expired => query.Where(x => x.CollectionEndDate < utcNow),
            CollectionPeriodState.Unspecified => query,
            _ => throw new ArgumentOutOfRangeException(nameof(periodState), periodState, null),
        };
    }

    public static IQueryable<DecreeEntity> WhereInCollectionOrPublished(this IQueryable<DecreeEntity> query, DateTime utcNow)
    {
        return query.Where(x => x.CollectionEndDate >= utcNow);
    }

    public static IQueryable<DecreeEntity> WhereInCollectionOrExpired(this IQueryable<DecreeEntity> query, DateTime utcNow)
    {
        return query.Where(x => x.CollectionStartDate <= utcNow);
    }

    public static IQueryable<DecreeEntity> WhereInState(this IQueryable<DecreeEntity> query, DecreeState state)
    {
        return query.Where(x => x.State == state);
    }

    public static IQueryable<DecreeEntity> WhereDoiTypeIsEnabled(
        this IQueryable<DecreeEntity> query,
        IReadOnlySet<DomainOfInfluenceType> enabledDoiTypes)
    {
        return query.Where(x => enabledDoiTypes.Contains(x.DomainOfInfluenceType));
    }
}
