// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Authorization;
using AuthorizationRoles = Voting.ECollecting.Admin.Domain.Authorization.Roles;

namespace Voting.ECollecting.Admin.Domain.Authorization;

public class KontrollzeichenerfasserOrStichprobenverwalterAttribute : AuthorizeAttribute
{
    public KontrollzeichenerfasserOrStichprobenverwalterAttribute()
    {
        Roles = $"{AuthorizationRoles.Kontrollzeichenerfasser},{AuthorizationRoles.Stichprobenverwalter}";
    }
}
