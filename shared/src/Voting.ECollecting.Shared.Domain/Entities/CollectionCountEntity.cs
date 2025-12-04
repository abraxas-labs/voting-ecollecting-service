// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities.IntegritySignature;

namespace Voting.ECollecting.Shared.Domain.Entities;

public class CollectionCountEntity : IntegritySignatureEntity
{
    public Guid CollectionId { get; set; }

    public int TotalCitizenCount { get; set; }

    public int ElectronicCitizenCount { get; set; }

    public CollectionBaseEntity Collection { get; set; } = null!;
}
