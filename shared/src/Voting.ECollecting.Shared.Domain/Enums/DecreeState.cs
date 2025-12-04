// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Enums;

public enum DecreeState
{
    /// <summary>
    /// State is unspecified.
    /// </summary>
    Unspecified,

    /// <summary>
    /// Zur Sammlung freigegeben.
    /// </summary>
    CollectionApplicable,

    /// <summary>
    /// Zustandegekommen.
    /// </summary>
    EndedCameAbout,

    /// <summary>
    /// Nicht zustandegekommen.
    /// </summary>
    EndedCameNotAbout,
}
