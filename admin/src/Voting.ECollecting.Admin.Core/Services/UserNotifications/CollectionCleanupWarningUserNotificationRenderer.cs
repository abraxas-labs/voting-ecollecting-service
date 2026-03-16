// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Core.Configuration;
using Voting.ECollecting.Shared.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Core.Services.UserNotifications;

public class CollectionCleanupWarningUserNotificationRenderer : UserNotificationRenderer
{
    private readonly UrlConfig _urlConfig;

    public CollectionCleanupWarningUserNotificationRenderer(UrlConfig urlConfig)
    {
        _urlConfig = urlConfig;
    }

    protected override string RenderSubject(UserNotificationTemplateBag templateBag)
        => $"'{templateBag.CollectionName}' wird bald gelöscht";

    protected override string RenderBodyHtml(UserNotificationTemplateBag templateBag)
    {
        var collectionUrl = _urlConfig.BuildCollectionUrl(
            templateBag.CollectionId!.Value,
            templateBag.CollectionType!.Value,
            templateBag.RecipientIsCitizen,
            false);

        var collectionNameLink = $"<a href=\"{EncodeHref(collectionUrl)}\">{EncodeHtml(templateBag.CollectionName)}</a>";

        var dateString = templateBag.CollectionCleanupDate.HasValue
            ? $"am {templateBag.CollectionCleanupDate.Value:dd.MM.yyyy}"
            : "demnächst";

        return Html($"""
                    <p>Hallo,</p>
                    <p><strong>{collectionNameLink}</strong> wurde vor längerer Zeit erstellt, aber noch nicht eingereicht.</p>
                    <p>Aus Datenschutzgründen wird die Sammlung und alle zugehörigen Daten {dateString} unwiderruflich gelöscht.</p>
                    <p>Falls Sie die Sammlung weiter bearbeiten und einreichen möchten, loggen Sie sich bitte im E-Collecting ein und schliessen Sie die Erfassung ab.</p>
                    """);
    }
}
