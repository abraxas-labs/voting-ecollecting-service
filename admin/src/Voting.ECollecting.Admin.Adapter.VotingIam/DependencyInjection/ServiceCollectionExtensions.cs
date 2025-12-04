// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.Extensions.DependencyInjection;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Admin.Adapter.VotingIam.Configuration;

namespace Voting.ECollecting.Admin.Adapter.VotingIam.DependencyInjection;

/// <summary>
/// Service collection extensions to register Core services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the core services to DI container.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <param name="idpConfig">The identity provider configuration which will be added as Singleton.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddIamServices(
        this IServiceCollection services,
        VotingIamConfig idpConfig)
    {
        services
            .AddSingleton(idpConfig)
            .AddScoped<IPermissionService, PermissionService>();

        return services;
    }
}
