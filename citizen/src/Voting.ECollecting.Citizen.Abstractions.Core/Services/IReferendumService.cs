// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Abstractions.Core.Services;

public interface IReferendumService
{
    Task<Guid> Create(string description, Guid? decreeId);

    Task<Guid> SetInPreparation(string referendumNumber);

    Task<Referendum> Get(Guid id, bool includeIsSigned = false);

    Task Update(Guid id, string description, string reason, CollectionAddress address, string membersCommittee, string link);

    Task Submit(Guid id);

    Task<(List<Decree> Decrees, List<Referendum> ReferendumsWithoutDecree)> ListMy();

    Task<Dictionary<DomainOfInfluenceType, List<Decree>>> ListDecreesEligibleForReferendumByDoiType(IReadOnlySet<DomainOfInfluenceType>? doiTypes, string? bfs, bool includeReferendums);

    Task UpdateDecree(Guid id, Guid decreeId);
}
