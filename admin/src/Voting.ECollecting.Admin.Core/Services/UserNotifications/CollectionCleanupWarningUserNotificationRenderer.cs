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
        => "E-Collecting: Ihre Sammlung wird bald gelöscht";

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

        return Html($$"""
                    <p>Guten Tag</p>
                    <p>Die Sammlung <strong>{collectionNameLink}</strong> wurde vor längerer Zeit erstellt, aber die Einrichtung wurde nicht abgeschlossen.</p>
                    <p>Aus Datenschutzgründen wird die Sammlung und damit alle bereits erfassten Informationen am {{dateString}} unwiderruflich gelöscht.</p>
                    <p>Falls Sie die Einrichtung der Sammlung abschliessen und diese zur Prüfung der Zulässigkeit einreichen möchten, loggen Sie sich bitte auf der E-Collecting-Plattform ein und schliessen Sie die Einrichtung ab.</p>
                    """);
    }
}
