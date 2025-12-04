// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities.Audit;

namespace Voting.ECollecting.Shared.Adapter.Data.Models;

public record AuditTrailEntryBuilderResult(
    IReadOnlyCollection<AuditTrailEntryEntity> AuditTrailEntries,
    IReadOnlyCollection<CollectionCitizenLogAuditTrailEntryEntity> CollectionCitizenLogAuditTrailEntries);
