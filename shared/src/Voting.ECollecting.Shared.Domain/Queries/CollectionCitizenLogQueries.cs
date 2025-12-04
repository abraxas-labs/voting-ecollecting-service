// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Domain.Queries;

public static class CollectionCitizenLogQueries
{
    public static IQueryable<CollectionCitizenLogEntity> WhereIsSigned(
        this IQueryable<CollectionCitizenLogEntity> query,
        Guid collectionId,
        byte[] mac)
    {
        return query.Where(x => x.CollectionId == collectionId && x.VotingStimmregisterIdMac == mac);
    }

    public static IQueryable<CollectionCitizenLogEntity> WhereIsSigned(
        this IQueryable<CollectionCitizenLogEntity> query,
        Guid collectionId,
        IReadOnlyList<byte[]> macs)
    {
        return query.Where(x => x.CollectionId == collectionId && macs.Contains(x.VotingStimmregisterIdMac));
    }
}
