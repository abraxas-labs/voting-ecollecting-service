// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Models;

namespace Voting.ECollecting.Shared.Abstractions.Core.Services;

public interface ICollectionCryptoService
{
    Task<EncryptStimmregisterIdResult> EncryptStimmregisterId(
        CollectionBaseEntity collection,
        IVotingStimmregisterPersonInfo stimmregisterInfo);

    Task<IReadOnlyList<EncryptStimmregisterIdResult>> EncryptStimmregisterIds(CollectionBaseEntity collection, IReadOnlySet<Guid> personRegisterIds);

    Task<Guid> DecryptStimmregisterId(CollectionBaseEntity collection, byte[] encryptedId);

    Task<byte[]> StimmregisterIdHmac(CollectionBaseEntity collection, Guid personRegisterId);

    Task<byte[]> StimmregisterIdHmac(CollectionBaseEntity collection, IVotingStimmregisterPersonInfo stimmregisterInfo)
        => StimmregisterIdHmac(collection, stimmregisterInfo.RegisterId);

    Task<IReadOnlyList<byte[]>> StimmregisterIdHmacs(CollectionBaseEntity collection, IReadOnlySet<Guid> personRegisterIds);
}
