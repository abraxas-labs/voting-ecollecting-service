// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Domain.Models;

public class DomainOfInfluence
{
    public string Name { get; set; } = string.Empty;

    public DomainOfInfluenceType Type { get; set; }

    public string Bfs { get; set; } = string.Empty;

    public int InitiativeMinSignatureCount { get; set; }

    public int InitiativeMaxElectronicSignaturePercent { get; set; }

    public int InitiativeNumberOfMembersCommittee { get; set; }

    public int ReferendumMinSignatureCount { get; set; }

    public int ReferendumMaxElectronicSignaturePercent { get; set; }

    public bool ECollectingEnabled { get; set; }
}
