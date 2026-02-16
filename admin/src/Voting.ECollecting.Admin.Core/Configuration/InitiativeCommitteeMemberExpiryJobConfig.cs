// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Scheduler;

namespace Voting.ECollecting.Admin.Core.Configuration;

public class InitiativeCommitteeMemberExpiryJobConfig : CronJobConfig
{
    public InitiativeCommitteeMemberExpiryJobConfig()
    {
        // by default run every 5 minutes
        CronSchedule = "*/5 * * * *";
    }
}
