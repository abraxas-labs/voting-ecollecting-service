// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Models;

public record DomainOfInfluenceSettings(
    int? InitiativeMinSignatureCount,
    int? InitiativeMaxElectronicSignaturePercent,
    int? InitiativeNumberOfMembersCommittee,
    int? ReferendumMinSignatureCount,
    int? ReferendumMaxElectronicSignaturePercent,
    bool? ECollectingEnabled);
