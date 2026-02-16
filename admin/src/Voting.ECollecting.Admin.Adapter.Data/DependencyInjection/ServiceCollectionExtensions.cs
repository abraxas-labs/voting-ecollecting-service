// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Adapter.Data.Builders;
using Voting.ECollecting.Admin.Adapter.Data.Configuration;
using Voting.ECollecting.Admin.Adapter.Data.Repositories;
using Voting.Lib.Database.Interceptors;

namespace Voting.ECollecting.Admin.Adapter.Data.DependencyInjection;

/// <summary>
/// Service collection extensions to register Adapter.Data services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the data services to DI container.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <param name="dataConfig">The data configuration which will be added as Singleton.</param>
    /// <param name="optionsBuilder">The db context options builder to configure additional db options.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddAdapterDataServices(
        this IServiceCollection services,
        DataConfig dataConfig,
        Action<DbContextOptionsBuilder> optionsBuilder)
    {
        services.AddDbContext<IDataContext, DataContext>((serviceProvider, db) =>
        {
            if (dataConfig.EnableDetailedErrors)
            {
                db.EnableDetailedErrors();
            }

            if (dataConfig.EnableSensitiveDataLogging)
            {
                db.EnableSensitiveDataLogging();
            }

            if (dataConfig.EnableMonitoring)
            {
                db.AddInterceptors(serviceProvider.GetRequiredService<DatabaseQueryMonitoringInterceptor>());
            }

            db.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            optionsBuilder(db);
        });

        if (dataConfig.EnableMonitoring)
        {
            services.AddDataMonitoring(dataConfig.Monitoring);
        }

        return services
            .AddSingleton(dataConfig)
            .AddScoped<IDomainOfInfluenceRepository, DomainOfInfluenceRepository>()
            .AddScoped<IDecreeRepository, DecreeRepository>()
            .AddScoped<IInitiativeRepository, InitiativeRepository>()
            .AddScoped<IInitiativeSubTypeRepository, InitiativeSubTypeRepository>()
            .AddScoped<IAccessControlListDoiRepository, AccessControlListDoiRepository>()
            .AddScoped<IImportStatisticRepository, ImportStatisticRepository>()
            .AddScoped<IReferendumRepository, ReferendumRepository>()
            .AddScoped<IUserNotificationRepository, UserNotificationRepository>()
            .AddScoped<ICollectionMessageRepository, CollectionMessageRepository>()
            .AddScoped<ICollectionCitizenLogRepository, CollectionCitizenLogRepository>()
            .AddScoped<ICollectionCitizenRepository, CollectionCitizenRepository>()
            .AddScoped<ICertificateRepository, CertificateRepository>()
            .AddScoped<ISecondFactorTransactionRepository, SecondFactorTransactionRepository>()
            .AddScoped<ICollectionRepository, CollectionRepository>()
            .AddScoped<ICollectionSignatureSheetRepository, CollectionSignatureSheetRepository>()
            .AddScoped<IFileRepository, FileRepository>()
            .AddScoped<ICollectionMunicipalityRepository, CollectionMunicipalityRepository>()
            .AddScoped<ICollectionCountRepository, CollectionCountRepository>()
            .AddScoped<IInitiativeCommitteeMemberRepository, InitiativeCommitteeMemberRepository>()
            .AddScoped<Shared.Adapter.Data.Builders.IAuditTrailEntryBuilder, AuditTrailEntryBuilder>()
            .AddVotingLibDatabase<DataContext>();
    }
}
