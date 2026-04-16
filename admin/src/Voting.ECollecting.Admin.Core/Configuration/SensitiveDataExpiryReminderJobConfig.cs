// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Scheduler;

namespace Voting.ECollecting.Admin.Core.Configuration;

public class SensitiveDataExpiryReminderJobConfig : CronJobConfig
{
    public SensitiveDataExpiryReminderJobConfig()
    {
        // by default run daily at 05:00
        CronSchedule = "0 5 * * *";
    }

    /// <summary>
    /// Gets or sets a value indicating whether the sensitive data expiry job is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;
}
