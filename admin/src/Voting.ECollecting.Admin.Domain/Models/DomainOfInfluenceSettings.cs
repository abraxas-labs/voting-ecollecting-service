// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Models;

public class DomainOfInfluenceSettings
{
    public int? InitiativeMinSignatureCount { get; set; }

    public int? InitiativeMaxElectronicSignaturePercent { get; set; }

    public int? InitiativeNumberOfMembersCommittee { get; set; }

    public int? ReferendumMinSignatureCount { get; set; }

    public int? ReferendumMaxElectronicSignaturePercent { get; set; }
}
