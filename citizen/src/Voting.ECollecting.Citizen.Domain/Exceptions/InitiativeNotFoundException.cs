// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Exceptions;

namespace Voting.ECollecting.Citizen.Domain.Exceptions;

public class InitiativeNotFoundException : EntityNotFoundException
{
    public InitiativeNotFoundException(string secureIdNumber)
        : base($"Initiative with number {secureIdNumber} not found")
    {
    }
}
