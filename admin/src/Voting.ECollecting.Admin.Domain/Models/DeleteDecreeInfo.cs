// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Models;

public class DeleteDecreeInfo
{
    public Decree Decree { get; set; } = null!;

    public List<ReferendumDeleteInfo> Referendums { get; set; } = [];
}
