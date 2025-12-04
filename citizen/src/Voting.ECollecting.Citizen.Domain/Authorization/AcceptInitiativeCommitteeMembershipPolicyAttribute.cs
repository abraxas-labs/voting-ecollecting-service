// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Authorization;

namespace Voting.ECollecting.Citizen.Domain.Authorization;

public class AcceptInitiativeCommitteeMembershipPolicyAttribute : AuthorizeAttribute
{
    public AcceptInitiativeCommitteeMembershipPolicyAttribute()
    {
        Policy = Policies.AcceptInitiativeCommitteeMembership;
    }
}
