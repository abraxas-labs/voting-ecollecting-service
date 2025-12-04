// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin;

namespace Voting.ECollecting.Citizen.Adapter.ELogin;

public class ELoginHandler : JwtBearerHandler
{
    private readonly IPermissionService _permissionService;

    public ELoginHandler(IOptionsMonitor<JwtBearerOptions> options, ILoggerFactory logger, UrlEncoder encoder, IPermissionService permissionService)
        : base(options, logger, encoder)
    {
        _permissionService = permissionService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var result = await base.HandleAuthenticateAsync();
        if (!result.Succeeded || result.Principal?.Identity is not ClaimsIdentity identity)
        {
            return result;
        }

        var sub = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        if (sub == null)
        {
            return AuthenticateResult.Fail("No sub present in token");
        }

        var userName = identity.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Name)?.Value ?? string.Empty;
        var userFirstName = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value ?? string.Empty;
        var userLastName = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value ?? string.Empty;
        var userEmail = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value ?? string.Empty;
        _permissionService.Init(
            sub,
            userName,
            userEmail,
            userFirstName,
            userLastName);
        return result;
    }
}
