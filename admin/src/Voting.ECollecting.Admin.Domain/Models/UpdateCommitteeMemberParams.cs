// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Models;

public record UpdateCommitteeMemberParams(
    string PoliticalFirstName,
    string PoliticalLastName,
    string PoliticalResidence,
    string? PoliticalDuty);
