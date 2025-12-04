// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Citizen.Domain.Authorization;

public static class Policies
{
    public const string AcceptPermission = nameof(AcceptPermission);
    public const string AcceptInitiativeCommitteeMembership = nameof(AcceptInitiativeCommitteeMembership);
    public const string SignCollection = nameof(SignCollection);
    public const string CreateCollection = nameof(CreateCollection);
}
