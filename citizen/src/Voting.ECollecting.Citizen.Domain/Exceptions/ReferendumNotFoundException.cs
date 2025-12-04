// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Exceptions;

namespace Voting.ECollecting.Citizen.Domain.Exceptions;

public class ReferendumNotFoundException : EntityNotFoundException
{
    public ReferendumNotFoundException(string number)
        : base($"Referendum with number {number} not found", null)
    {
    }
}
