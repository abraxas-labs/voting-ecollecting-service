// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Adapter.VotingBasis.Configuration;

namespace Voting.ECollecting.Admin.Adapter.VotingBasis.DependencyInjection;

/// <inheritdoc />
public class AdminManagementServiceClientHandler : DelegatingHandler
{
    private readonly VotingBasisConfig _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminManagementServiceClientHandler"/> class.
    /// </summary>
    /// <param name="config">The configuration.</param>
    public AdminManagementServiceClientHandler(VotingBasisConfig config)
    {
        _config = config;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("authorize", "true");
        request.Headers.Add("x-tenant", _config.ApiAdminTenantId);
        request.Headers.Add("x-app", _config.ApiAdminAppId);

        if (_config.EnableGrpcWeb)
        {
            request.Headers.Add("X-Grpc-Web", "1");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
