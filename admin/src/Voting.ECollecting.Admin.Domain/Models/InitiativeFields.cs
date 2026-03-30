// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Models;

[Flags]
public enum InitiativeFields
{
    None = 0,
    Description = 1 << 0,
    Wording = 1 << 1,
    Reason = 1 << 2,
    Address = 1 << 3,
}
