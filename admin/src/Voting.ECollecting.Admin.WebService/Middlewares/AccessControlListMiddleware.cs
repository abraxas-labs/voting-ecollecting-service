// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.WebService.Configuration;

namespace Voting.ECollecting.Admin.WebService.Middlewares;

public class AccessControlListMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AppConfig _config;

    public AccessControlListMiddleware(RequestDelegate next, AppConfig config)
    {
        _next = next;
        _config = config;
    }

    public async Task Invoke(
        HttpContext context,
        IPermissionService permissionService,
        IAccessControlListService aclService)
    {
        if (context.Request.Path.Value == null ||
            _config.AccessControlListEvaluationIgnoredPaths.Contains(context.Request.Path.Value))
        {
            await _next(context);
            return;
        }

        var bfsAcls = await aclService.GetBfsNumberAccessControlListsByTenantId(permissionService.TenantId);
        permissionService.SetAccessControlPermissions(bfsAcls);

        await _next(context).ConfigureAwait(false);
    }
}
