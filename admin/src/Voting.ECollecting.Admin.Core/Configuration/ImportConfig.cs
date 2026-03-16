// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Admin.Core.Configuration;

public class ImportConfig
{
    /// <summary>
    /// Gets or sets the cron schedule expression for DOI synchronization.
    /// </summary>
    public string CronScheduleDoiSync { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the canton which the imported data should be filtered for.
    /// </summary>
    public HashSet<Canton> AllowedCantons { get; set; } = Enum.GetValues<Canton>().ToHashSet();

    /// <summary>
    /// Gets or sets the list of BFS numbers of domain of influences which should be ignored during import.
    /// </summary>
    public HashSet<string> IgnoredBfs { get; set; } = [];

    /// <summary>
    /// Gets or sets the name of this import worker.
    /// </summary>
    public string WorkerName { get; set; } = Environment.MachineName;
}
