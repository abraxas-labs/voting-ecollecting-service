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
        DateOnly today)
    {
        return periodState switch
        {
            CollectionPeriodState.Published => query.Where(x => x.CollectionStartDate > today),
            CollectionPeriodState.InCollection => query.Where(x => x.CollectionStartDate <= today && x.CollectionEndDate >= today),
            CollectionPeriodState.Expired => query.Where(x => x.CollectionEndDate < today),
            CollectionPeriodState.Unspecified => query,
            _ => throw new ArgumentOutOfRangeException(nameof(periodState), periodState, null),
        };
    }

    public static IQueryable<DecreeEntity> WhereInCollectionOrPublished(this IQueryable<DecreeEntity> query, DateOnly today)
    {
        return query.Where(x => x.CollectionEndDate >= today);
    }

    public static IQueryable<DecreeEntity> WhereInCollectionOrExpired(this IQueryable<DecreeEntity> query, DateOnly today)
    {
        return query.Where(x => x.CollectionStartDate <= today);
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
