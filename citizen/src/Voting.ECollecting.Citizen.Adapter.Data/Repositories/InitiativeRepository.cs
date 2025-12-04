// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Citizen.Adapter.Data.Repositories;

public class InitiativeRepository(DataContext context) : HasAuditTrailTrackedEntityRepository<InitiativeEntity>(context), IInitiativeRepository;
