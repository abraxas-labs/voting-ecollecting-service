// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Entities;

public interface IHasCollectionPeriod
{
    DateOnly? CollectionStartDate { get; }

    DateOnly? CollectionEndDate { get; }
}
