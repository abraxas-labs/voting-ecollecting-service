// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration.Json;
using Serilog;
#if !DEBUG
using Serilog.Formatting.Json;
#endif
using Voting.ECollecting.Admin.Domain.Diagnostics;
using Voting.ECollecting.Admin.WebService.Configuration;
using Voting.Lib.Common;
using Voting.Lib.Database.Migrations;

namespace Voting.ECollecting.Admin.WebService;

public static class Program
{
    public static async Task Main(string[] args)
    {
        EnvironmentVariablesFixer.FixDotEnvironmentVariables();

        // A bootstrap logger which will be used until the app is completely initialized, since we cannot read from the config yet
        // Will be replaced later with the "real" logger
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
#if DEBUG
            .WriteTo.Console()
#else
            .WriteTo.Console(new JsonFormatter())
#endif
            .CreateBootstrapLogger();

        DiagnosticsConfig.Initialize();

        var host = CreateHostBuilder(args).Build();
        await RunMigrations(host);
        await host.RunAsync();
    }

    private static async Task RunMigrations(IHost host)
    {
        var isDev = host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment();
        var runOnlyDbMigrations = Environment.GetCommandLineArgs() is [_, "migrate-db"];
        if (!isDev && !runOnlyDbMigrations)
        {
            return;
        }

        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Migrations");

        logger.LogInformation("Running migrations...");
        await host.Services.GetRequiredService<IDatabaseMigrator>().Migrate();

        // if we should only run the migrations, terminate the process successfully.
        if (runOnlyDbMigrations)
        {
            logger.LogInformation("Migrations done, exiting.");
            Environment.Exit(0);
        }

        logger.LogInformation("Migrations done.");
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog((context, _, configuration) => configuration.ReadFrom.Configuration(context.Configuration))
            .ConfigureAppConfiguration((_, config) =>
            {
                // we deploy our config with the docker image, no need to watch for changes
                foreach (var source in config.Sources.OfType<JsonConfigurationSource>())
                {
                    source.ReloadOnChange = false;
                }
            })
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>().ConfigureKestrel(
                server =>
                {
                    var config = server.ApplicationServices.GetRequiredService<AppConfig>();
                    server.ListenAnyIP(config.Ports.Http, o => o.Protocols = HttpProtocols.Http1);
                    server.ListenAnyIP(config.Ports.Http2, o => o.Protocols = HttpProtocols.Http2);
                    server.ListenAnyIP(config.MetricPort, o => o.Protocols = HttpProtocols.Http1);
                }));
}
