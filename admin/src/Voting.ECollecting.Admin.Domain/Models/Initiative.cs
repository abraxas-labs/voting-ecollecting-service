// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Models;

namespace Voting.ECollecting.Admin.Domain.Models;

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
}
