// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Adapter.Data.Repositories;

/// <inheritdoc cref="IDecreeRepository"/>
public class DecreeRepository(DataContext context) : HasAuditTrailTrackedEntityRepository<DecreeEntity>(context), IDecreeRepository;
