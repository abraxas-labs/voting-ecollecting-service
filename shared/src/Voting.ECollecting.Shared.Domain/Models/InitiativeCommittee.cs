// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Shared.Domain.Models;

public record InitiativeCommittee(
    string Bfs,
    IReadOnlyCollection<FileEntity> CommitteeLists,
    IReadOnlyCollection<InitiativeCommitteeMember> CommitteeMembers)
{
    private int _requiredApprovedMembersCount;
    private int? _approvedMembersCount;

    public bool ApprovedMembersCountOk => ApprovedMembersCount >= RequiredApprovedMembersCount;

    public int ApprovedMembersCount
        => _approvedMembersCount ??=
            CommitteeMembers.Count(x => x.ApprovalState is InitiativeCommitteeMemberApprovalState.Approved
                or InitiativeCommitteeMemberApprovalState.Signed);

    public int TotalMembersCount => CommitteeMembers.Count;

    public int RequiredApprovedMembersCount
    {
        get => _requiredApprovedMembersCount;
        set
        {
            _approvedMembersCount = null;
            _requiredApprovedMembersCount = value;
        }
    }

    public IEnumerable<InitiativeCommitteeMember> ActiveCommitteeMembers => CommitteeMembers.Where(x =>
        x.ApprovalState is InitiativeCommitteeMemberApprovalState.Requested
            or InitiativeCommitteeMemberApprovalState.Signed or InitiativeCommitteeMemberApprovalState.Approved);

    public IEnumerable<InitiativeCommitteeMember> RejectedOrExpiredCommitteeMembers => CommitteeMembers
        .Where(x => x.ApprovalState is InitiativeCommitteeMemberApprovalState.SignatureRejected
            or InitiativeCommitteeMemberApprovalState.Rejected or InitiativeCommitteeMemberApprovalState.Expired);
}
