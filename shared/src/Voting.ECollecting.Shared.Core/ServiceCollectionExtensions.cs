// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.Extensions.DependencyInjection;
using Microsoft.IO;
using Voting.ECollecting.Shared.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Signature;
using Voting.ECollecting.Shared.Core.Configuration;
using Voting.ECollecting.Shared.Core.DmDoc;
using Voting.ECollecting.Shared.Core.Services;
using Voting.ECollecting.Shared.Core.Services.Documents;
using Voting.ECollecting.Shared.Core.Services.Signature;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.DmDoc;
using Voting.Lib.DmDoc.Serialization;

namespace Voting.ECollecting.Shared.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedCoreServices(
        this IServiceCollection services,
        UrlConfig urlConfig,
        UserNotificationsConfig userNotificationsConfig,
        DmDocConfig dmDocConfig)
    {
        services
            .AddSingleton(urlConfig)
            .AddSingleton(userNotificationsConfig)
            .AddSingleton(dmDocConfig)
            .AddScoped<ISignService<InitiativeEntity>, InitiativeSignService>()
            .AddScoped<ISignService<ReferendumEntity>, ReferendumSignService>()
            .AddForwardRefScoped<IReferendumSignService, ReferendumSignService>()
            .AddScoped<IUserNotificationService, UserNotificationService>()
            .AddScoped<ICollectionCryptoService, CollectionCryptoService>()
            .AddScoped<IFileService, FileService>()
            .AddScoped<IInitiativeSignatureSheetTemplateGenerator, InitiativeSignatureSheetTemplateGenerator>()
            .AddScoped<IReferendumSignatureSheetTemplateGenerator, ReferendumSignatureSheetTemplateGenerator>()
            .AddScoped<ICommitteeListTemplateGenerator, CommitteeListTemplateGenerator>()
            .AddScoped<IInitiativeCommitteeMemberService, InitiativeCommitteeMemberService>()
            .AddScoped<IOfficialJournalPublicationProtocolGenerator, OfficialJournalPublicationProtocolGenerator>()
            .AddScoped<IElectronicSignaturesProtocolGenerator, ElectronicSignaturesProtocolGenerator>()
            .AddScoped<IAccessControlListDoiService, AccessControlListDoiService>()
            .AddSingleton<RecyclableMemoryStreamManager>();
        return services;
    }

    public static IServiceCollection AddPermissionService<T>(this IServiceCollection services)
        where T : class, IPermissionService
    {
        services.AddForwardRefScoped<IPermissionService, T>();
        return services;
    }

    public static IServiceCollection AddUserNotificationRepo<T>(this IServiceCollection services)
        where T : class, IUserNotificationRepository
    {
        services.AddForwardRefScoped<IUserNotificationRepository, T>();
        return services;
    }

    public static IServiceCollection AddAccessControlListDoiRepository<T>(this IServiceCollection services)
        where T : class, IAccessControlListDoiRepository
    {
        services.AddForwardRefScoped<IAccessControlListDoiRepository, T>();
        return services;
    }

    public static IServiceCollection AddReferendumRepository<T>(this IServiceCollection services)
        where T : class, IReferendumRepository
    {
        services.AddForwardRefScoped<IReferendumRepository, T>();
        return services;
    }

    public static IServiceCollection AddCollectionCitizenLogRepository<T>(this IServiceCollection services)
        where T : class, ICollectionCitizenLogRepository
    {
        services.AddForwardRefScoped<ICollectionCitizenLogRepository, T>();
        return services;
    }

    public static IServiceCollection AddInitiativeRepository<T>(this IServiceCollection services)
        where T : class, IInitiativeRepository
    {
        services.AddForwardRefScoped<IInitiativeRepository, T>();
        return services;
    }

    public static IServiceCollection AddDmDocOrMock(this IServiceCollection services, DmDocConfig config)
    {
#if !RELEASE
        if (config.EnableMock)
        {
            return services.AddSingleton<IDmDocService, DmDocServiceMock>();
        }
#endif

        return services
            .AddDmDoc(config)
            .AddSingleton<IDmDocDataSerializer, DmDocJsonDataSerializer>();
    }
}
