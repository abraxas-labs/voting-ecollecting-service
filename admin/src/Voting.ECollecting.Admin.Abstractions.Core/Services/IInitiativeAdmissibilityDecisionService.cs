// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Abstractions.Core.Services;

public interface IInitiativeAdmissibilityDecisionService
{
    Task<List<Initiative>> ListEligibleForAdmissibilityDecision();

    Task<List<Initiative>> ListAdmissibilityDecisions();

    Task DeleteAdmissibilityDecision(Guid id);

    Task CreateLinkedAdmissibilityDecision(
        Guid initiativeId,
        string governmentDecisionNumber,
        AdmissibilityDecisionState state);

    Task<Guid> CreateWithAdmissibilityDecision(CreateInitiativeParams reqParams);

    Task UpdateAdmissibilityDecision(
        Guid initiativeId,
        string? governmentDecisionNumber,
        AdmissibilityDecisionState state);
}
