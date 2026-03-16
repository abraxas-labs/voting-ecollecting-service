// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Voting.ECollecting.Admin.Adapter.Data;
using Voting.ECollecting.Admin.Adapter.Data.DependencyInjection;
using Voting.ECollecting.Admin.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Adapter.VotingBasis.DependencyInjection;
using Voting.ECollecting.Admin.Adapter.VotingIam;
using Voting.ECollecting.Admin.Adapter.VotingIam.DependencyInjection;
using Voting.ECollecting.Admin.Adapter.VotingStimmregister.DependencyInjection;
using Voting.ECollecting.Admin.Api.Grpc.Services;
using Voting.ECollecting.Admin.Core;
using Voting.ECollecting.Admin.WebService.Configuration;
using Voting.ECollecting.Admin.WebService.DependencyInjection;
using Voting.ECollecting.Admin.WebService.Middlewares;
using Voting.ECollecting.Shared.Core;
using Voting.ECollecting.Shared.Migrations.DependencyInjection;
using Voting.Lib.Common.DependencyInjection;
using Voting.Lib.Cryptography.Extensions;
using Voting.Lib.Database.Postgres.Locking;
using Voting.Lib.Grpc.Interceptors;
using Voting.Lib.MalwareScanner.DependencyInjection;
using Voting.Lib.Rest.Middleware;
using Voting.Lib.Rest.Swagger.DependencyInjection;
using ExceptionHandler = Voting.ECollecting.Admin.WebService.Middlewares.ExceptionHandler;
using ExceptionInterceptor = Voting.ECollecting.Admin.WebService.Interceptors.ExceptionInterceptor;

namespace Voting.ECollecting.Admin.WebService;

public class Startup(IConfiguration configuration)
{
    protected AppConfig AppConfig { get; } = configuration.Get<AppConfig>()!;

    public virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddWebServiceServices(AppConfig);
        services.AddIamServices(AppConfig.SecureConnect);
        services.AddCoreServices(AppConfig);
        services.AddSharedCoreServices(AppConfig.Urls, AppConfig.DmDoc)
            .AddPermissionService<PermissionService>()
            .AddUserNotificationRepo<UserNotificationRepository>()
            .AddDomainOfInfluenceRepository<DomainOfInfluenceRepository>()
            .AddReferendumRepository<ReferendumRepository>()
            .AddInitiativeRepository<InitiativeRepository>()
            .AddCollectionCitizenLogRepository<CollectionCitizenLogRepository>()
            .AddDmDocOrMock(AppConfig.DmDoc);
        services.AddAdapterDataServices(AppConfig.Database, ConfigureDatabase);
        services.AddDatabaseMigrationServices(ConfigureMigrationDatabase);
        services.AddAdapterVotingBasisServices(AppConfig.VotingBasis);
        services.AddVotingStimmregisterServices(AppConfig.VotingStimmregister);

        services.AddCertificatePinning(AppConfig.CertificatePinning);
        services.AddMalwareScanner(AppConfig.MalwareScanner);
        services.AddVotingLibPrometheusAdapter(new() { Interval = AppConfig.PrometheusAdapterInterval });

        ConfigureHealthChecks(services.AddHealthChecks());
        ConfigureAuthentication(services.AddVotingLibIam(new() { BaseUrl = AppConfig.SecureConnectApi }, AppConfig.AuthStore));
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        services
            .AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        if (AppConfig.EnableGrpcWeb)
        {
            services.AddCors(configuration);
        }

        services.AddSwaggerGenerator(configuration);

        services.AddGrpc(o =>
        {
            o.EnableDetailedErrors = AppConfig.EnableDetailedErrors;
            o.Interceptors.Add<ExceptionInterceptor>();
            o.Interceptors.Add<RequestProtoValidatorInterceptor>();
        });

        services.AddGrpcReflection();
        services.AddProtoValidators();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseMetricServer(AppConfig.MetricPort);
        app.UseRouting();
        app.UseHttpMetrics();
        app.UseGrpcMetrics();

        if (AppConfig.EnableGrpcWeb)
        {
            app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
            app.UseCorsFromConfig();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<ExceptionHandler>();
        app.UseMiddleware<AccessControlListMiddleware>();
        app.UseMiddleware<IamLoggingHandler>();
        app.UseSerilogRequestLoggingWithTraceabilityModifiers();

        app.UseSwaggerGenerator();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapVotingHealthChecks(AppConfig.LowPriorityHealthCheckNames);
            MapEndpoints(endpoints);
        });
    }

    protected virtual void ConfigureAuthentication(AuthenticationBuilder builder)
        => builder.AddSecureConnectScheme(options =>
        {
            options.Audience = AppConfig.SecureConnect.Audience;
            options.Authority = AppConfig.SecureConnect.Authority;
            options.FetchRoleToken = true;
            options.LimitRolesToAppHeaderApps = false;
            options.ServiceAccount = AppConfig.SecureConnect.ServiceAccount;
            options.ServiceAccountPassword = AppConfig.SecureConnect.ServiceAccountPassword;
            options.ServiceAccountScopes = AppConfig.SecureConnect.ServiceAccountScopes;
            options.AnyRoleRequired = AppConfig.SecureConnect.AnyRoleRequired;
        });

    protected virtual void ConfigureDatabase(DbContextOptionsBuilder db)
        => db.UseNpgsql(AppConfig.Database.ConnectionString, o => o.SetPostgresVersion(AppConfig.Database.Version)).AddLockInterceptors();

    protected virtual void ConfigureMigrationDatabase(DbContextOptionsBuilder db)
        => db.UseNpgsql(AppConfig.MigrationDatabaseOrDatabase.ConnectionString, o => o.SetPostgresVersion(AppConfig.MigrationDatabaseOrDatabase.Version));

    private void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
        endpoints.MapGrpcReflectionService();
        endpoints.MapGrpcService<CertificateGrpcService>();
        endpoints.MapGrpcService<CollectionGrpcService>();
        endpoints.MapGrpcService<CollectionMunicipalityGrpcService>();
        endpoints.MapGrpcService<CollectionSignatureSheetGrpcService>();
        endpoints.MapGrpcService<DecreeGrpcService>();
        endpoints.MapGrpcService<DomainOfInfluenceGrpcService>();
        endpoints.MapGrpcService<InitiativeGrpcService>();
        endpoints.MapGrpcService<ReferendumGrpcService>();
    }

    private void ConfigureHealthChecks(IHealthChecksBuilder checks)
    {
        checks
            .AddDbContextCheck<DataContext>()
            .AddCryptoProviderHealthCheck()
            .ForwardToPrometheus();
    }
}
