// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.Extensions.DependencyInjection;
using Voting.ECollecting.Citizen.Abstractions.Core.Services;
using Voting.ECollecting.Citizen.Abstractions.Core.Services.Signature;
using Voting.ECollecting.Citizen.Core.Configuration;
using Voting.ECollecting.Citizen.Core.Services;
using Voting.ECollecting.Citizen.Core.Services.Documents;
using Voting.ECollecting.Citizen.Core.Services.Signature;
using Voting.ECollecting.Citizen.Core.Services.UserNotifications;
using Voting.ECollecting.Citizen.Core.Services.UserNotifications.Renderer;
using Voting.ECollecting.Citizen.Core.Services.Validation;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.UserNotifications;
using IDomainOfInfluenceService = Voting.ECollecting.Citizen.Abstractions.Core.Services.IDomainOfInfluenceService;
using IInitiativeCommitteeMemberService = Voting.ECollecting.Citizen.Abstractions.Core.Services.IInitiativeCommitteeMemberService;
using InitiativeCommitteeMemberService = Voting.ECollecting.Citizen.Core.Services.InitiativeCommitteeMemberService;
using IUserNotificationService = Voting.ECollecting.Citizen.Abstractions.Core.Services.IUserNotificationService;

namespace Voting.ECollecting.Citizen.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, CoreAppConfig config)
    {
        services
            .AddForwardRefScoped<ICollectionPermissionService, CollectionPermissionService>()
            .AddScoped<CollectionSignService>()
            .AddScoped<PersonInfoResolver>()
            .AddForwardRefScoped<IInitiativeSignService, InitiativeSignService>()
            .AddForwardRefScoped<IReferendumSignService, ReferendumSignService>()
            .AddScoped<IDomainOfInfluenceService, DomainOfInfluenceService>()
            .AddScoped<IInitiativeCommitteeListService, InitiativeCommitteeListService>()
            .AddScoped<IInitiativeCommitteeMemberService, InitiativeCommitteeMemberService>()
            .AddScoped<IInitiativeService, InitiativeService>()
            .AddScoped<IReferendumService, ReferendumService>()
            .AddScoped<IUserNotificationService, UserNotificationService>()
            .AddKeyedSingleton<IUserNotificationRenderer, UserNotificationPermissionRenderer>(UserNotificationType.PermissionAdded)
            .AddKeyedSingleton<IUserNotificationRenderer, UserNotificationCommitteeMemberRenderer>(UserNotificationType.CommitteeMembershipAdded)
            .AddKeyedSingleton<IUserNotificationRenderer>(UserNotificationType.CommitteeMembershipAddedWithPermission, (sp, _) => sp.GetRequiredKeyedService<IUserNotificationRenderer>(UserNotificationType.CommitteeMembershipAdded))
            .AddKeyedSingleton<IUserNotificationRenderer, UserNotificationAccessibilityMessageRenderer>(UserNotificationType.AccessibilityMessage)
            .AddKeyedScoped<ICollectionValidationService, InitiativeValidationService>(CollectionType.Initiative)
            .AddKeyedScoped<ICollectionValidationService, ReferendumValidationService>(CollectionType.Referendum)
            .AddScoped<InitiativeValidationService>()
            .AddScoped<ReferendumValidationService>()
            .AddForwardRefScoped<ICollectionService, CollectionService>()
            .AddForwardRefScoped<ICollectionFilesService, CollectionFilesService>()
            .AddScoped<ICollectionSignatureSheetGenerationService, CollectionSignatureSheetGenerationService>()
            .AddSystemTimeProvider()
            .AddUserNotificationsSmtpSender(config.Smtp);

        if (config.Kms.EnableMock)
        {
            services.AddVotingLibCryptoProviderMock();
        }
        else
        {
            services.AddVotingLibKms(config.Kms);
        }

        return services;
    }
}
