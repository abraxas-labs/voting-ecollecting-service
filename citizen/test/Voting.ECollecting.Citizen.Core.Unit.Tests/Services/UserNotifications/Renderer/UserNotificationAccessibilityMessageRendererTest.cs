// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Core.Services.UserNotifications.Renderer;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Models;

namespace Voting.ECollecting.Citizen.Core.Unit.Tests.Services.UserNotifications.Renderer;

public class UserNotificationAccessibilityMessageRendererTest
{
    private readonly UserNotificationAccessibilityMessageRenderer _renderer = new();

    [Fact]
    public async Task ShouldRender()
    {
        var rendered = _renderer.Render(
            new UserNotificationEntity
            {
                RecipientEMail = "support@sg.ch",
                TemplateBag = new UserNotificationTemplateBag
                {
                    NotificationType = UserNotificationType.AccessibilityMessage,
                    AccessibilityMessage = new AccessibilityMessage
                    {
                        Salutation = AccessibilitySalutation.Mrs,
                        FirstName = "Petra",
                        LastName = "Muster",
                        Email = "petra.muster@example.com",
                        Phone = "071 123 45 67",
                        Category = AccessibilityCategory.OptimisationProposal,
                        Message = "Test",
                    },
                },
            });
        await Verify(rendered);
    }

    [Fact]
    public async Task ShouldRenderWithoutOptionalFields()
    {
        var rendered = _renderer.Render(
            new UserNotificationEntity
            {
                RecipientEMail = "support@sg.ch",
                TemplateBag = new UserNotificationTemplateBag
                {
                    NotificationType = UserNotificationType.AccessibilityMessage,
                    AccessibilityMessage = new AccessibilityMessage
                    {
                        Email = "petra.muster@example.com",
                        Category = AccessibilityCategory.OptimisationProposal,
                        Message = "Test",
                    },
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
                RecipientEMail = "support@sg.ch",
                TemplateBag = new UserNotificationTemplateBag
                {
                    NotificationType = UserNotificationType.AccessibilityMessage,
                    AccessibilityMessage = new AccessibilityMessage
                    {
                        Salutation = AccessibilitySalutation.Mrs,
                        FirstName = "example.com Foo bar baz<script>alert('xss')</script>",
                        LastName = "example.com Foo bar baz<script>alert('xss')</script>",
                        Email = "example.com Foo bar baz<script>alert('xss')</script>",
                        Phone = "example.com Foo bar baz<script>alert('xss')</script>",
                        Category = AccessibilityCategory.OptimisationProposal,
                        Message = "example.com Foo bar baz<script>alert('xss')</script>",
                    },
                },
            });
        await Verify(rendered);
    }
}
