// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Shared.Domain.Queries;

public static class CollectionQueries
{
    public static IQueryable<T> WhereIsEnabledForCollection<T>(this IQueryable<T> q)
        where T : CollectionBaseEntity
    {
        return q.Where(x => x.State == CollectionState.EnabledForCollection);
    }

    public static IQueryable<T> WhereDoiTypeIsEnabled<T>(
        this IQueryable<T> query,
        IReadOnlySet<DomainOfInfluenceType> enabledDoiTypes)
        where T : CollectionBaseEntity
    {
        return query.Where(x =>
            !x.DomainOfInfluenceType.HasValue
            || enabledDoiTypes.Contains(x.DomainOfInfluenceType.Value));
    }

    public static IQueryable<T> WhereInPeriodState<T>(
        this IQueryable<T> query,
        CollectionPeriodState periodState,
        bool requireEnabledForCollection,
        DateOnly today)
        where T : CollectionBaseEntity
    {
        if (requireEnabledForCollection)
        {
            query = query.Where(x => x.State == CollectionState.EnabledForCollection);
        }

        return periodState switch
        {
            CollectionPeriodState.Published => query.Where(x => x.CollectionStartDate > today),
            CollectionPeriodState.InCollection => query.Where(x => x.CollectionStartDate <= today && x.CollectionEndDate >= today),
            CollectionPeriodState.Expired => query.Where(x => x.CollectionEndDate < today),
            CollectionPeriodState.Unspecified => query.Where(x => x.CollectionStartDate.HasValue && x.CollectionEndDate.HasValue),
            _ => throw new ArgumentOutOfRangeException(nameof(periodState), periodState, null),
        };
    }

    public static IQueryable<T> WhereInPeriodStateUnspecified<T>(this IQueryable<T> query)
        where T : CollectionBaseEntity
    {
        return query.Where(x => !x.CollectionStartDate.HasValue || !x.CollectionEndDate.HasValue);
    }

    public static IQueryable<T> WhereInPeriodStatePublishedOrUnspecified<T>(this IQueryable<T> query, DateOnly today)
        where T : CollectionBaseEntity
    {
        return query.Where(x => !x.CollectionStartDate.HasValue || x.CollectionStartDate > today);
    }

    public static IQueryable<T> WhereInPeriodStateInCollectionOrExpired<T>(this IQueryable<T> query, DateOnly today)
        where T : CollectionBaseEntity
    {
        return query.Where(x => x.CollectionStartDate <= today);
    }

    public static IQueryable<T> WhereInState<T>(this IQueryable<T> q, CollectionState state)
        where T : CollectionBaseEntity
    {
        return q.Where(x => x.State == state);
    }

    public static IQueryable<T> WhereInPreparationOrReturnedForCorrection<T>(this IQueryable<T> q)
        where T : CollectionBaseEntity
    {
        return q.Where(x => x.State == CollectionState.InPreparation || x.State == CollectionState.ReturnedForCorrection);
    }

    public static IQueryable<T> WhereIsElectronicSubmission<T>(this IQueryable<T> q)
        where T : CollectionBaseEntity
    {
        return q.Where(x => x.IsElectronicSubmission);
    }

    public static IQueryable<T> WhereIsPaperSubmission<T>(this IQueryable<T> q)
        where T : CollectionBaseEntity
    {
        return q.Where(x => !x.IsElectronicSubmission);
    }

    public static IQueryable<T> WhereIsNotEnded<T>(this IQueryable<T> q)
        where T : CollectionBaseEntity
    {
        return q.Where(x =>
            x.State != CollectionState.SignatureSheetsSubmitted
            && x.State != CollectionState.EndedCameAbout
            && x.State != CollectionState.EndedCameNotAbout);
    }

    public static IQueryable<T> WhereIsEnded<T>(this IQueryable<T> q)
        where T : CollectionBaseEntity
    {
        return q.Where(x =>
            x.State == CollectionState.SignatureSheetsSubmitted
            || x.State == CollectionState.EndedCameAbout
            || x.State == CollectionState.EndedCameNotAbout);
    }

    public static IQueryable<T> WhereIsNotEndedAndNotAborted<T>(this IQueryable<T> q)
        where T : CollectionBaseEntity
    {
        return q.Where(x => x.State != CollectionState.Withdrawn
                            && x.State != CollectionState.NotPassed
                            && x.State != CollectionState.SignatureSheetsSubmitted
                            && x.State != CollectionState.EndedCameAbout
                            && x.State != CollectionState.EndedCameNotAbout);
    }

    public static IQueryable<T> WhereIsEnabledForCollectionOrEnded<T>(this IQueryable<T> q)
        where T : CollectionBaseEntity
    {
        return q.Where(x => x.State == CollectionState.EnabledForCollection
                            || x.State == CollectionState.SignatureSheetsSubmitted
                            || x.State == CollectionState.EndedCameAbout
                            || x.State == CollectionState.EndedCameNotAbout);
    }
}
