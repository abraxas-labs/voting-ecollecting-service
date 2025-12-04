// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Enums;

public enum CollectionPermissionRole
{
    /// <summary>
    /// Role is unspecified.
    /// </summary>
    Unspecified,

    /// <summary>
    /// Leserecht.
    /// </summary>
    Reader,

    /// <summary>
    /// Stellvertretung.
    /// </summary>
    Deputy,

    /// <summary>
    /// Eigentümer / Ersteller.
    /// </summary>
    Owner,
}
