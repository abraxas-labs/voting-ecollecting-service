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
        => "E-Collecting: Neue Berechtigung";

    protected override string RenderBodyHtml(UserNotificationTemplateBag templateBag)
    {
        var url = _urlConfig.BuildPermissionApprovalUrl(templateBag.PermissionToken);
        return Html($"""
                      <h2>Neue Berechtigung in E-Collecting</h2>
                      <p>Hallo,</p>
                      <p>
                      Im E-Collecting wurde eine neue Berechtigung in <a href="{EncodeHref(url)}">{EncodeHtml(templateBag.CollectionName)}</a> für Sie hinzugefügt.
                      Klicken Sie <a href="{EncodeHref(url)}">hier</a> um diese anzunehmen oder abzulehnen.
                      </p>
                      """);
    }
}
