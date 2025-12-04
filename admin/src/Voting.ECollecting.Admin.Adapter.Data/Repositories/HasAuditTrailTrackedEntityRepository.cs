// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities.Audit;
using Voting.Lib.Database.Models;

namespace Voting.ECollecting.Admin.Adapter.Data.Repositories;

public abstract class HasAuditTrailTrackedEntityRepository<TAuditTrailTrackedEntity>(DataContext context)
    : Shared.Adapter.Data.Repositories.HasAuditTrailTrackedEntityRepository<DataContext, TAuditTrailTrackedEntity>(context)
    where TAuditTrailTrackedEntity : BaseEntity, IAuditTrailTrackedEntity, new()
{
}
