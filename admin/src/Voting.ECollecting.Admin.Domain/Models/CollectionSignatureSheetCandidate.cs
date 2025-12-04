// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Domain.Models;

public record CollectionSignatureSheetCandidate(IVotingStimmregisterPersonInfo Person)
{
    public CollectionCitizenEntity? ExistingSignature { get; set; }

    public bool ExistingSignatureIsInSameMunicipality { get; set; }

    public bool ExistingSignatureIsOnSameSheet { get; set; }
}
