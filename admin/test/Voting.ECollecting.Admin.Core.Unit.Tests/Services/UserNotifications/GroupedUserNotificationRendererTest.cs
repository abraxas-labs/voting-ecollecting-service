// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Core.Services.UserNotifications;
using Voting.ECollecting.Shared.Core.Configuration;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Admin.Core.Unit.Tests.Services.UserNotifications;

public class GroupedUserNotificationRendererTest
{
    private readonly GroupedUserNotificationRenderer _renderer = new(new UrlConfig
    {
        Admin = "http://localhost:5000",
        Citizen = "http://localhost:5001",
    });

    [Fact]
    public async Task ShouldRenderSingleCitizen()
    {
        var rendered = _renderer.Render("foo@example.com", [
            new UserNotificationEntity
            {
                RecipientEMail = "foo@example.com",
                TemplateBag = new UserNotificationTemplateBag
                {
                    CollectionId = Guid.Parse("fd680937-4b7a-46d9-87ef-51540a40aa85"),
                    CollectionName = "Foo bar baz",
                    CollectionType = CollectionType.Initiative,
                    NotificationType = UserNotificationType.MessageAdded,
                    RecipientIsCitizen = true,
                },
            }
        ]);
        await Verify(rendered);
    }

    [Fact]
    public async Task ShouldRenderEncodedSingleCitizen()
    {
        var rendered = _renderer.Render("foo@example.com", [
            new UserNotificationEntity
            {
                RecipientEMail = "foo@example.com",
                TemplateBag = new UserNotificationTemplateBag
                {
                    CollectionId = Guid.Parse("fd680937-4b7a-46d9-87ef-51540a40aa85"),
                    CollectionName = "example.com Foo bar baz<script>alert('xss')</script>",
                    CollectionType = CollectionType.Initiative,
                    NotificationType = UserNotificationType.MessageAdded,
                    RecipientIsCitizen = true,
                },
            }
        ]);
        await Verify(rendered);
    }

    [Fact]
    public async Task ShouldRenderMultipleAdmin()
    {
        var rendered = _renderer.Render("foo@example.com", [
            new UserNotificationEntity
            {
                RecipientEMail = "foo@example.com",
                TemplateBag = new UserNotificationTemplateBag
                {
                    CollectionId = Guid.Parse("fd680937-4b7a-46d9-87ef-51540a40aa85"),
                    CollectionName = "Foo bar baz",
                    CollectionType = CollectionType.Initiative,
                    NotificationType = UserNotificationType.MessageAdded,
                    RecipientIsCitizen = false,
                },
            },

            // same collection
            new UserNotificationEntity
            {
                RecipientEMail = "foo@example.com",
                TemplateBag = new UserNotificationTemplateBag
                {
                    CollectionId = Guid.Parse("fd680937-4b7a-46d9-87ef-51540a40aa85"),
                    CollectionName = "Foo bar baz",
                    CollectionType = CollectionType.Initiative,
                    NotificationType = UserNotificationType.StateChanged,
                    RecipientIsCitizen = false,
                },
            },

            // other collection
            new UserNotificationEntity
            {
                RecipientEMail = "foo@example.com",
                TemplateBag = new UserNotificationTemplateBag
                {
                    CollectionId = Guid.Parse("dbd88deb-a930-4ac9-adb5-b5656a14496a"),
                    CollectionName = "Foo bar baz2",
                    CollectionType = CollectionType.Initiative,
                    NotificationType = UserNotificationType.MessageAdded,
                    RecipientIsCitizen = false,
                },
            }
        ]);
        await Verify(rendered);
    }
}
