// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.Cryptography;

namespace Voting.ECollecting.Shared.Core.Services;

public class CollectionCryptoService : ICollectionCryptoService
{
    private readonly ICryptoProvider _cryptoProvider;

    public CollectionCryptoService(ICryptoProvider cryptoProvider)
    {
        _cryptoProvider = cryptoProvider;
    }

    public Task<byte[]> StimmregisterIdHmac(CollectionBaseEntity collection, Guid personRegisterId)
    {
        var idBytes = CollectionCryptoIdSerializer.SerializeStimmregisterId(personRegisterId);
        return _cryptoProvider.CreateHmacSha256(idBytes.ToArray(), GetMacKeyId(collection));
    }

    public Task<byte[]> StimmregisterIdHmac(CollectionBaseEntity collection, IVotingStimmregisterPersonInfo stimmregisterInfo)
        => StimmregisterIdHmac(collection, stimmregisterInfo.RegisterId);

    public Task<IReadOnlyList<byte[]>> StimmregisterIdHmacs(CollectionBaseEntity collection, IReadOnlySet<Guid> personRegisterIds)
    {
        var idsBytes = personRegisterIds.Select(CollectionCryptoIdSerializer.SerializeStimmregisterId);
        return _cryptoProvider.BulkCreateHmacSha256(idsBytes, GetMacKeyId(collection));
    }

    private static string GetMacKeyId(CollectionBaseEntity collection)
        => collection.MacKeyId ?? throw new InvalidOperationException($"MacKeyId is not set on collection with id {collection.Id} in state {collection.State}");
}
