// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.Extensions.DependencyInjection;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Admin;
using Voting.ECollecting.Citizen.Adapter.Admin.Config;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.Lib.Grpc.Extensions;
using Voting.Lib.Iam.TokenHandling;

namespace Voting.ECollecting.Citizen.Adapter.Admin.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAdminServices(
        this IServiceCollection services,
        VotingECollectingAdminConfig config)
    {
        services.AddSingleton(config);

#if !RELEASE
        if (config.EnableMock)
        {
            return services.AddSingleton<IAdminAdapter, AdminAdapterMock>();
        }
#endif

        services.AddScoped<IAdminAdapter, AdminAdapter>();
        services.AddGrpcClient<CollectionService.CollectionServiceClient>(opts => opts.Address = config.ApiEndpoint)
            .ConfigureGrpcPrimaryHttpMessageHandler(config.Mode)
            .AddSecureConnectServiceToken(config.IdpServiceAccount)
            .WithTenant(config.Tenant);
        return services;
    }
}
