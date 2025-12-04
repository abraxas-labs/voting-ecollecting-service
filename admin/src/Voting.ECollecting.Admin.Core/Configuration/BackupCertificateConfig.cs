// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Core.Configuration;

public class BackupCertificateConfig
{
    /// <summary>
    /// Gets or sets the CA certificate to validate the backup certificate.
    /// Needs to contain a single public key in the PEM format.
    /// </summary>
    public string CACertificate { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the grace period of the NotAfter of the CA certificate.
    /// If NotAfter after is less than the grace period from now, a warning is issued.
    /// </summary>
    public TimeSpan CACertificateNotAfterGracePeriod { get; set; } = TimeSpan.FromDays(180);

    /// <summary>
    /// Gets or sets the min. validity period of the CA certificate.
    /// If NotAfter is less than the validity period from now, an error is issued.
    /// </summary>
    public TimeSpan CACertificateNotAfterValidityPeriod { get; set; } = TimeSpan.FromDays(120);

    /// <summary>
    /// Gets or sets the grace period of the NotAfter of the backup certificate.
    /// If NotAfter is less than the grace period from now, a warning is issued.
    /// </summary>
    public TimeSpan NotAfterGracePeriod { get; set; } = TimeSpan.FromDays(14);

    /// <summary>
    /// Gets or sets the allowed file extensions.
    /// </summary>
    public HashSet<string> AllowedFileExtensions { get; set; } = new();
}
