// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Abstractions.Core.Services;

public interface IInitiativeService
{
    Task<List<Initiative>> ListMy();

    Task<List<InitiativeSubTypeEntity>> ListSubTypes();

    Task<Guid> Create(DomainOfInfluenceType domainOfInfluenceType, string description, Guid? subTypeId, string bfs);

    Task<Guid> SetInPreparation(string governmentDecisionNumber);

    Task<Initiative> Get(
        Guid id,
        bool includeCommitteeDescription = false,
        bool includeIsSigned = false);

    Task Update(Guid id, Guid? subTypeId, string description, string wording, string reason, CollectionAddress address, string link);

    Task Submit(Guid id);

    Task FlagForReview(Guid id);

    Task Register(Guid id);
}
