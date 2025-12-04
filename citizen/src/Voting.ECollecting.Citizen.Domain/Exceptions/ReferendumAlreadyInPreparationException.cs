// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Citizen.Domain.Exceptions;

public class ReferendumAlreadyInPreparationException : Exception
{
    public ReferendumAlreadyInPreparationException(string number)
        : base($"Referendum with number {number} is already in preparation")
    {
    }
}
