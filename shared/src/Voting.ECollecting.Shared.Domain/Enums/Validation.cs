// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Enums;

public enum Validation
{
    Unspecified = 0,
    GeneralInformationNotNull = 1,
    HasDeputyPermissions = 2,
    DecreeNotNull = 3,
    ApprovedCommitteeMembersMinValid = 4,
    ApprovedCommitteeMembersMaxValid = 5,
    CommitteeListUploaded = 6,
}
