// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Models;

namespace Voting.ECollecting.Citizen.Domain.Models;

public class Referendum : ReferendumEntity, ICollection
{
    public CollectionUserPermissions? UserPermissions { get; set; }

    // use another name to ensure this never gets mapped automatically by accident.
    public NullableCollectionCount? AttestedCollectionCount { get; set; }

    public bool? IsSigned { get; set; }

    public CollectionSignatureType? SignatureType { get; set; }

    public bool? IsDecreeSigned { get; set; }

    public bool? IsOtherReferendumOfSameDecreeSigned =>
        !IsSigned.HasValue || !IsDecreeSigned.HasValue
            ? null
            : !IsSigned.Value && IsDecreeSigned.Value;

    public IReadOnlySet<string>? SignAcceptedAcrs { get; set; }
}
