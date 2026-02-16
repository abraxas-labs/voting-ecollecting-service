// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Models;

namespace Voting.ECollecting.Shared.Abstractions.Core.Services.Signature;

public interface ISignService<in T>
    where T : CollectionBaseEntity
{
    Task LockAndEnsureCanSign(T collection, IVotingStimmregisterPersonInfo personInfo, byte[] personCollectionMac);

    Task LockAndEnsureCanSign(T collection, IReadOnlySet<Guid> personRegisterIds, IReadOnlyList<byte[]> personCollectionMacs);

    Task<(bool IsSigned, CollectionSignatureType? SignatureType)> IsCollectionSigned(T collection, IVotingStimmregisterPersonInfo personInfo);
}
