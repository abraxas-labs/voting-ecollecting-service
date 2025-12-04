// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Citizen.Domain.Models;

public record CollectionsGroup(
    IReadOnlyList<Initiative> Initiatives,
    IReadOnlyList<Decree> Referendums);
