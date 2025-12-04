// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Enums;

/// <summary>
/// The Import status enumeration.
/// </summary>
public enum ImportStatus
{
    Unspecified,
    Queued,
    Running,
    Aborted,
    FinishedWithErrors,
    FinishedSuccessfully,
    Stale,
    Failed,
}
