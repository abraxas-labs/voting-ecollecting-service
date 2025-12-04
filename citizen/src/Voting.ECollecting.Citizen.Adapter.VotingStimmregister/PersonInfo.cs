// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Citizen.Adapter.VotingStimmregister;

public record PersonInfo(
    Guid RegisterId,
    int Age,
    int Sex,
    int MunicipalityId,
    string MunicipalityName) : IVotingStimmregisterPersonInfo;
