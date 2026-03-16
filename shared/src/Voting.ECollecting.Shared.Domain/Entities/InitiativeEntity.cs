// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Database.Models;

namespace Voting.ECollecting.Shared.Domain.Entities;

public class InitiativeEntity : CollectionBaseEntity
{
    public int MinSignatureCount { get; set; }

    public MarkdownString Wording { get; set; } = string.Empty;

    public Guid? SubTypeId { get; set; }

    public InitiativeSubTypeEntity? SubType { get; set; }

    public string GovernmentDecisionNumber { get; set; } = string.Empty;

    public ICollection<FileEntity> CommitteeLists { get; set; } = new List<FileEntity>();

    public ICollection<InitiativeCommitteeMemberEntity> CommitteeMembers { get; set; } = new List<InitiativeCommitteeMemberEntity>();

    public CollectionCameNotAboutReason? CameNotAboutReason { get; set; }

    public AdmissibilityDecisionState? AdmissibilityDecisionState { get; set; }

    public InitiativeLockedFields LockedFields { get; set; } = new();

    public DateOnly? SensitiveDataExpiryDate { get; set; }
}
