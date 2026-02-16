// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

#if DEBUG
using Voting.ECollecting.Shared.Migrations.DependencyInjection;
#endif
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Voting.ECollecting.Admin.WebService.Middlewares;
using Voting.ECollecting.Citizen.Adapter.Admin.DependencyInjection;
using Voting.ECollecting.Citizen.Adapter.Data;
using Voting.ECollecting.Citizen.Adapter.Data.DependencyInjection;
using Voting.ECollecting.Citizen.Adapter.Data.Repositories;
using Voting.ECollecting.Citizen.Adapter.ELogin;
using Voting.ECollecting.Citizen.Adapter.ELogin.DependencyInjection;
using Voting.ECollecting.Citizen.Adapter.VotingStimmregister.DependencyInjection;
using Voting.ECollecting.Citizen.Api.Grpc.Services;
using Voting.ECollecting.Citizen.Core;
using Voting.ECollecting.Citizen.WebService.Configuration;
using Voting.ECollecting.Citizen.WebService.DependencyInjection;
using Voting.ECollecting.Citizen.WebService.Extensions;
using Voting.ECollecting.Shared.Core;
using Voting.Lib.Common.DependencyInjection;
using Voting.Lib.Cryptography.Extensions;
using Voting.Lib.Database.Postgres.Locking;
using Voting.Lib.Grpc.DependencyInjection;
using Voting.Lib.Grpc.Interceptors;
using Voting.Lib.MalwareScanner.DependencyInjection;
using Voting.Lib.Rest.Swagger.DependencyInjection;
using ExceptionHandler = Voting.ECollecting.Citizen.WebService.Middlewares.ExceptionHandler;
using ExceptionInterceptor = Voting.ECollecting.Citizen.WebService.Interceptors.ExceptionInterceptor;

namespace Voting.ECollecting.Citizen.WebService;

public class Startup(IConfiguration configuration, IWebHostEnvironment environment)
{
    protected AppConfig AppConfig { get; } = configuration.Get<AppConfig>()!;

    public virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddWebServiceServices(AppConfig);
        services.AddCoreServices(AppConfig);
        services.AddSharedCoreServices(AppConfig.Urls, AppConfig.UserNotification, AppConfig.DmDoc)
            .AddPermissionService<PermissionService>()
            .AddUserNotificationRepo<UserNotificationRepository>()
            .AddAccessControlListDoiRepository<AccessControlListDoiRepository>()
            .AddReferendumRepository<ReferendumRepository>()
            .AddInitiativeRepository<InitiativeRepository>()
            .AddCollectionCitizenLogRepository<CollectionCitizenLogRepository>()
            .AddDmDocOrMock(AppConfig.DmDoc);
        services.AddIamServices(AppConfig.ELogin);
        services.AddVotingStimmregisterServices(AppConfig.VotingStimmregister);
        services.AddAdapterDataServices(AppConfig.Database, ConfigureDatabase);
        services.AddAdminServices(AppConfig.VotingECollectingAdmin);
#if DEBUG
        // Intended to be executed on local environment only. Service doesn't have permission to run migrations on remote environments.
        services.AddDatabaseMigrationServices(ConfigureMigrationDatabase);
#endif

        services.AddCertificatePinning(AppConfig.CertificatePinning);
        services.AddVotingLibPrometheusAdapter(new() { Interval = AppConfig.PrometheusAdapterInterval });

        services.AddMalwareScanner(AppConfig.MalwareScanner);

        ConfigureHealthChecks(services.AddHealthChecks());
        ConfigureAuthentication(services.AddAuthentication());
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationResultHandler>();
        services.AddAuthorizationBuilder()
            .AddAcrPolicies(AppConfig.Acr)
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        services.AddSingleton<ExceptionInterceptor>();
        services
            .AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services
            .AddGrpc(o =>
            {
                o.EnableDetailedErrors = AppConfig.EnableDetailedErrors;
                o.Interceptors.Add<ExceptionInterceptor>();
                o.Interceptors.Add<RequestProtoValidatorInterceptor>();
            });

        services.AddGrpcRequestLoggerInterceptor(environment);

        if (AppConfig.EnableGrpcWeb)
        {
            services.AddCors(configuration);
        }

        services.AddGrpcReflection();
        services.AddProtoValidators();

        services.AddSwaggerGenerator(configuration);
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseMetricServer(AppConfig.MetricPort);
        app.UseHttpMetrics();

        app.UseRouting();

        if (AppConfig.EnableGrpcWeb)
        {
            app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
            app.UseCorsFromConfig();
        }

        app.UseMiddleware<ExceptionHandler>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseSerilogRequestLoggingWithTraceabilityModifiers();

        app.UseSwaggerGenerator();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapVotingHealthChecks(AppConfig.LowPriorityHealthCheckNames);
            MapEndpoints(endpoints);
        });
    }

    protected virtual void ConfigureDatabase(DbContextOptionsBuilder db)
        => db.UseNpgsql(AppConfig.Database.ConnectionString, o => o.SetPostgresVersion(AppConfig.Database.Version)).AddLockInterceptors();

#if DEBUG
    protected virtual void ConfigureMigrationDatabase(DbContextOptionsBuilder db)
        => db.UseNpgsql(AppConfig.MigrationDatabaseOrDatabase.ConnectionString, o => o.SetPostgresVersion(AppConfig.MigrationDatabaseOrDatabase.Version));
#endif

    protected virtual void ConfigureAuthentication(AuthenticationBuilder builder)
    {
        builder.AddJwtBearer();
        builder.Services.AddTransient<JwtBearerHandler, ELoginHandler>();
    }

    private void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
        endpoints.MapGrpcReflectionService();
        endpoints.MapGrpcService<CollectionGrpcService>();
        endpoints.MapGrpcService<DomainOfInfluenceGrpcService>();
        endpoints.MapGrpcService<InitiativeGrpcService>();
        endpoints.MapGrpcService<ReferendumGrpcService>();
        endpoints.MapGrpcService<AccessibilityGrpcService>();
        endpoints.MapGrpcService<AuthGrpcService>();
    }

    private void ConfigureHealthChecks(IHealthChecksBuilder checks)
    {
        checks
            .AddDbContextCheck<DataContext>()
            .AddCryptoProviderHealthCheck()
            .ForwardToPrometheus();
    }
}
