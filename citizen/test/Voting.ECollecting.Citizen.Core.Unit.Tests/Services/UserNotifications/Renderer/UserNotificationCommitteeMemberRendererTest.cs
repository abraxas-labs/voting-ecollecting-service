// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Core.Services.UserNotifications.Renderer;
using Voting.ECollecting.Shared.Core.Configuration;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Core.Unit.Tests.Services.UserNotifications.Renderer;

public class UserNotificationCommitteeMemberRendererTest
{
    private readonly UserNotificationCommitteeMemberRenderer _renderer = new(new UrlConfig
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
                    CollectionId = Guid.Parse("799415ab-7149-4a21-ba39-2b268bf59c33"),
                    CollectionName = "Foo bar baz",
                    CollectionType = CollectionType.Initiative,
                    NotificationType = UserNotificationType.CommitteeMembershipAdded,
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
                    CollectionId = Guid.Parse("799415ab-7149-4a21-ba39-2b268bf59c33"),
                    CollectionName = "bad.example.com baz<script>alert('xss')</script>",
                    CollectionType = CollectionType.Initiative,
                    NotificationType = UserNotificationType.CommitteeMembershipAdded,
                    RecipientIsCitizen = true,
                },
            });
        await Verify(rendered);
    }

    [Fact]
    public async Task ShouldRenderWithPermission()
    {
        var rendered = _renderer.Render(
            new UserNotificationEntity
            {
                RecipientEMail = "foo@example.com",
                TemplateBag = new UserNotificationTemplateBag
                {
                    CollectionId = Guid.Parse("c475ff63-8659-4bcf-ba57-69692e4fb771"),
                    CollectionName = "Foo bar baz",
                    CollectionType = CollectionType.Initiative,
                    NotificationType = UserNotificationType.CommitteeMembershipAddedWithPermission,
                    RecipientIsCitizen = true,
                },
            });
        await Verify(rendered);
    }
}
