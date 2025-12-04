// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Admin.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Admin.Api.Http.Controllers;
using Voting.ECollecting.Admin.WebService.Integration.Tests.Mocks;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.Lib.Iam.SecondFactor.Services;
using Voting.Lib.Testing.Mocks;
using Voting.Lib.UserNotifications;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests;

public class TestStartup : Startup
{
    public TestStartup(IConfiguration configuration)
        : base(configuration)
    {
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services
            .AddMockedTimeProvider()
            .AddVotingLibIamMocks()
            .AddVotingLibCryptoProviderMock()
            .RemoveHostedServices()
            .AddMock<ISignatureSheetAttestationGenerator, SignatureSheetAttestationGeneratorMock>()
            .AddMock<IInitiativeSignatureSheetTemplateGenerator, InitiativeSignatureSheetTemplateGeneratorMock>()
            .AddMock<IReferendumSignatureSheetTemplateGenerator, ReferendumSignatureSheetTemplateGeneratorMock>()
            .AddMock<IUserNotificationSender, UserNotificationSenderMock>()
            .AddMock<ISecondFactorTransactionService, SecondFactorTransactionServiceMock>()
            .AddMock<IOfficialJournalPublicationProtocolGenerator, OfficialJournalPublicationProtocolGeneratorMock>()
            .AddMock<IElectronicSignaturesProtocolGenerator, ElectronicSignaturesProtocolGeneratorMock>();

        services.RemoveAll<IPermissionService>();
        services.TryAddScoped<PermissionServiceMock>();
        services.AddScoped<IPermissionService>(sp => sp.GetRequiredService<PermissionServiceMock>());

        // controllers otherwise are not found
        services.AddControllers().AddApplicationPart(typeof(CollectionController).Assembly);
    }

    protected override void ConfigureAuthentication(AuthenticationBuilder builder)
        => builder.AddMockedSecureConnectScheme();
}
