// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Domain.Models;

public record PendingCollectionPermission(
    Guid CollectionId,
    CollectionType CollectionType,
    string CollectionDescription,
    string InvitedByName,
    string LastName,
    string FirstName,
    CollectionPermissionRole Role,
    IReadOnlySet<string> AcceptAcceptedAcrs);
