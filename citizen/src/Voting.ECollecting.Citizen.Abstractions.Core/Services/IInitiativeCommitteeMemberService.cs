// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.Lib.Common;

namespace Voting.ECollecting.Citizen.Abstractions.Core.Services;

public interface IInitiativeCommitteeMemberService
{
    Task<InitiativeCommittee> GetCommittee(Guid initiativeId);

    Task<InitiativeCommitteeMemberEntity> AddCommitteeMember(InitiativeCommitteeMemberEntity member, CollectionPermissionRole? role);

    Task RemoveCommitteeMember(Guid initiativeId, Guid id);

    Task UpdateCommitteeMemberSort(Guid initiativeId, Guid id, int newIndex);

    Task UpdateCommitteeMember(InitiativeCommitteeMemberEntity member, CollectionPermissionRole? newRole);

    Task ResendCommitteeMemberInvitation(Guid initiativeId, Guid id);

    Task<bool> AcceptCommitteeMemberInvitation(UrlToken token);

    Task RejectCommitteeMemberInvitation(UrlToken token);

    Task<PendingCommitteeMembership> GetPendingCommitteeMembershipByToken(UrlToken token);
}
