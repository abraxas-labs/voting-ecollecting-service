// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Shared.Domain.Extensions;

public static class CollectionSignatureSheetStateExtensions
{
    public static bool IsAttestedOrLater(this CollectionSignatureSheetState state)
        => state >= CollectionSignatureSheetState.Attested;
}
