// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin;
using Voting.ECollecting.Citizen.Api.Http.Controllers;
using Voting.ECollecting.Citizen.WebService.Integration.Tests.Mocks;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.Lib.Testing.Mocks;
using Voting.Lib.UserNotifications;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests;

public class TestStartup : Startup
{
    public TestStartup(IConfiguration configuration, IWebHostEnvironment environment)
        : base(configuration, environment)
    {
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services
            .AddMockedTimeProvider()
            .RemoveHostedServices()
            .AddMock<ICommitteeListTemplateGenerator, CommitteeListTemplateGeneratorMock>()
            .AddMock<IElectronicSignaturesProtocolGenerator, ElectronicSignaturesProtocolGeneratorMock>()
            .AddMock<IInitiativeSignatureSheetTemplateGenerator, InitiativeSignatureSheetTemplateGeneratorMock>()
            .AddMock<IReferendumSignatureSheetTemplateGenerator, ReferendumSignatureSheetTemplateGeneratorMock>()
            .AddMock<IUserNotificationSender, UserNotificationSenderMock>();

        services.RemoveAll<IPermissionService>();
        services.TryAddScoped<PermissionServiceMock>();
        services.AddScoped<IPermissionService>(sp => sp.GetRequiredService<PermissionServiceMock>());

        // controllers otherwise are not found
        services.AddControllers().AddApplicationPart(typeof(CollectionController).Assembly);
    }

    protected override void ConfigureAuthentication(AuthenticationBuilder builder) =>
        builder.AddScheme<JwtBearerOptions, AuthenticationHandlerMock>(JwtBearerDefaults.AuthenticationScheme, _ => { });
}
