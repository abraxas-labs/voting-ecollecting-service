// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Domain.Models;
using Voting.Lib.Database.Models;
using IVotingStimmregisterPersonInfo = Voting.ECollecting.Admin.Domain.Models.IVotingStimmregisterPersonInfo;

namespace Voting.ECollecting.Admin.Abstractions.Adapter.VotingStimmregister;

public interface IVotingStimmregisterAdapter
{
    Task<Page<IVotingStimmregisterPersonInfo>> ListPersonInfos(
        VotingStimmregisterPersonFilterData filterData,
        Pageable? pageable = null,
        CancellationToken cancellationToken = default);

    Task<IVotingStimmregisterPersonInfo> GetPersonInfo(
        VotingStimmregisterPersonFilterData filterData,
        CancellationToken cancellationToken = default);

    Task<IVotingStimmregisterPersonInfo> GetPersonInfo(
        string bfs,
        Guid registerId,
        DateTime actualityDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<IVotingStimmregisterPersonInfo>> GetPersonInfos(
        string bfs,
        IReadOnlySet<Guid> registerIds,
        DateTime actualityDate,
        CancellationToken cancellationToken = default);
}
