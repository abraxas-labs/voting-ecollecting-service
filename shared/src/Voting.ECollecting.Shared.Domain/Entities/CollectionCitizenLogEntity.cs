// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities.Audit;
using Voting.ECollecting.Shared.Domain.Entities.IntegritySignature;

namespace Voting.ECollecting.Shared.Domain.Entities;

public class CollectionCitizenLogEntity : IntegritySignatureEntity
{
    /// <summary>
    /// Gets or sets the collection ID.
    /// Denormalized FK to speed up has already signed queries.
    /// </summary>
    public Guid CollectionId { get; set; }

    public CollectionBaseEntity? Collection { get; set; }

    public Guid CollectionCitizenId { get; set; }

    public CollectionCitizenEntity? CollectionCitizen { get; set; }

    /// <summary>
    /// Gets or sets the encrypted Stimmregister ID.
    /// Should only be set for physical signatures.
    /// </summary>
    public byte[] VotingStimmregisterIdEncrypted { get; set; } = [];

    public byte[] VotingStimmregisterIdMac { get; set; } = [];

    public List<CollectionCitizenLogAuditTrailEntryEntity>? AuditTrailEntries { get; set; }
}
