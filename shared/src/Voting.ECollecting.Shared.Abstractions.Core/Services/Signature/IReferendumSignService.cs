// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Abstractions.Core.Services.Signature;

public interface IReferendumSignService : ISignService<ReferendumEntity>
{
    Task<(bool IsCollectionSigned, bool IsDecreeSigned)> IsReferendumOrDecreeSigned(
        ReferendumEntity referendum,
        IVotingStimmregisterPersonInfo personInfo);
}
