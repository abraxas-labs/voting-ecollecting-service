// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Citizen.Domain.Models;

public class CollectionPermission : CollectionPermissionEntity
{
    public CollectionPermissionUserPermissions? UserPermissions { get; set; }
}
