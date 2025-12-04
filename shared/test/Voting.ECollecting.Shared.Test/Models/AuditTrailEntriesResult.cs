// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities.Audit;

namespace Voting.ECollecting.Shared.Test.Models;

public record AuditTrailEntriesResult(
    List<AuditTrailEntryEntity> AuditTrailEntries,
    List<CollectionCitizenLogAuditTrailEntryEntity> CollectionCitizenLogAuditTrailEntries);
