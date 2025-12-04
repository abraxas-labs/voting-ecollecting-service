// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Enums;

/// <summary>
/// The DOI types enumeration.
/// </summary>
public enum DomainOfInfluenceType
{
    /// <summary>
    /// Domain of influence (DOI) type is unspecified.
    /// Represents DOI- and district specific extended types.
    /// </summary>
    Unspecified = 0,

    /// <summary>
    /// Switzerland / Confederation (de: Schweiz / Bund).
    /// </summary>
    Ch = 1,

    /// <summary>
    /// The canton (de: Kanton).
    /// </summary>
    Ct = 2,

    /// <summary>
    /// The municipality (de: Gemeinde).
    /// </summary>
    Mu = 3,
}
