// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Citizen.Core.Exceptions;

public class MaxReferendumsOnDecreeReachedException : Exception
{
    public MaxReferendumsOnDecreeReachedException(Guid decreeId, int maxAllowedReferendumsPerDecree)
        : base($"Only a maximum of {maxAllowedReferendumsPerDecree} referendums on decree {decreeId} are allowed.", null)
    {
    }
}
