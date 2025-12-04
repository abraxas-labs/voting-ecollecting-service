// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Models;

public interface IVotingStimmregisterPersonInfo : Shared.Domain.Entities.IVotingStimmregisterPersonInfo
{
    bool IsVotingAllowed { get; }

    bool IsBirthDateValidForVotingRights { get; }

    bool IsNationalityValidForVotingRights { get; }

    string OfficialName { get; }

    string FirstName { get; }

    DateOnly DateOfBirth { get; }

    string ResidenceAddressStreet { get; }

    string ResidenceAddressHouseNumber { get; }

    string ResidenceAddressTown { get; }

    string ResidenceAddressZipCode { get; }
}
