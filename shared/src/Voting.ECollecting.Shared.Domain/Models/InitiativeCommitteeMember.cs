// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Domain.Models;

public class InitiativeCommitteeMember : InitiativeCommitteeMemberEntity
{
    public string Residence { get; set; } = string.Empty;

    public string PoliticalResidence { get; set; } = string.Empty;
}
