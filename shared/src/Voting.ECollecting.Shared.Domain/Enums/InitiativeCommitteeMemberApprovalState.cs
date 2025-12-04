// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Enums;

public enum InitiativeCommitteeMemberApprovalState
{
    Unspecified = 0,

    /// <summary>
    /// Requested approval from the committee member.
    /// </summary>
    Requested = 1,

    /// <summary>
    /// Committee member has either uploaded a committee list
    /// or an initiative owner/deputy has uploaded a committee list
    /// The signed state is skipped if the committee member has approved himself with AGOV 400.
    /// </summary>
    Signed = 2,

    /// <summary>
    /// Committee member has rejected the signature to approve the membership.
    /// </summary>
    SignatureRejected = 3,

    /// <summary>
    /// Committee member has been approved by the government admin
    /// or the committee member has approved himself with AGOV 400 (<see cref="Signed"/> is skipped in this case).
    /// </summary>
    Approved = 4,

    /// <summary>
    /// Committee member has been rejected by the government admin.
    /// </summary>
    Rejected = 5,
}
