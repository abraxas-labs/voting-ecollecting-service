// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Models;

namespace Voting.ECollecting.Admin.Domain.Models;

public interface ICollection
{
    CollectionUserPermissions? UserPermissions { get; set; }

    // use another name to ensure this never gets mapped automatically by accident.
    public NullableCollectionCount? AttestedCollectionCount { get; set; }
}
