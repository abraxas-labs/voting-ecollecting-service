// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.Extensions.DependencyInjection;
using Microsoft.IO;
using Voting.ECollecting.Admin.Abstractions.Core.HostedServices;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Admin.Core.HostedServices;
using Voting.ECollecting.Admin.Core.Import;
using Voting.ECollecting.Admin.Core.Services;
using Voting.ECollecting.Admin.Core.Services.Crypto;
using Voting.ECollecting.Admin.Core.Services.Documents;
using Voting.ECollecting.Admin.Core.Services.Signature;
using Voting.ECollecting.Admin.Core.Services.UserNotifications;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.Iam.SecondFactor.DependencyInjection;
using Voting.Lib.Scheduler;
using Voting.Lib.UserNotifications;
using IAccessControlListDoiService = Voting.ECollecting.Admin.Abstractions.Core.Services.IAccessControlListDoiService;

namespace Voting.ECollecting.Admin.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(
        this IServiceCollection services,
        CoreAppConfig config)
    {
        services
            .AddSingleton(config.UserNotificationsJob)
            .AddSingleton(config)
            .AddSecondFactorTransactionProvider<SecondFactorTransactionStorageService>(config.SecondFactorTransaction)
            .AddForwardRefScoped<ICollectionService, CollectionService>()
            .AddForwardRefScoped<IDecreeService, DecreeService>()
            .AddForwardRefScoped<IAccessControlListDoiService, AccessControlListDoiService>()
            .AddScoped<ICollectionFilesService, CollectionFilesService>()
            .AddScoped<CollectionCryptoService>()
            .AddScoped<IDomainOfInfluenceFilesService, DomainOfInfluenceFilesService>()
            .AddScoped<IDomainOfInfluenceService, DomainOfInfluenceService>()
            .AddScoped<InitiativeSignService>()
            .AddScoped<ReferendumSignService>()
            .AddScoped<CollectionSignService>()
            .AddScoped<CertificateValidator>()
            .AddKeyedScoped<IUserNotificationRenderer, CollectionDeletedUserNotificationRenderer>(UserNotificationType.CollectionDeleted)
            .AddKeyedScoped<IUserNotificationRenderer, DecreeDeletedUserNotificationRenderer>(UserNotificationType.DecreeDeleted)
            .AddScoped<ICertificateService, CertificateService>()
            .AddScoped<IDomainOfInfluenceService, DomainOfInfluenceService>()
            .AddForwardRefScoped<IInitiativeService, InitiativeService>()
            .AddScoped<IReferendumService, ReferendumService>()
            .AddScoped<IInitiativeCommitteeService, InitiativeCommitteeService>()
            .AddScoped<IInitiativeAdmissibilityDecisionService, InitiativeAdmissibilityDecisionService>()
            .AddCronJob<InitiativeCommitteeMemberExpiryJob>(config.InitiativeCommitteeMemberExpiryJob)
            .AddCronJob<CollectionPermissionExpiryJob>(config.CollectionPermissionExpiryJob)
            .AddCronJob<UserNotificationSenderJob>(config.UserNotificationsJob)
            .AddScoped<GroupedUserNotificationRenderer>()
            .AddScoped<ISignatureSheetAttestationGenerator, SignatureSheetAttestationGenerator>()
            .AddScoped<ISignatureSheetAttestationGenerationService, SignatureSheetAttestationGenerationService>()
            .AddScoped<ICollectionSignatureSheetGenerationService, CollectionSignatureSheetGenerationService>()
            .AddScoped<ICollectionMunicipalityService, CollectionMunicipalityService>()
            .AddScoped<ICollectionSignatureSheetService, CollectionSignatureSheetService>()
            .AddUserNotificationsSmtpSender(config.Smtp)
            .AddSingleton<RecyclableMemoryStreamManager>()
            .AddSystemTimeProvider()
            .AddSingleton(config.Csv)
            .AddScoped<CsvService>()
            .AddScoped<IStatisticalDataCsvGenerator, StatisticalDataCsvGenerator>()
            .AddScoped<IStatisticalDataTimeLapseCsvGenerator, StatisticalDataTimeLapseCsvGenerator>();

        if (config.Kms.EnableMock)
        {
            services.AddVotingLibCryptoProviderMock();
        }
        else
        {
            services.AddVotingLibKms(config.Kms);
        }

        // AddHostedService (Generic) only supports specifying the implementation, but not the interface.
        // This is done to Be able to replace the HostedService with a mock in integration tests
        services.AddSingleton(config.Import);
        services.AddSingleton<IAccessControlListDoiHostedService, AccessControlListDoiHostedService>();
        services.AddTransient<IAccessControlListImporter, AccessControlListImporter>();
        services.AddHostedService(sp => sp.GetRequiredService<IAccessControlListDoiHostedService>());

        return services;
    }
}
