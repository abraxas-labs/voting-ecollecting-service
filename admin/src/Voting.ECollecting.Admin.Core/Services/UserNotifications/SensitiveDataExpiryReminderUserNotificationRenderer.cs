// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Core.Configuration;
using Voting.ECollecting.Shared.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Core.Services.UserNotifications;

public class SensitiveDataExpiryReminderUserNotificationRenderer : UserNotificationRenderer
{
    private readonly UrlConfig _urlConfig;

    public SensitiveDataExpiryReminderUserNotificationRenderer(UrlConfig urlConfig)
    {
        _urlConfig = urlConfig;
    }

    protected override string RenderSubject(UserNotificationTemplateBag templateBag)
    {
        return "E-Collecting: Erinnerung Kontrollzeichenlöschung";
    }

    protected override string RenderBodyHtml(UserNotificationTemplateBag templateBag)
    {
        var name = templateBag.DecreeName ?? templateBag.CollectionName;
        var controlSignsUrl = _urlConfig.BuildControlSignsUrl();
        var controlSignsLink = $"<a href=\"{EncodeHref(controlSignsUrl)}\">Kontrollzeichenlöschung</a>";

        return Html($"""
            <p>Guten Tag</p>
            <p>Dies ist eine automatische Erinnerung, dass die Kontrollzeichen von <strong>{EncodeHtml(name)}</strong> ab heute gelöscht werden können.</p>
            <p>Zur Kontrollzeichenlöschung: {controlSignsLink}</p>
            """);
    }
}
