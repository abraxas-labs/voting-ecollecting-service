// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Core.Configuration;
using Voting.ECollecting.Shared.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Core.Services.UserNotifications;

public class CertificateValidityWarningUserNotificationRenderer : UserNotificationRenderer
{
    private readonly UrlConfig _urlConfig;

    public CertificateValidityWarningUserNotificationRenderer(UrlConfig urlConfig)
    {
        _urlConfig = urlConfig;
    }

    protected override string RenderSubject(UserNotificationTemplateBag templateBag)
        => $"E-Collecting: Zertifikat läuft ab: {GetCertificateType(templateBag)}";

    protected override string RenderBodyHtml(UserNotificationTemplateBag templateBag)
    {
        var type = GetCertificateType(templateBag);
        var dateString = templateBag.CertificateExpirationDate?.ToString("dd.MM.yyyy HH:mm") ?? "unbekannt";

        var certificateManagementUrl = _urlConfig.BuildCertificateManagementUrl();
        var certificateManagementLink = $"<a href=\"{EncodeHref(certificateManagementUrl)}\">Schlüsselmanagement</a>";

        return Html($"""
                    <p>Guten Tag</p>
                    <p>Das aktuelle {EncodeHtml(type)} läuft bald ab:</p>
                    <ul>
                        <li><strong>Ablaufdatum:</strong> {EncodeHtml(dateString)}</li>
                    </ul>
                    <p>Bitte erneuern Sie das Zertifikat rechtzeitig, um einen reibungslosen Betrieb sicherzustellen: {certificateManagementLink}</p>
                    """);
    }

    private static string GetCertificateType(UserNotificationTemplateBag templateBag)
        => templateBag.IsCaCertificate ? "CA-Zertifikat" : "Backup-Zertifikat";
}
