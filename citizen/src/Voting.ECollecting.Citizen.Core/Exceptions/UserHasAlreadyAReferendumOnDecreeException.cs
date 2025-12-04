// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Citizen.Core.Exceptions;

public class UserHasAlreadyAReferendumOnDecreeException : Exception
{
    public UserHasAlreadyAReferendumOnDecreeException(Guid decreeId)
        : base($"User has already a referendum on decree {decreeId}", null)
    {
    }
}
