// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities.Audit;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Common;

namespace Voting.ECollecting.Shared.Domain.Entities;

public class InitiativeCommitteeMemberEntity : AuditedEntity, IAuditTrailTrackedEntity
{
    public Guid InitiativeId { get; set; }

    public InitiativeEntity? Initiative { get; set; }

    public int SortIndex { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string PoliticalFirstName { get; set; } = string.Empty;

    public string PoliticalLastName { get; set; } = string.Empty;

    public string Bfs { get; set; } = string.Empty;

    public string PoliticalBfs { get; set; } = string.Empty;

    public string? PoliticalDuty { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public string? Email { get; set; }

    public bool MemberSignatureRequested { get; set; } = true;

    public InitiativeCommitteeMemberSignatureType SignatureType { get; set; }

    public InitiativeCommitteeMemberApprovalState ApprovalState { get; set; }

    public CollectionPermissionEntity? Permission { get; set; }

    public UrlToken? Token { get; set; }

    public DateTime? TokenExpiry { get; set; }

    public string? IamUserId { get; set; }

    public Guid? SignatureFileId { get; set; }

    public FileEntity? SignatureFile { get; set; }

    /// <summary>
    /// Gets a value indicating whether this membership is editable.
    /// True if:
    /// the approval state is still requested
    /// or the signature was provided directly by
    /// the editor of the membership (not the member himself)
    /// by uploading the signature list.
    /// </summary>
    public bool CanEdit
        => ApprovalState == InitiativeCommitteeMemberApprovalState.Requested
           || (ApprovalState == InitiativeCommitteeMemberApprovalState.Signed
               && !MemberSignatureRequested
               && SignatureType == InitiativeCommitteeMemberSignatureType.UploadedSignature);

    public bool CanReset => SignatureType == InitiativeCommitteeMemberSignatureType.UploadedSignature &&
                            ApprovalState is InitiativeCommitteeMemberApprovalState.Approved or InitiativeCommitteeMemberApprovalState.Rejected;

    public bool CanVerify => ApprovalState is InitiativeCommitteeMemberApprovalState.Requested
        or InitiativeCommitteeMemberApprovalState.Signed;

    public void SetInitialValues()
    {
        SetInitialState();
        SetInitialSignatureType();
    }

    public void SetToken(DateTime tokenExpiry)
    {
        if (MemberSignatureRequested)
        {
            Token = UrlToken.New();
            TokenExpiry = tokenExpiry;
        }
        else
        {
            Token = null;
            TokenExpiry = null;
        }
    }

    private void SetInitialState()
    {
        // if no signature is requested from the committee member,
        // the initiative admin already provided a signature.
        // Set the state to signed already.
        ApprovalState = MemberSignatureRequested
            ? InitiativeCommitteeMemberApprovalState.Requested
            : InitiativeCommitteeMemberApprovalState.Signed;
    }

    private void SetInitialSignatureType()
    {
        // if the signature is provided by the initiative admin via upload
        // set the according signature type.
        // otherwise let the committee member decide, whether he wants
        // to upload a signature file or sign via AGOV 400.
        SignatureType = MemberSignatureRequested
            ? InitiativeCommitteeMemberSignatureType.Unspecified
            : InitiativeCommitteeMemberSignatureType.UploadedSignature;
    }
}
