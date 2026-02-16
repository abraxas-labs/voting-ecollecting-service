// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.Lib.Iam.AuthenticationScheme;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.Mocks;

/// <summary>
/// Default mock implementation.
/// </summary>
/// <inheritdoc />
public class AuthenticationHandlerMock : JwtBearerHandler
{
    private readonly PermissionServiceMock _permissionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationHandlerMock"/> class.
    /// </summary>
    /// <param name="options">The SecureConnect options.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="encoder">The URL encoder.</param>
    /// <param name="permissionService">Permission service.</param>
    public AuthenticationHandlerMock(
        IOptionsMonitor<SecureConnectOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        PermissionServiceMock permissionService)
        : base(options, logger, encoder)
    {
        _permissionService = permissionService;
    }

    /// <summary>
    /// If no authorize header is provided 401 is returned immediately.
    /// Otherwise, the auth succeeds and a name identifier claim is added.
    /// </summary>
    /// <inheritdoc />
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey(CitizenAuthMockDefaults.AuthHeader))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);
        if (!Request.Headers.TryGetValue(CitizenAuthMockDefaults.UserIdHeaderName, out var userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var hasAcr = Request.Headers.TryGetValue(CitizenAuthMockDefaults.UserAcrHeaderName, out var acrValue);
        if (hasAcr)
        {
            identity.AddClaim(new Claim(ClaimTypes.Acr, acrValue.ToString()));
        }

        var hasEmail = Request.Headers.TryGetValue(CitizenAuthMockDefaults.UserEMailHeaderName, out var emailValue);
        var userEmail = hasEmail ? emailValue[0] ?? CitizenAuthMockDefaults.UserTestEMail : CitizenAuthMockDefaults.UserTestEMail;

        var hasEmailVerified = Request.Headers.TryGetValue(CitizenAuthMockDefaults.UserEmailVerifiedHeaderName, out var emailVerifiedValue);
        var emailVerified = !hasEmailVerified || (bool.TryParse(emailVerifiedValue[0], out var ev) && ev);

        var hasSsn = Request.Headers.TryGetValue(CitizenAuthMockDefaults.UserSocialSecurityNumberHeaderName, out var ssnValue);
        var userSsn = hasSsn && !string.IsNullOrWhiteSpace(ssnValue[0]) ? ssnValue[0] : null;

        identity.AddClaim(
            new Claim(
                System.Security.Claims.ClaimTypes.NameIdentifier,
                userId!));

        _permissionService.Init(
            userId!,
            CitizenAuthMockDefaults.UserTestName,
            userEmail,
            emailVerified,
            CitizenAuthMockDefaults.UserTestFirstName,
            CitizenAuthMockDefaults.UserTestLastName);
        _permissionService.SetSsn(userSsn);

        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(
                new ClaimsPrincipal(identity),
                SecureConnectDefaults.AuthenticationScheme)));
    }

    /// <inheritdoc />
    protected override Task InitializeHandlerAsync() => Task.CompletedTask;

    /// <inheritdoc />
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        if (!Request.Headers.ContainsKey(CitizenAuthMockDefaults.AuthHeader))
        {
            Response.StatusCode = 401;
        }

        return Task.CompletedTask;
    }
}
