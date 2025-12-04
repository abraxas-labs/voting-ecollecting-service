// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Admin.Domain.Models;

public record CreateInitiativeParams(
    DomainOfInfluenceType DomainOfInfluenceType,
    Guid? SubTypeId,
    string Description,
    string Wording,
    CollectionAddress? Address,
    string GovernmentDecisionNumber,
    AdmissibilityDecisionState AdmissibilityDecisionState);
