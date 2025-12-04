// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using ProtoCitizenModels = Voting.ECollecting.Proto.Citizen.Services.V1.Models;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.AccessibilityTests;

public class AccessibilitySendMessageTest : BaseGrpcTest<AccessibilityService.AccessibilityServiceClient>
{
    public AccessibilitySendMessageTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        await Client.SendMessageAsync(NewValidRequest());

        var userNotification = await RunOnDb(async db => await db.UserNotifications
            .SingleAsync(x => x.TemplateBag.NotificationType == UserNotificationType.AccessibilityMessage));

        await Verify(userNotification);
    }

    private SendAccessibilityMessageRequest NewValidRequest()
    {
        return new SendAccessibilityMessageRequest
        {
            Salutation = ProtoCitizenModels.AccessibilitySalutation.Mrs,
            FirstName = "Petra",
            LastName = "Muster",
            Email = "petra.muster@test.com",
            Phone = "+41 79 456 79 90",
            Category = ProtoCitizenModels.AccessibilityCategory.Various,
            Message = "Test Message",
        };
    }
}
