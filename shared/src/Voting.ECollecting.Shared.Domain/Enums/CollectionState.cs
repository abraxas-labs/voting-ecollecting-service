// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Enums;

public enum CollectionState
{
    /// <summary>
    /// State is unspecified.
    /// </summary>
    Unspecified,

    /// <summary>
    /// Vorerfasst (in paper submission).
    /// </summary>
    PreRecorded,

    /// <summary>
    /// In Erfassung.
    /// </summary>
    InPreparation,

    /// <summary>
    /// Zurückgezogen.
    /// </summary>
    Withdrawn,

    /// <summary>
    /// Eingereicht.
    /// </summary>
    Submitted,

    /// <summary>
    /// Nicht zulässig.
    /// </summary>
    NotPassed,

    /// <summary>
    /// In Korrektur.
    /// </summary>
    ReturnedForCorrection,

    /// <summary>
    /// In Prüfung.
    /// </summary>
    UnderReview,

    /// <summary>
    /// Bereit für Anmeldung.
    /// </summary>
    ReadyForRegistration,

    /// <summary>
    /// Angemeldet.
    /// </summary>
    Registered,

    /// <summary>
    /// Sammlung wird aufgeschaltet.
    /// </summary>
    PreparingForCollection,

    /// <summary>
    /// Freigegeben (Initiative) / Erfasst (Referendum).
    /// </summary>
    EnabledForCollection,

    /// <summary>
    /// Unterschriftenlisten eingereicht.
    /// </summary>
    SignatureSheetsSubmitted,

    /// <summary>
    /// Zustandegekommen.
    /// </summary>
    EndedCameAbout,

    /// <summary>
    /// Nicht zustandegekommen.
    /// </summary>
    EndedCameNotAbout,
}
