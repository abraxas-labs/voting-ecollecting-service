// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Enums;

/// <summary>
/// The DOI types enumeration.
/// </summary>
public enum AclDomainOfInfluenceType
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

    /// <summary>
    /// The catholic church circle (de: Kirchgemeinde (katholisch)).
    /// Represents a specific type for a district.
    /// </summary>
    KiKat = 11,

    /// <summary>
    /// The evangelic church circle (de: Kirchgemeinde (evangelisch)).
    /// Represents a specific type for a district.
    /// </summary>
    KiEva = 12,

    /// <summary>
    /// Another type (de: Verkehrskreis).
    /// Represents a specific type for a district.
    /// </summary>
    AnVek = 13,

    /// <summary>
    /// Another type (de: Wohnviertel).
    /// Represents a specific type for a district.
    /// </summary>
    AnWok = 14,

    /// <summary>
    /// Another type (de: Volkskreis).
    /// Represents a specific type for a district.
    /// </summary>
    AnVok = 15,
}
