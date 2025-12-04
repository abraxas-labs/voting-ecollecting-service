// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Models;

public record VotingStimmregisterPersonFilterData(
    string Bfs,
    string? OfficialName = null,
    string? FirstName = null,
    DateOnly? DateOfBirth = null,
    string? ResidenceAddressStreet = null,
    string? ResidenceAddressHouseNumber = null,
    DateTime? ActualityDate = null);
