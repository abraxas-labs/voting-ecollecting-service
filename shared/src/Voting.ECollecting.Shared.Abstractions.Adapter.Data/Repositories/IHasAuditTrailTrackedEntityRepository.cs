// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Shared.Domain.Entities.Audit;
using Voting.Lib.Database.Models;
using Voting.Lib.Database.Repositories;

namespace Voting.ECollecting.Shared.Abstractions.Adapter.Data.Repositories;

public interface IHasAuditTrailTrackedEntityRepository<TAuditTrailTrackedEntity> : IDbRepository<DbContext, TAuditTrailTrackedEntity>
    where TAuditTrailTrackedEntity : BaseEntity, IAuditTrailTrackedEntity, new()
{
    Task<int> AuditedDeleteRange(Func<IQueryable<TAuditTrailTrackedEntity>, IQueryable<TAuditTrailTrackedEntity>> predicate);

    Task<int> AuditedUpdateRange(
        Func<IQueryable<TAuditTrailTrackedEntity>, IQueryable<TAuditTrailTrackedEntity>> predicate,
        Action<TAuditTrailTrackedEntity> updateAction);

    Task AuditedUpdateRange(
        IEnumerable<TAuditTrailTrackedEntity> originalValues,
        Func<TAuditTrailTrackedEntity, Task> updateAction);

    Task AuditedUpdate(
        TAuditTrailTrackedEntity originalValue,
        Action updateAction,
        int expectedAffectedEntities = 1);

    Task AuditedUpdate(
        TAuditTrailTrackedEntity originalValue,
        Func<Task> updateAction);

    Task AuditedDelete(TAuditTrailTrackedEntity originalValue);
}
