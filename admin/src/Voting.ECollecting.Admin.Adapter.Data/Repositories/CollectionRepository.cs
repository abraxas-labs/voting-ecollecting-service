// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Database.Postgres.Locking;

namespace Voting.ECollecting.Admin.Adapter.Data.Repositories;

/// <inheritdoc cref="ICollectionRepository"/>
public class CollectionRepository(DataContext context) : HasAuditTrailTrackedEntityRepository<CollectionBaseEntity>(context), ICollectionRepository
{
    public async Task<List<CollectionBaseEntity>> FetchAndLockPreparingForCollection()
    {
        return await Query()
            .Where(x => x.State == CollectionState.PreparingForCollection)
            .ForUpdateSkipLocked()
            .ToListAsync();
    }
}
