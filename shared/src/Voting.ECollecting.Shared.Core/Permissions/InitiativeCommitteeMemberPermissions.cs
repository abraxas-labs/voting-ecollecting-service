// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Diagnostics;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Domain.Models;

namespace Voting.ECollecting.Shared.Core.Permissions;

public static class InitiativeCommitteeMemberPermissions
{
    public static InitiativeCommitteeMemberUserPermissions Build(InitiativeCommitteeMemberEntity member)
    {
        Debug.Assert(member.Initiative != null, "Initiative should be loaded with integrated query.");
        return new InitiativeCommitteeMemberUserPermissions(
            CanEdit(member),
            CanEditPoliticalDetails(member),
            CanResend(member),
            CanReset(member),
            CanVerify(member));
    }

    /// <summary>
    /// Gets a value indicating whether this membership is editable.
    /// True if:
    /// the approval state is still requested
    /// or the signature was provided directly by
    /// the editor of the membership (not the member himself)
    /// by uploading the signature list.
    /// </summary>
    private static bool CanEdit(InitiativeCommitteeMemberEntity member)
        => member.Initiative!.State.InPreparationOrReturnForCorrection()
           && (member.ApprovalState == InitiativeCommitteeMemberApprovalState.Requested
               || member is
               {
                   ApprovalState: InitiativeCommitteeMemberApprovalState.Signed,
                   MemberSignatureRequested: false,
                   SignatureType: InitiativeCommitteeMemberSignatureType.UploadedSignature
               });

    private static bool CanEditPoliticalDetails(InitiativeCommitteeMemberEntity member)
        => member.Initiative!.State.IsNotEndedAndNotAborted()
           && member.ApprovalState
               is InitiativeCommitteeMemberApprovalState.Approved
               or InitiativeCommitteeMemberApprovalState.Requested
               or InitiativeCommitteeMemberApprovalState.Signed;

    private static bool CanResend(InitiativeCommitteeMemberEntity member)
        => member.Initiative!.State.InPreparationOrReturnForCorrection()
           && member is
           {
               MemberSignatureRequested: true,
               ApprovalState: InitiativeCommitteeMemberApprovalState.Requested
               or InitiativeCommitteeMemberApprovalState.SignatureRejected
               or InitiativeCommitteeMemberApprovalState.Expired
           };

    private static bool CanReset(InitiativeCommitteeMemberEntity member)
        => member.ApprovalState is InitiativeCommitteeMemberApprovalState.Approved
            or InitiativeCommitteeMemberApprovalState.Rejected;

    private static bool CanVerify(InitiativeCommitteeMemberEntity member)
        => member.ApprovalState is InitiativeCommitteeMemberApprovalState.Requested
            or InitiativeCommitteeMemberApprovalState.Signed;
}
