// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Text.Json;

namespace Voting.ECollecting.Shared.Domain.Entities.Audit;

public class CollectionCitizenLogAuditTrailEntryEntity : AuditedEntity
{
    public Guid CollectionId { get; set; }

    public string Action { get; set; } = string.Empty;

    public JsonDocument? RecordBefore { get; set; }

    public JsonDocument? RecordAfter { get; set; }

    public Guid SourceEntityId { get; set; }

    public CollectionCitizenLogEntity? SourceEntity { get; set; }
}
