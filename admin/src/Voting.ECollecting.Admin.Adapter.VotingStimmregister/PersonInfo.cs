// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Domain.Models;

namespace Voting.ECollecting.Admin.Adapter.VotingStimmregister;

public record PersonInfo(
    Guid RegisterId,
    bool IsVotingAllowed,
    bool IsBirthDateValidForVotingRights,
    bool IsNationalityValidForVotingRights,
    string OfficialName,
    string FirstName,
    DateOnly DateOfBirth,
    int Age,
    int Sex,
    string ResidenceAddressStreet,
    string ResidenceAddressHouseNumber,
    string ResidenceAddressTown,
    string ResidenceAddressZipCode,
    int MunicipalityId,
    string MunicipalityName) : IVotingStimmregisterPersonInfo;
