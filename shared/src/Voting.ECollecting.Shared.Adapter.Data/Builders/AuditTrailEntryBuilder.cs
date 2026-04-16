// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Voting.ECollecting.Shared.Adapter.Data.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Entities.Audit;
using Voting.Lib.Database.Models;

namespace Voting.ECollecting.Shared.Adapter.Data.Builders;

public abstract class AuditTrailEntryBuilder : IAuditTrailEntryBuilder
{
    public AuditTrailEntryBuilderResult BuildAuditTrailEntries(
        DbContext dbContext)
    {
        dbContext.ChangeTracker.DetectChanges();

        var auditTrailTrackedEntityEntries = dbContext.ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditTrailTrackedEntity)
            .OrderBy(e => ((IAuditTrailTrackedEntity)e.Entity).AuditInfo.CreatedAt)
            .ThenBy(e => ((IAuditTrailTrackedEntity)e.Entity).AuditInfo.ModifiedAt)
            .ThenBy(e => ((BaseEntity)e.Entity).Id)
            .ToList();

        var auditTrailEntries = BuildAuditTrailEntries(auditTrailTrackedEntityEntries);
        var collectionCitizenLogAuditTrailEntries = BuildCollectionCitizenLogAuditTrailEntries(auditTrailTrackedEntityEntries);

        return new(auditTrailEntries, collectionCitizenLogAuditTrailEntries);
    }

    protected virtual AuditTrailEntryEntity CreateAuditTrailEntry(EntityEntry entry)
    {
        var (recordBefore, recordAfter) = BuildRecordBeforeAndAfter(entry);

        return new AuditTrailEntryEntity
        {
            Action = entry.State.ToString(),
            SourceEntityId = ((BaseEntity)entry.Entity).Id,
            SourceEntityName = entry.Metadata.GetTableName()!,
            RecordBefore = recordBefore,
            RecordAfter = recordAfter,
        };
    }

    protected virtual CollectionCitizenLogAuditTrailEntryEntity CreateCollectionCitizenLogAuditTrailEntry(EntityEntry entry)
    {
        var (recordBefore, recordAfter) = BuildRecordBeforeAndAfter(entry);

        var collectionCitizenLog = entry.Entity as CollectionCitizenLogEntity
            ?? throw new InvalidCastException();

        return new CollectionCitizenLogAuditTrailEntryEntity
        {
            Action = entry.State.ToString(),
            CollectionId = collectionCitizenLog.CollectionId,
            SourceEntityId = collectionCitizenLog.Id,
            RecordBefore = recordBefore,
            RecordAfter = recordAfter,
        };
    }

    private List<AuditTrailEntryEntity> BuildAuditTrailEntries(List<EntityEntry> auditTrailTrackedEntityEntries)
    {
        var auditTrailEntries = new List<AuditTrailEntryEntity>();

        foreach (var entry in auditTrailTrackedEntityEntries)
        {
            MarkUnchangedPricipalAsModifiedOnOwnedEntitesModifications(entry);

            if (entry.State is EntityState.Detached or EntityState.Unchanged
                || entry.Entity is CollectionCitizenLogEntity)
            {
                continue;
            }

            auditTrailEntries.Add(CreateAuditTrailEntry(entry));
        }

        return auditTrailEntries;
    }

    private List<CollectionCitizenLogAuditTrailEntryEntity> BuildCollectionCitizenLogAuditTrailEntries(List<EntityEntry> auditTrailTrackedEntityEntries)
    {
        var auditTrailEntries = new List<CollectionCitizenLogAuditTrailEntryEntity>();

        foreach (var entry in auditTrailTrackedEntityEntries)
        {
            MarkUnchangedPricipalAsModifiedOnOwnedEntitesModifications(entry);

            if (entry.State is EntityState.Detached or EntityState.Unchanged
                || entry.Entity is not CollectionCitizenLogEntity)
            {
                continue;
            }

            auditTrailEntries.Add(CreateCollectionCitizenLogAuditTrailEntry(entry));
        }

        return auditTrailEntries;
    }

    private void MarkUnchangedPricipalAsModifiedOnOwnedEntitesModifications(EntityEntry entry)
    {
        if (entry.State is not EntityState.Unchanged)
        {
            return;
        }

        foreach (var navigationMetadata in entry.Navigations.Select(n => n.Metadata))
        {
            if (navigationMetadata.TargetEntityType == null)
            {
                continue;
            }

            if (navigationMetadata.TargetEntityType.IsOwned())
            {
                var ownedEntry = entry.Reference(navigationMetadata.Name).TargetEntry;
                if (ownedEntry?.Properties.Any(p => p.IsModified) == true)
                {
                    entry.State = EntityState.Modified;
                    return;
                }
            }
        }
    }

    private (JsonDocument? RecordBefore, JsonDocument? RecordAfter) BuildRecordBeforeAndAfter(EntityEntry entry)
    {
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();

        foreach (var prop in entry.Properties)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    newValues[prop.Metadata.Name] = prop.CurrentValue;
                    break;

                case EntityState.Deleted:
                    oldValues[prop.Metadata.Name] = prop.OriginalValue;
                    break;

                case EntityState.Modified:
                    oldValues[prop.Metadata.Name] = prop.OriginalValue;
                    newValues[prop.Metadata.Name] = prop.CurrentValue;
                    break;
            }
        }

        foreach (var navigation in entry.Metadata.GetNavigations())
        {
            if (!navigation.TargetEntityType.IsOwned())
            {
                continue;
            }

            var ownedPropertyName = navigation.Name;
            var refereceEntry = entry.Reference(ownedPropertyName);
            var ownedEntry = refereceEntry.TargetEntry;

            switch (entry.State)
            {
                case EntityState.Added:
                    newValues[ownedPropertyName] = ownedEntry?.CurrentValues.ToObject();
                    break;

                case EntityState.Deleted:
                    oldValues[ownedPropertyName] = ownedEntry?.OriginalValues.ToObject();
                    break;

                case EntityState.Modified:
                    oldValues[ownedPropertyName] = ownedEntry?.OriginalValues.ToObject();
                    newValues[ownedPropertyName] = ownedEntry?.CurrentValues.ToObject();
                    break;
            }
        }

        return (
            oldValues.Count > 0 ? JsonDocument.Parse(JsonSerializer.Serialize(oldValues)) : null,
            newValues.Count > 0 ? JsonDocument.Parse(JsonSerializer.Serialize(newValues)) : null);
    }
}
