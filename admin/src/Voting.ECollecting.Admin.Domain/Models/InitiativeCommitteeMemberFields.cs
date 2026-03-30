// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Models;

[Flags]
public enum InitiativeCommitteeMemberFields
{
    None = 0,
    PoliticalFirstName = 1 << 0,
    PoliticalLastName = 1 << 1,
    PoliticalResidence = 1 << 2,
    PoliticalDuty = 1 << 3,
}
