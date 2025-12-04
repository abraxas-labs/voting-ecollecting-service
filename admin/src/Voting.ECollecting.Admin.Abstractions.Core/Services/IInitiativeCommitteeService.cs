// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Models;
using IVotingStimmregisterPersonInfo = Voting.ECollecting.Admin.Domain.Models.IVotingStimmregisterPersonInfo;

namespace Voting.ECollecting.Admin.Abstractions.Core.Services;

public interface IInitiativeCommitteeService
{
    Task<InitiativeCommittee> GetCommittee(Guid initiativeId);

    Task<FileEntity> GetCommitteeList(Guid initiativeId, Guid fileId);

    Task ResetCommitteeMember(Guid initiativeId, Guid id);

    Task<IVotingStimmregisterPersonInfo> VerifyCommitteeMember(Guid initiativeId, Guid id);

    Task ApproveCommitteeMember(Guid initiativeId, Guid id);

    Task RejectCommitteeMember(Guid initiativeId, Guid id);
}
