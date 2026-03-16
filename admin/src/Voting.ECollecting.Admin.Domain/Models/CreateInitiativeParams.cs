// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Database.Models;

namespace Voting.ECollecting.Admin.Domain.Models;

public record CreateInitiativeParams(
    DomainOfInfluenceType DomainOfInfluenceType,
    Guid? SubTypeId,
    string Description,
    MarkdownString Wording,
    CollectionAddress? Address,
    string GovernmentDecisionNumber,
    AdmissibilityDecisionState AdmissibilityDecisionState);
