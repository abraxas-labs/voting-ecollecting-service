// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;

public interface ICollectionRepository : Shared.Abstractions.Adapter.Data.Repositories.ICollectionRepository
{
    Task<List<CollectionBaseEntity>> FetchAndLockPreparingForCollection();
}
