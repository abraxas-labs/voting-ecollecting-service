// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Citizen.Domain.Models;

public record PendingCommitteeMembership(
    Guid InitiativeId,
    string Description,
    InitiativeSubTypeEntity? SubType,
    string Wording,
    string Reason,
    string Link,
    string FirstName,
    string LastName,
    string InvitedByName,
    IReadOnlySet<string> AcceptAcceptedAcrs);
