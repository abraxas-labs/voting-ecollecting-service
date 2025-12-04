// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Core.Services.UserNotifications;

public class CollectionDeletedUserNotificationRenderer : UserNotificationRenderer
{
    protected override string RenderSubject(UserNotificationTemplateBag templateBag)
        => $"{templateBag.CollectionName} gelöscht";

    protected override string RenderBodyHtml(UserNotificationTemplateBag templateBag)
        => Html($"<p>Hallo,</p><p>Im E-Collecting wurde die Sammlung <strong>{EncodeHtml(templateBag.CollectionName)}</strong> gelöscht.</p>");
}
