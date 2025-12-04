// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Core.Services.UserNotifications;

public class DecreeDeletedUserNotificationRenderer : UserNotificationRenderer
{
    protected override string RenderSubject(UserNotificationTemplateBag templateBag)
        => $"{templateBag.DecreeName} gelöscht";

    protected override string RenderBodyHtml(UserNotificationTemplateBag templateBag)
        => Html($"<p>Hallo,</p><p>Im E-Collecting wurde der Erlass <strong>{EncodeHtml(templateBag.DecreeName)}</strong> gelöscht.</p>");
}
