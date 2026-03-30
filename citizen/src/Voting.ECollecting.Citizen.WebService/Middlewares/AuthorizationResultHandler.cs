// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Grpc.AspNetCore.Server;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;
using Voting.ECollecting.Citizen.WebService.Exceptions;
using Voting.ECollecting.Citizen.WebService.Interceptors;

namespace Voting.ECollecting.Citizen.WebService.Middlewares;

public class AuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();
    private readonly ExceptionInterceptor _exceptionInterceptor;

    public AuthorizationResultHandler(ExceptionInterceptor exceptionInterceptor)
    {
        _exceptionInterceptor = exceptionInterceptor;
    }

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (!TryGetFailedAcrRequirement(authorizeResult, out var requirement))
        {
            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
            return;
        }

        var ex = new InsufficientAcrException(
            requirement.AllowedValues ?? [],
            context.User.FindFirstValue(ClaimTypes.Acr) ?? "<none>");

        // this runs before the gRPC pipeline: no access to the gRPC context...
        if (context.GetEndpoint()?.Metadata.GetMetadata<GrpcMethodMetadata>() != null)
        {
            var status = _exceptionInterceptor.BuildRpcStatus(ex);
            context.Response.Headers.GrpcStatus = status.StatusCode.ToString("D");
            context.Response.Headers.GrpcMessage = status.Detail;
            context.Response.ContentType = "application/grpc";
            await context.Response.CompleteAsync();
            return;
        }

        throw ex;
    }

    private static bool TryGetFailedAcrRequirement(
        PolicyAuthorizationResult authorizeResult,
        [NotNullWhen(true)] out ClaimsAuthorizationRequirement? failedAcrRequirement)
    {
        if (!authorizeResult.Succeeded
            && authorizeResult.AuthorizationFailure?.FailedRequirements.FirstOrDefault(x => x is ClaimsAuthorizationRequirement { ClaimType: ClaimTypes.Acr })
                is ClaimsAuthorizationRequirement requirement)
        {
            failedAcrRequirement = requirement;
            return true;
        }

        failedAcrRequirement = null;
        return false;
    }
}
