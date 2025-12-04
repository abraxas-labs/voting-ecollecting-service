// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities.IntegritySignature;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Common;

namespace Voting.ECollecting.Shared.Domain.Entities;

public class CollectionPermissionEntity : IntegritySignatureEntity
{
    public string LastName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UrlToken? Token { get; set; }

    public DateTime? TokenExpiry { get; set; }

    public bool Accepted => State == CollectionPermissionState.Accepted;

    public CollectionPermissionState State { get; set; } = CollectionPermissionState.Pending;

    public string IamLastName { get; set; } = string.Empty;

    public string IamFirstName { get; set; } = string.Empty;

    public string IamUserId { get; set; } = string.Empty;

    public CollectionPermissionRole Role { get; set; }

    public Guid CollectionId { get; set; }

    public CollectionBaseEntity Collection { get; set; } = null!;

    public Guid? InitiativeCommitteeMemberId { get; set; }

    public InitiativeCommitteeMemberEntity? InitiativeCommitteeMember { get; set; }

    public string FullName
    {
        get
        {
            var firstName = !string.IsNullOrEmpty(IamFirstName) ? IamFirstName : FirstName;
            var lastName = !string.IsNullOrEmpty(IamLastName) ? IamLastName : LastName;
            return $"{firstName} {lastName}";
        }
    }
}
