// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Core.Services.Documents.TemplateBag;

public record InitiativeCommitteeMemberDataContainer(
    string LastName,
    string PoliticalLastName,
    string FirstName,
    string PoliticalFirstName,
    string DayOfBirth,
    string MonthOfBirth,
    string YearOfBirth,
    string Residence,
    string PoliticalResidence,
    string ApprovalState);
