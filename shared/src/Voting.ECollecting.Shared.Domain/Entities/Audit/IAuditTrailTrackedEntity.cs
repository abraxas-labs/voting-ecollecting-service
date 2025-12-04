// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Entities.Audit;

public interface IAuditTrailTrackedEntity
{
    public AuditInfo AuditInfo { get; }
}
