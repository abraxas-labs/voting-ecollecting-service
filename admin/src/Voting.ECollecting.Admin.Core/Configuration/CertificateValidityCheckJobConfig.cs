// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Scheduler;

namespace Voting.ECollecting.Admin.Core.Configuration;

public class CertificateValidityCheckJobConfig : CronJobConfig
{
    public CertificateValidityCheckJobConfig()
    {
        // by default run daily at 01:00
        CronSchedule = "0 1 * * *";
    }

    /// <summary>
    /// Gets or sets a value indicating whether the certificate validity check job is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the threshold for backup certificates.
    /// </summary>
    public TimeSpan BackupCertificateThreshold { get; set; } = TimeSpan.FromDays(30);

    /// <summary>
    /// Gets or sets the threshold for CA certificates.
    /// </summary>
    public TimeSpan CACertificateThreshold { get; set; } = TimeSpan.FromDays(60);

    /// <summary>
    /// Gets or sets the email addresses to send notifications to.
    /// </summary>
    public List<string> NotificationEmails { get; set; } = ["voting@abraxas.ch"];
}
