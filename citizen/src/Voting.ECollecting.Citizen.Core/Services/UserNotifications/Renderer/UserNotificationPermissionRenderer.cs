// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Core.Configuration;
using Voting.ECollecting.Shared.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Citizen.Core.Services.UserNotifications.Renderer;

public class UserNotificationPermissionRenderer : UserNotificationRenderer
{
    private readonly UrlConfig _urlConfig;

    public UserNotificationPermissionRenderer(UrlConfig urlConfig)
    {
        _urlConfig = urlConfig;
    }

    protected override string RenderSubject(UserNotificationTemplateBag templateBag)
        => "E-Collecting: Neue Lese- oder Schreibrechte";

    protected override string RenderBodyHtml(UserNotificationTemplateBag templateBag)
    {
        var url = _urlConfig.BuildPermissionApprovalUrl(templateBag.PermissionToken);
        return Html($"""
                      <p>Guten Tag</p>
                      <p>
                      Für die Einrichtung der Sammlung {EncodeHtml(templateBag.CollectionName)} auf der E-Collecting-Plattform wurden Ihnen Lese- oder Schreibrechte erteilt.
                      Um diese anzunehmen oder abzulehnen, klicken Sie bitte <a href="{EncodeHref(url)}">hier</a>.
                      </p>
                      """);
    }
}
