// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Prometheus;

namespace Voting.ECollecting.Citizen.Domain.Diagnostics;

/// <summary>
/// A static Diagnostic class holding prometheus metrics instances and provides methods to update them accordingly.
/// </summary>
public static class DiagnosticsConfig
{
    private static readonly Gauge _importJobsInQueue = Metrics
        .CreateGauge(
        "voting_stimmregister_import_jobs_queued",
        "Number of import jobs waiting for processing in the queue.");

    /// <summary>
    /// Initializes the diagnostic instances.
    /// </summary>
    public static void Initialize()
    {
        _importJobsInQueue.Set(0);
    }
}
