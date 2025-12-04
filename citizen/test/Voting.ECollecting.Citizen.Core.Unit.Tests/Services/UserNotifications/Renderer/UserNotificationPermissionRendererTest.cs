// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Core.Services.UserNotifications.Renderer;
using Voting.ECollecting.Shared.Core.Configuration;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Core.Unit.Tests.Services.UserNotifications.Renderer;

public class UserNotificationPermissionRendererTest
{
    private readonly UserNotificationPermissionRenderer _renderer = new(new UrlConfig
    {
        Admin = "http://localhost:5000",
        Citizen = "http://localhost:5001",
    });

    [Fact]
    public async Task ShouldRender()
    {
        var rendered = _renderer.Render(
            new UserNotificationEntity
            {
                RecipientEMail = "foo@example.com",
                TemplateBag = new UserNotificationTemplateBag
                {
                    CollectionId = Guid.Parse("c9d38f39-e237-4c6b-a5b5-f845f791a695"),
                    CollectionName = "Foo bar baz",
                    CollectionType = CollectionType.Initiative,
                    NotificationType = UserNotificationType.PermissionAdded,
                    RecipientIsCitizen = true,
                },
            });
        await Verify(rendered);
    }

    [Fact]
    public async Task ShouldRenderEncoded()
    {
        var rendered = _renderer.Render(
            new UserNotificationEntity
            {
                RecipientEMail = "foo@example.com",
                TemplateBag = new UserNotificationTemplateBag
                {
                    CollectionId = Guid.Parse("c9d38f39-e237-4c6b-a5b5-f845f791a695"),
                    CollectionName = "example.com Foo bar baz<script>alert('xss')</script>",
                    CollectionType = CollectionType.Initiative,
                    NotificationType = UserNotificationType.PermissionAdded,
                    RecipientIsCitizen = true,
                },
            });
        await Verify(rendered);
    }
}
