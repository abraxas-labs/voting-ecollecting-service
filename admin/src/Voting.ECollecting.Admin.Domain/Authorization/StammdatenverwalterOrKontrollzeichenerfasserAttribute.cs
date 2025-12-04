// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Authorization;
using AuthorizationRoles = Voting.ECollecting.Admin.Domain.Authorization.Roles;

namespace Voting.ECollecting.Admin.Domain.Authorization;

public class StammdatenverwalterOrKontrollzeichenerfasserAttribute : AuthorizeAttribute
{
    public StammdatenverwalterOrKontrollzeichenerfasserAttribute()
    {
        Roles = $"{AuthorizationRoles.Stammdatenverwalter},{AuthorizationRoles.Kontrollzeichenerfasser}";
    }
}
