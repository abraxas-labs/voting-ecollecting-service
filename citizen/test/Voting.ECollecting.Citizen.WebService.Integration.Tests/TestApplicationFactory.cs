// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Hosting;
using Serilog;
using Voting.Lib.Testing;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests;

public class TestApplicationFactory : TestApplicationFactory<TestStartup>;

public class TestApplicationFactory<TStartup> : BaseTestApplicationFactory<TStartup>
    where TStartup : Startup
{
    public override HttpClient CreateHttpClient(
        bool authorize,
        string? tenant,
        string? userId,
        string[]? roles,
        IEnumerable<(string, string)>? additionalHeaders)
    {
        var httpClient = base.CreateHttpClient(authorize, tenant, userId, roles, additionalHeaders);
        httpClient.DefaultRequestHeaders.Add("x-language", "de");
        return httpClient;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder
            .UseEnvironment("Test")
            .UseSolutionRelativeContentRoot("citizen/src/Voting.ECollecting.Citizen.WebService");
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return base.CreateHostBuilder()
            .UseSerilog((context, _, configuration) => configuration.ReadFrom.Configuration(context.Configuration))
            .ConfigureAppConfiguration((_, config) =>
            {
                // we deploy our config with the docker image, no need to watch for changes
                foreach (var source in config.Sources.OfType<JsonConfigurationSource>())
                {
                    source.ReloadOnChange = false;
                }
            });
    }
}
