// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Abstractions.Core.Services;

public interface ICollectionCryptoService
{
    Task<byte[]> StimmregisterIdHmac(CollectionBaseEntity collection, Guid personRegisterId);

    Task<byte[]> StimmregisterIdHmac(CollectionBaseEntity collection, IVotingStimmregisterPersonInfo stimmregisterInfo)
        => StimmregisterIdHmac(collection, stimmregisterInfo.RegisterId);

    Task<IReadOnlyList<byte[]>> StimmregisterIdHmacs(CollectionBaseEntity collection, IReadOnlySet<Guid> personRegisterIds);
}
