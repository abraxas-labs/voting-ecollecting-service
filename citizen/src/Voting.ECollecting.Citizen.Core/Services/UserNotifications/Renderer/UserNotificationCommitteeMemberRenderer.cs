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
        => "E-Collecting: Einladung zum Komiteemitglied";

    protected override string RenderBodyHtml(UserNotificationTemplateBag templateBag)
    {
        var url = _urlConfig.BuildInitiativeCommitteeMembershipApprovalUrl(templateBag.InitiativeCommitteeMembershipToken);
        return Html($"""
                      <h2>Neue Mitgliedschaft im Initiativkomitee in E-Collecting</h2>
                      <p>Hallo,</p>
                      <p>
                        Im E-Collecting wurden Sie zum Initiativkomitee von {EncodeHtml(templateBag.CollectionName)} hinzugefügt.
                        Bestätigen Sie die Teilnahme <a href="{EncodeHref(url)}">hier</a>.
                      </p>
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
                        Zusätzlich wurde für Sie eine Berechtigung angelegt.
                        Klicken Sie <a href="{EncodeHref(url)}">hier</a> um diese anzunehmen oder abzulehnen.
                     </p>
                     """);
    }
}
