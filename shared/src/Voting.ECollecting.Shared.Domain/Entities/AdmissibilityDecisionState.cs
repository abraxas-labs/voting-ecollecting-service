// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Entities;

public enum AdmissibilityDecisionState
{
    Unspecified,
    Open,
    ValidButSubjectToConditions,
    Valid,
    Rejected,
}
