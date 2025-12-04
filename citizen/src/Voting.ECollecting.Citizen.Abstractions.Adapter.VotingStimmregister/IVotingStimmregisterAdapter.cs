// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Abstractions.Adapter.VotingStimmregister;

public interface IVotingStimmregisterAdapter
{
    Task<bool> HasVotingRight(string socialSecurityNumber, DomainOfInfluenceType doiType, string bfs);

    Task<IVotingStimmregisterPersonInfo> GetPersonInfo(string socialSecurityNumber, DomainOfInfluenceType doiType, string bfs);
}
