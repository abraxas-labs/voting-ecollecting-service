// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.Database.Models;

namespace Voting.ECollecting.Citizen.Domain.Models;

public record PendingCommitteeMembership(
    Guid InitiativeId,
    string Description,
    InitiativeSubTypeEntity? SubType,
    MarkdownString Wording,
    string Reason,
    string Link,
    string FirstName,
    string LastName,
    string InvitedByName,
    IReadOnlySet<string> AcceptAcceptedAcrs);
