// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Basis.Services.V1;
using Grpc.Net.Client.Web;
using Microsoft.Extensions.DependencyInjection;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingBasis;
using Voting.ECollecting.Admin.Adapter.VotingBasis.Configuration;

namespace Voting.ECollecting.Admin.Adapter.VotingBasis.DependencyInjection;

/// <summary>
/// Service collection extensions to register Adapter.VotingBasis services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the adapter VOTING Basis services to DI container.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <param name="config">The voting basis configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddAdapterVotingBasisServices(this IServiceCollection services, VotingBasisConfig config)
    {
        services
            .AddGrpcClient<AdminManagementService.AdminManagementServiceClient>(opts => opts.Address = config.ApiEndpoint)
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return config.EnableGrpcWeb ?
                    new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler()) :
                    new HttpClientHandler();
            })
            .AddHttpMessageHandler(_ => new AdminManagementServiceClientHandler(config))
            .AddSecureConnectServiceToken(config.IdpServiceAccount);

        services.AddSingleton(config);
        services.AddScoped<IVotingBasisAdapter, VotingBasisAdapter>();

        return services;
    }
}
