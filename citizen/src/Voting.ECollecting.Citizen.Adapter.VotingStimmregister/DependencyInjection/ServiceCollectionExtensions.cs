// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.Extensions.DependencyInjection;
using Voting.ECollecting.Citizen.Abstractions.Adapter.VotingStimmregister;
using Voting.ECollecting.Citizen.Adapter.VotingStimmregister.Config;
using Voting.Lib.Grpc.Extensions;
using Voting.Lib.Iam.TokenHandling;
using Voting.Stimmregister.Proto.V1.Services;

namespace Voting.ECollecting.Citizen.Adapter.VotingStimmregister.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVotingStimmregisterServices(
        this IServiceCollection services,
        VotingStimmregisterConfig config)
    {
        services.AddSingleton(config);

#if !RELEASE
        if (config.EnableMock)
        {
            return services.AddSingleton<IVotingStimmregisterAdapter, VotingStimmregisterAdapterMock>();
        }
#endif

        services.AddScoped<IVotingStimmregisterAdapter, VotingStimmregisterAdapter>();
        services.AddGrpcClient<EcollectingService.EcollectingServiceClient>(opts => opts.Address = config.ApiEndpoint)
            .ConfigureGrpcPrimaryHttpMessageHandler(config.Mode)
            .AddSecureConnectServiceToken(config.IdpServiceAccount)
            .WithTenant(config.Tenant);
        return services;
    }
}
