// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Citizen.Domain.Exceptions;

public class InitiativeAlreadyInPreparationException : Exception
{
    public InitiativeAlreadyInPreparationException(string secureIdNumber)
        : base($"Initiative with number {secureIdNumber} is already in preparation")
    {
    }
}
