// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities.Audit;

namespace Voting.ECollecting.Shared.Domain.Entities;

public class CollectionMessageEntity : AuditedEntity
{
    public Guid CollectionId { get; set; }

    public CollectionBaseEntity? Collection { get; set; }

    public string Content { get; set; } = string.Empty;
}
