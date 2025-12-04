// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Constants;

/// <summary>
/// Enumeration defining the process status code.
/// </summary>
public enum ProcessStatusCode
{
    /// <summary>
    /// Unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Success.
    /// </summary>
    Success = 100,

    /// <summary>
    /// Invalid AHV number N13 Format.
    /// </summary>
    InvalidAhvn13Format = 400,

    /// <summary>
    /// Invalid BFS canton number format.
    /// </summary>
    InvalidBfsCantonFormat = 401,

    /// <summary>
    /// Person not found.
    /// </summary>
    PersonNotFound = 404,

    /// <summary>
    /// E-Voting permission error.
    /// </summary>
    EVotingPermissionError = 410,

    /// <summary>
    /// E-Voting reached max allowed voters limit.
    /// </summary>
    EVotingReachedMaxAllowedVoters = 411,

    /// <summary>
    /// Error indicating E-Voting is not enabled for the municipality.
    /// </summary>
    EVotingNotEnabledError = 412,
}
