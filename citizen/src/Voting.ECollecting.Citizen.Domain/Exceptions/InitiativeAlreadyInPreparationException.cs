// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Citizen.Domain.Exceptions;

public class InitiativeAlreadyInPreparationException : Exception
{
    public InitiativeAlreadyInPreparationException(string governmentDecisionNumber)
        : base($"Initiative with government decision number {governmentDecisionNumber} is already in preparation")
    {
    }
}
