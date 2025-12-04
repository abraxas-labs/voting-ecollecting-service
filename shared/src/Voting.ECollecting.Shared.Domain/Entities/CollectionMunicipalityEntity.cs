// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities.Audit;

namespace Voting.ECollecting.Shared.Domain.Entities;

public class CollectionMunicipalityEntity : AuditedEntity, IHasBfs, IAuditTrailTrackedEntity
{
    public CollectionBaseEntity? Collection { get; set; }

    public Guid CollectionId { get; set; }

    public string Bfs { get; set; } = string.Empty;

    public string MunicipalityName { get; set; } = string.Empty;

    public CollectionSignatureSheetCount PhysicalCount { get; set; } = new();

    public int ElectronicCitizenCount { get; set; }

    public List<CollectionSignatureSheetEntity>? SignatureSheets { get; set; }

    public List<CollectionCitizenEntity>? Citizens { get; set; }

    public bool IsLocked { get; set; }

    public int NextSheetNumber { get; set; }

    public int TotalValidCitizenCount => ElectronicCitizenCount + PhysicalCount.Valid;

    public CollectionMunicipalitySignatureSheetsCount? SignatureSheetsCount { get; set; }
}
