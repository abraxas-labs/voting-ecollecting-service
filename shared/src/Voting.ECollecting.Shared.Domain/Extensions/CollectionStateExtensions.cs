// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Shared.Domain.Extensions;

public static class CollectionStateExtensions
{
    public static bool InPreparationOrReturnForCorrection(this CollectionState state)
        => state
            is CollectionState.InPreparation
            or CollectionState.ReturnedForCorrection;

    public static bool IsEnabledForCollectionOrEnded(this CollectionState state)
        => state.IsEnded() || state is CollectionState.EnabledForCollection;

    public static bool IsEndedOrAborted(this CollectionState state)
        => state.IsEnded() || state is CollectionState.Withdrawn or CollectionState.NotPassed;

    public static bool IsNotEndedAndNotAborted(this CollectionState state)
        => !state.IsEndedOrAborted();

    public static bool IsEnded(this CollectionState state)
        => state
            is CollectionState.SignatureSheetsSubmitted
            or CollectionState.EndedCameAbout
            or CollectionState.EndedCameNotAbout;

    public static bool IsEndedCameAboutOrCameNotAbout(this CollectionState state)
        => state is CollectionState.EndedCameAbout or CollectionState.EndedCameNotAbout;
}
