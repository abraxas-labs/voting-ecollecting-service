// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Core.Configuration;
using Voting.ECollecting.Shared.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Citizen.Core.Services.UserNotifications.Renderer;

public class UserNotificationCommitteeMemberRenderer : UserNotificationRenderer
{
    private readonly UrlConfig _urlConfig;

    public UserNotificationCommitteeMemberRenderer(UrlConfig urlConfig)
    {
        _urlConfig = urlConfig;
    }

    protected override string RenderSubject(UserNotificationTemplateBag templateBag)
        => "E-Collecting: Anfrage für Mitgliedschaft in einem Komitee";

    protected override string RenderBodyHtml(UserNotificationTemplateBag templateBag)
    {
        var url = _urlConfig.BuildInitiativeCommitteeMembershipApprovalUrl(templateBag.InitiativeCommitteeMembershipToken);
        return Html($"""
                      <p>Guten Tag</p>
                      <p>
                        Das Komitee der Sammlung {EncodeHtml(templateBag.CollectionName)} hat Sie als Komiteemitglied vorgeschlagen.
                        Über den nachfolgenden Link gelangen Sie zur E-Collecting-Plattform, wo Sie die Mitgliedschaft bestätigen oder ablehnen können:
                      </p>
                      <a href="{EncodeHref(url)}">{EncodeHtml(templateBag.CollectionName)}</a>
                      {RenderPermissionHtml(templateBag)}
                      """);
    }

    private string RenderPermissionHtml(UserNotificationTemplateBag templateBag)
    {
        if (templateBag.NotificationType is not UserNotificationType.CommitteeMembershipAddedWithPermission)
        {
            return string.Empty;
        }

        var url = _urlConfig.BuildPermissionApprovalUrl(templateBag.PermissionToken);
        return Html($"""
                     <p>
                        Zudem wurde Ihnen Lese- oder Schreibrechte für die Einrichtung der Sammlung auf der E-Collecting-Plattform erteilt.
                        Um diese anzunehmen oder abzulehnen, klicken Sie bitte <a href="{EncodeHref(url)}">hier</a>.
                     </p>
                     """);
    }
}
