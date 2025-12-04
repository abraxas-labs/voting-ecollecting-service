// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Models;

public class NullableCollectionCount
{
    public Guid Id { get; set; }

    public Guid CollectionId { get; set; }

    public int? TotalCitizenCount { get; set; }

    public int ElectronicCitizenCount { get; set; }
}
