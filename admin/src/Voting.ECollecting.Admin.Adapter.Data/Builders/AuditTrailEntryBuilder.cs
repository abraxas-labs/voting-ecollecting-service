// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Shared.Domain.Entities.Audit;

namespace Voting.ECollecting.Admin.Adapter.Data.Builders;

public class AuditTrailEntryBuilder(IPermissionService permissionService) : Shared.Adapter.Data.Builders.AuditTrailEntryBuilder
{
    protected override AuditTrailEntryEntity CreateAuditTrailEntry(EntityEntry entry)
    {
        var auditTrailEntry = base.CreateAuditTrailEntry(entry);
        permissionService.SetCreated(auditTrailEntry);
        return auditTrailEntry;
    }

    protected override CollectionCitizenLogAuditTrailEntryEntity CreateCollectionCitizenLogAuditTrailEntry(EntityEntry entry)
    {
        var auditTrailEntry = base.CreateCollectionCitizenLogAuditTrailEntry(entry);
        permissionService.SetCreated(auditTrailEntry);
        return auditTrailEntry;
    }
}
