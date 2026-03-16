// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Scheduler;

namespace Voting.ECollecting.Admin.Core.Configuration;

public class CollectionCleanupJobConfig : CronJobConfig
{
    public CollectionCleanupJobConfig()
    {
        // by default run daily at 02:00
        CronSchedule = "0 2 * * *";
    }

    /// <summary>
    /// Gets or sets a value indicating whether the collection cleanup jobs are enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the retention period.
    /// </summary>
    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.FromDays(90);

    /// <summary>
    /// Gets or sets the notification period before deletion.
    /// </summary>
    public TimeSpan NotificationPeriod { get; set; } = TimeSpan.FromDays(14);
}
