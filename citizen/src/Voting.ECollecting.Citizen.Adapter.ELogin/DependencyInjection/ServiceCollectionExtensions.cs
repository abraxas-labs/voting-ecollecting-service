// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin;

namespace Voting.ECollecting.Citizen.Adapter.ELogin.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIamServices(
        this IServiceCollection services,
        ELoginConfig config)
    {
        services
            .AddSingleton(config.SocialSecurityNumberCache)
            .AddSingleton<SocialSecurityNumberCache>()
            .AddForwardRefScoped<IPermissionService, PermissionService>()
            .AddHttpContextAccessor()
            .AddHttpClient<PersonServiceClient>((sp, client) =>
            {
                client.BaseAddress = config.ApiBaseUrl ?? throw new InvalidOperationException($"ELogin {nameof(config.ApiBaseUrl)} not configured");

                var contextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                var authHeader = contextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
                if (!string.IsNullOrEmpty(authHeader))
                {
                    client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader);
                }
            });

        return services;
    }
}
