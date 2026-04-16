// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Models;

public class ReferendumDeleteInfo
{
    public Referendum Referendum { get; set; } = null!;

    public string CreatorFullName { get; set; } = string.Empty;

    public string CreatorEmail { get; set; } = string.Empty;
}
