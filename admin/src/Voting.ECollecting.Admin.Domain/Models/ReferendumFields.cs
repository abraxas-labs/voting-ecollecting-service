// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Models;

[Flags]
public enum ReferendumFields
{
    None = 0,
    Description = 1 << 0,
    Reason = 1 << 1,
    MembersCommittee = 1 << 2,
    Link = 1 << 3,
    Address = 1 << 4,
}
