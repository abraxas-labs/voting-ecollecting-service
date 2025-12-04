// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities.Audit;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Shared.Domain.Entities;

public class CollectionSignatureSheetEntity : AuditedEntity, IAuditTrailTrackedEntity
{
    public CollectionMunicipalityEntity? CollectionMunicipality { get; set; }

    public Guid CollectionMunicipalityId { get; set; }

    public int Number { get; set; }

    /// <summary>
    /// Gets or sets the date the signature sheet was received ("Eingangsdatum").
    /// </summary>
    public DateTime ReceivedAt { get; set; }

    public DateTime? AttestedAt { get; set; }

    public CollectionSignatureSheetCount Count { get; set; } = new();

    public CollectionSignatureSheetState State { get; set; } = CollectionSignatureSheetState.Created;

    public ICollection<CollectionCitizenEntity> Citizens { get; set; } = [];

    public bool IsSample { get; set; }
}
