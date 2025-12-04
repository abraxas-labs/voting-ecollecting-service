// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Citizen.Domain.Exceptions;

public class InitiativeAdmissibilityDecisionRejectedException : Exception
{
    public InitiativeAdmissibilityDecisionRejectedException()
        : base("Initiative admissibility decision state cannot be rejected")
    {
    }
}
