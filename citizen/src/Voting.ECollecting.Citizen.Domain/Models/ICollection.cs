// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Models;

namespace Voting.ECollecting.Citizen.Domain.Models;

public interface ICollection
{
    bool? IsSigned { get; set; }

    DomainOfInfluenceType? DomainOfInfluenceType { get; set; }

    CollectionUserPermissions? UserPermissions { get; set; }

    // use another name to ensure this never gets mapped automatically by accident.
    public NullableCollectionCount? AttestedCollectionCount { get; set; }

    string? Bfs { get; set; }
}
