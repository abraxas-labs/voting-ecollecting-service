// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Abstractions.Adapter.VotingBasis;

public interface IVotingBasisAdapter
{
    Task<IEnumerable<DomainOfInfluenceEntity>> GetAccessControlList(Guid? importStatisticId);
}
