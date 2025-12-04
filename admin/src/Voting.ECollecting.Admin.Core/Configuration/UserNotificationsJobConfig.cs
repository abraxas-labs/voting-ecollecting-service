// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Scheduler;

namespace Voting.ECollecting.Admin.Core.Configuration;

public class UserNotificationsJobConfig : CronJobConfig
{
    public UserNotificationsJobConfig()
    {
        // default at XX:00 and XX:30
        CronSchedule = "0,30 * * * *";
        CronTimeZone = "Europe/Zurich";
    }

    public int MaxRetries { get; set; } = 5;
}
