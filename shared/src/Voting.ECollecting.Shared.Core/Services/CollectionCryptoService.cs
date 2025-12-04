// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.Lib.Cryptography;

namespace Voting.ECollecting.Shared.Core.Services;

public class CollectionCryptoService : ICollectionCryptoService
{
    private readonly ICryptoProvider _cryptoProvider;

    public CollectionCryptoService(ICryptoProvider cryptoProvider)
    {
        _cryptoProvider = cryptoProvider;
    }

    public async Task<EncryptStimmregisterIdResult> EncryptStimmregisterId(CollectionBaseEntity collection, IVotingStimmregisterPersonInfo stimmregisterInfo)
    {
        var idBytes = CollectionCryptoIdSerializer.SerializeStimmregisterId(stimmregisterInfo.RegisterId);
        var macTask = StimmregisterIdHmac(collection, stimmregisterInfo.RegisterId);
        var encryptedTask = _cryptoProvider.EncryptAesGcm(idBytes, GetEncryptionKeyId(collection));
        await Task.WhenAll(macTask, encryptedTask);
        return new EncryptStimmregisterIdResult(encryptedTask.Result, macTask.Result);
    }

    public async Task<IReadOnlyList<EncryptStimmregisterIdResult>> EncryptStimmregisterIds(CollectionBaseEntity collection, IReadOnlySet<Guid> personRegisterIds)
    {
        var idBytesList = personRegisterIds.Select(CollectionCryptoIdSerializer.SerializeStimmregisterId).ToList();
        var macTask = StimmregisterIdHmacs(collection, personRegisterIds);
        var encryptedTask = _cryptoProvider.BulkEncryptAesGcm(idBytesList, GetEncryptionKeyId(collection));
        await Task.WhenAll(macTask, encryptedTask);
        return macTask.Result.Zip(encryptedTask.Result, (mac, encrypted) => new EncryptStimmregisterIdResult(encrypted, mac)).ToList();
    }

    public Task<byte[]> StimmregisterIdHmac(CollectionBaseEntity collection, Guid personRegisterId)
    {
        var idBytes = CollectionCryptoIdSerializer.SerializeStimmregisterId(personRegisterId);
        return _cryptoProvider.CreateHmacSha256(idBytes.ToArray(), GetMacKeyId(collection));
    }

    public Task<byte[]> StimmregisterIdHmac(CollectionBaseEntity collection, IVotingStimmregisterPersonInfo stimmregisterInfo)
        => StimmregisterIdHmac(collection, stimmregisterInfo.RegisterId);

    public async Task<Guid> DecryptStimmregisterId(CollectionBaseEntity collection, byte[] encryptedId)
    {
        var decryptedBytes = await _cryptoProvider.DecryptAesGcm(encryptedId, GetEncryptionKeyId(collection));
        return CollectionCryptoIdSerializer.DeserializeStimmregisterId(decryptedBytes);
    }

    public Task<IReadOnlyList<byte[]>> StimmregisterIdHmacs(CollectionBaseEntity collection, IReadOnlySet<Guid> personRegisterIds)
    {
        var idsBytes = personRegisterIds.Select(CollectionCryptoIdSerializer.SerializeStimmregisterId);
        return _cryptoProvider.BulkCreateHmacSha256(idsBytes, GetMacKeyId(collection));
    }

    private static string GetMacKeyId(CollectionBaseEntity collection)
        => collection.MacKeyId ?? throw new InvalidOperationException($"MacKeyId is not set on collection with id {collection.Id} in state {collection.State}");

    private static string GetEncryptionKeyId(CollectionBaseEntity collection)
        => collection.EncryptionKeyId ?? throw new InvalidOperationException($"EncryptionKeyId is not set on collection with id {collection.Id} in state {collection.State}");
}
