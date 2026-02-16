// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Core.Resources;
using Voting.ECollecting.Shared.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Core.Services.UserNotifications.Renderer;

public class UserNotificationAccessibilityMessageRenderer : UserNotificationRenderer
{
    protected override string RenderSubject(UserNotificationTemplateBag templateBag)
        => "E-Collecting: Feedback Accessibility";

    protected override string RenderBodyHtml(UserNotificationTemplateBag templateBag)
    {
        if (templateBag.AccessibilityMessage == null)
        {
            throw new InvalidOperationException($"{nameof(templateBag.AccessibilityMessage)} cannot be null");
        }

        var salutation = templateBag.AccessibilityMessage.Salutation != null &&
                         templateBag.AccessibilityMessage.Salutation != AccessibilitySalutation.Unspecified
            ? Strings.ResourceManager.GetString(
                  $"UserNotification.AccessibilityMessage.Salutation.{templateBag.AccessibilityMessage.Salutation.ToString()}") ??
              string.Empty
            : Strings.UserNotification_AccessibilityMessage_NoData;

        var category = templateBag.AccessibilityMessage.Category != AccessibilityCategory.Unspecified
            ? Strings.ResourceManager.GetString(
                  $"UserNotification.AccessibilityMessage.Category.{templateBag.AccessibilityMessage.Category.ToString()}") ??
              string.Empty
            : Strings.UserNotification_AccessibilityMessage_NoData;

        return Html($"""
                      <h2>Feedback Accessibility</h2>
                      <p>Anrede: {EncodeHtml(salutation)}</p>
                      <p>Vorname: {EncodeHtml(string.IsNullOrEmpty(templateBag.AccessibilityMessage.FirstName) ? Strings.UserNotification_AccessibilityMessage_NoData : templateBag.AccessibilityMessage.FirstName)}</p>
                      <p>Nachname: {EncodeHtml(string.IsNullOrEmpty(templateBag.AccessibilityMessage.LastName) ? Strings.UserNotification_AccessibilityMessage_NoData : templateBag.AccessibilityMessage.LastName)}</p>
                      <p>Email: {EncodeHtml(templateBag.AccessibilityMessage.Email)}</p>
                      <p>Telefon: {EncodeHtml(string.IsNullOrEmpty(templateBag.AccessibilityMessage.Phone) ? Strings.UserNotification_AccessibilityMessage_NoData : templateBag.AccessibilityMessage.Phone)}</p>
                      <p>Kategorie: {EncodeHtml(category)}</p>
                      <p>Meldung:<br/><span class="user-text">{EncodeHtml(templateBag.AccessibilityMessage.Message)}</span></p>
                     """);
    }
}
