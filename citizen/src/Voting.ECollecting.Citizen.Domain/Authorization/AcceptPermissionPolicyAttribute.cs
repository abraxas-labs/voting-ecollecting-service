// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Authorization;

namespace Voting.ECollecting.Citizen.Domain.Authorization;

public class AcceptPermissionPolicyAttribute : AuthorizeAttribute
{
    public AcceptPermissionPolicyAttribute()
    {
        Policy = Policies.AcceptPermission;
    }
}
