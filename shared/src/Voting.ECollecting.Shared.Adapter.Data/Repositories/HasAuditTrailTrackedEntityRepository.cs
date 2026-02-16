// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Voting.ECollecting.Shared.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Shared.Domain.Entities.Audit;
using Voting.Lib.Database.Models;
using Voting.Lib.Database.Repositories;

namespace Voting.ECollecting.Shared.Adapter.Data.Repositories;

public abstract class HasAuditTrailTrackedEntityRepository<TDbContext, TAuditTrailTrackedEntity>(TDbContext context) : DbRepository<TDbContext, TAuditTrailTrackedEntity>(context), IHasAuditTrailTrackedEntityRepository<TAuditTrailTrackedEntity>
    where TDbContext : DbContext
    where TAuditTrailTrackedEntity : BaseEntity, IAuditTrailTrackedEntity, new()
{
    public async Task<int> AuditedUpdateRange(
        Func<IQueryable<TAuditTrailTrackedEntity>, IQueryable<TAuditTrailTrackedEntity>> predicate,
        Action<TAuditTrailTrackedEntity> updateAction)
    {
        await using var transaction = await BeginTransactionIfNotActive();

        var query = Set.AsQueryable();
        query = predicate(query);
        var entities = await query.ToListAsync();

        Detach(entities.Select(e => e.Id).ToHashSet());

        foreach (var entity in entities)
        {
            SetEntityState(entity, EntityState.Unchanged);
            updateAction(entity);
            SetEntityState(entity, EntityState.Modified);
        }

        await SaveChangesAndHandleTransaction(transaction, entities.Count * 2);
        return entities.Count;
    }

    public async Task AuditedUpdateRange(
        IEnumerable<TAuditTrailTrackedEntity> originalValues,
        Func<TAuditTrailTrackedEntity, Task> updateAction)
    {
        await using var transaction = await BeginTransactionIfNotActive();

        Detach(originalValues.Select(v => v.Id).ToHashSet());

        foreach (var originalValue in originalValues)
        {
            SetEntityState(originalValue, EntityState.Unchanged);

            await updateAction(originalValue);
            SetEntityState(originalValue, EntityState.Modified);
        }

        await SaveChangesAndHandleTransaction(transaction);
    }

    public async Task AuditedUpdate(
        TAuditTrailTrackedEntity originalValue,
        Action updateAction,
        int expectedAffectedEntities = 1)
    {
        await using var transaction = await BeginTransactionIfNotActive();

        Detach(new HashSet<Guid> { originalValue.Id });
        SetEntityState(originalValue, EntityState.Unchanged);

        updateAction();
        SetEntityState(originalValue, EntityState.Modified);

        await SaveChangesAndHandleTransaction(transaction, 2 * expectedAffectedEntities);
    }

    public async Task AuditedUpdate(
        TAuditTrailTrackedEntity originalValue,
        Func<Task> updateAction)
    {
        await using var transaction = await BeginTransactionIfNotActive();

        Detach(new HashSet<Guid> { originalValue.Id });
        SetEntityState(originalValue, EntityState.Unchanged);

        await updateAction();
        SetEntityState(originalValue, EntityState.Modified);

        await SaveChangesAndHandleTransaction(transaction, 2);
    }

    public async Task<int> AuditedDeleteRange(
        Func<IQueryable<TAuditTrailTrackedEntity>, IQueryable<TAuditTrailTrackedEntity>> predicate)
    {
        await using var transaction = await BeginTransactionIfNotActive();

        var query = Set.AsQueryable();
        query = predicate(query);
        var entities = await query.ToListAsync();

        foreach (var entity in entities)
        {
            SetEntityState(entity, EntityState.Deleted);
        }

        await SaveChangesAndHandleTransaction(transaction, entities.Count * 2);
        return entities.Count;
    }

    public async Task AuditedDelete(TAuditTrailTrackedEntity originalValue)
    {
        await using var transaction = await BeginTransactionIfNotActive();

        Detach(new HashSet<Guid> { originalValue.Id });
        SetEntityState(originalValue, EntityState.Deleted);

        await SaveChangesAndHandleTransaction(transaction, 2);
    }

    public override Task Update(TAuditTrailTrackedEntity value)
    {
        // Update must not be used to update audit trail tracked entities. Use AuditedUpdate instead.
        // This is to ensure that entities are correctly tracked (initial values and updated values) for the audit trail.
        throw new NotSupportedException();
    }

    public override Task UpdateRange(IEnumerable<TAuditTrailTrackedEntity> values)
    {
        // UpdateRange must not be used to update audit trail tracked entities. Use AuditedUpdateRange instead.
        // This is to ensure that entities are correctly tracked (initial values and updated values) for the audit trail.
        throw new NotSupportedException();
    }

    public override Task<bool> DeleteByKeyIfExists(Guid key)
    {
        // DeleteByKeyIfExists must not be used to delete audit trail tracked entities. Use AuditedDelete instead.
        // This is to ensure that entities are correctly tracked (initial values) for the audit trail.
        throw new NotSupportedException();
    }

    public override Task DeleteRangeByKey(IEnumerable<Guid> keys)
    {
        // DeleteRangeByKey must not be used to update audit trail tracked entities. Use AuditedDeleteRange instead.
        // This is to ensure that entities are correctly tracked (initial values) for the audit trail.
        throw new NotSupportedException();
    }

    private void SetEntityState(TAuditTrailTrackedEntity entity, EntityState entityState)
    {
        SetEntityState(Context.Entry(entity), entityState);
    }

    private void SetEntityState(EntityEntry entry, EntityState entityState)
    {
        entry.State = entityState;

        foreach (var navigationMetadata in entry.Navigations.Select(n => n.Metadata))
        {
            if (navigationMetadata.TargetEntityType == null)
            {
                continue;
            }

            if (navigationMetadata.TargetEntityType.IsOwned())
            {
                var ownedEntry = entry.Reference(navigationMetadata.Name).TargetEntry;
                if (ownedEntry != null)
                {
                    ownedEntry.State = entityState;
                }
            }
        }
    }

    private async Task SaveChangesAndHandleTransaction(IDbContextTransaction? transaction, int? expectedAffectedRows = null)
    {
        var affectedRows = await Context.SaveChangesAsync();

        if (expectedAffectedRows.HasValue && affectedRows != expectedAffectedRows)
        {
            if (transaction != null)
            {
                await transaction.DisposeAsync();
            }

            throw new DbUpdateConcurrencyException($"Expected operation to affect {expectedAffectedRows} rows, but it affected {affectedRows}");
        }

        if (transaction != null)
        {
            await transaction.CommitAsync();
        }
    }

    private void Detach(IReadOnlySet<Guid> ids)
    {
        foreach (var existingEntry in Context.ChangeTracker.Entries<TAuditTrailTrackedEntity>().Where(e => ids.Contains(e.Entity.Id)))
        {
            SetEntityState(existingEntry, EntityState.Detached);
        }
    }

    private async Task<IDbContextTransaction?> BeginTransactionIfNotActive()
    {
        return Context.Database.CurrentTransaction == null
            ? await Context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted)
            : null;
    }
}
