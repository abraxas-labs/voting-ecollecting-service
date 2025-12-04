// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Abstractions.Adapter.Data.Repositories;

public interface ICollectionCitizenRepository : IHasAuditTrailTrackedEntityRepository<CollectionCitizenEntity>;
