// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Admin.Adapter.Data;
using Voting.ECollecting.Admin.Domain.Constants;
using Voting.ECollecting.Shared.Domain.Entities.Audit;

namespace Voting.ECollecting.Admin.WebService.Middlewares;

public class AuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden)
        {
            var endpoint = context.GetEndpoint()?.DisplayName ?? context.Request.Path.Value ?? "Unknown";

            var dataContext = context.RequestServices.GetRequiredService<DataContext>();
            var permissionService = context.RequestServices.GetRequiredService<IPermissionService>();
            var auditTrailEntry = new AuditTrailEntryEntity
            {
                Action = AuditTrailAction.DeniedAccess,
                Information = endpoint,
            };

            permissionService.SetCreated(auditTrailEntry);
            dataContext.AuditTrailEntries.Add(auditTrailEntry);
            await dataContext.SaveChangesAsync();
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
