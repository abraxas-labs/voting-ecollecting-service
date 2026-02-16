// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Models;

namespace Voting.ECollecting.Citizen.Domain.Models;

public class Initiative : InitiativeEntity, ICollection
{
    public CollectionUserPermissions? UserPermissions { get; set; }

    // ensure the permission-checked count is used.
    public new CollectionCountEntity CollectionCount
    {
        get => throw new InvalidOperationException($"Use {nameof(AttestedCollectionCount)} instead.");
        set => base.CollectionCount = value;
    }

    // use another name to ensure this never gets mapped automatically by accident.
    public NullableCollectionCount? AttestedCollectionCount { get; set; }

    public string? CommitteeDescription { get; set; }

    public bool? IsSigned { get; set; }

    public CollectionSignatureType? SignatureType { get; set; }

    public IReadOnlySet<string>? SignAcceptedAcrs { get; set; }
}
