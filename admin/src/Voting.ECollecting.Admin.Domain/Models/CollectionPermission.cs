// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Admin.Domain.Models;

public class CollectionPermission
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public CollectionPermissionRole Role { get; set; }
}
