// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Entities;

public interface IVotingStimmregisterPersonInfo
{
    Guid RegisterId { get; }

    int Sex { get; }

    int Age { get; }

    int MunicipalityId { get; }

    string MunicipalityName { get; }
}
