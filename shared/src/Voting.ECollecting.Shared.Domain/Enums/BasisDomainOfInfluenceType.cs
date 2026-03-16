// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Enums;

/// <summary>
/// The DOI types enumeration.
/// </summary>
public enum BasisDomainOfInfluenceType
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
    /// The district (de: Bezirk).
    /// </summary>
    Bz = 3,

    /// <summary>
    /// The municipality (de: Gemeinde).
    /// </summary>
    Mu = 4,

    /// <summary>
    /// The city district (de: Stadtkreis).
    /// </summary>
    Sk = 5,

    /// <summary>
    /// The school circle (de: Schulkreis).
    /// </summary>
    Sc = 6,

    /// <summary>
    /// The church circle (de: Kirchgemeinde).
    /// </summary>
    Ki = 7,

    /// <summary>
    /// The local community (de: Ortsbürgergemeinde).
    /// </summary>
    Og = 8,

    /// <summary>
    /// The coorperation (de: Koprorationen).
    /// </summary>
    Ko = 9,

    /// <summary>
    /// Other types (de: Andere).
    /// </summary>
    An = 10,
}
