// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Core.Services.Documents.TemplateBag;

public record InitiativeSignatureSheetTemplateBag(
    string Description,
    string Wording,
    string Reason,
    IEnumerable<InitiativeCommitteeMemberDataContainer> CommitteeMembers,
    string CommitteeName,
    string CommitteeStreet,
    string CommitteeZipCode,
    string CommitteeLocality,
    string Website,
    string? Image,
    string? Logo,
    string CollectionStartDate,
    string CollectionEndDate,
    string SignatureListSubmissionEndDate);
