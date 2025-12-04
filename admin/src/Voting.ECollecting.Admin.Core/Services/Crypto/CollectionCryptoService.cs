// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.Extensions.Logging;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.Cryptography;
using Voting.Lib.Cryptography.Kms.Exceptions;

namespace Voting.ECollecting.Admin.Core.Services.Crypto;

public class CollectionCryptoService
{
    private readonly ICryptoProvider _cryptoProvider;
    private readonly ILogger<CollectionCryptoService> _logger;
    private readonly CoreAppConfig _config;

    public CollectionCryptoService(
        ICryptoProvider cryptoProvider,
        ILogger<CollectionCryptoService> logger,
        CoreAppConfig config)
    {
        _cryptoProvider = cryptoProvider;
        _logger = logger;
        _config = config;
    }

    internal async Task DeleteKeys(CollectionBaseEntity collection)
    {
        if (!string.IsNullOrEmpty(collection.EncryptionKeyId))
        {
            await _cryptoProvider.DeleteAesSecretKey(collection.EncryptionKeyId);
        }

        if (!string.IsNullOrEmpty(collection.MacKeyId))
        {
            await _cryptoProvider.DeleteMacSecretKey(collection.MacKeyId);
        }
    }

    internal async Task<CollectionCryptoKeyGenerateResult> GenerateKey(Guid collectionId)
    {
        var encKeyName = BuildEncryptionKeyName(collectionId);
        var macKeyName = BuildMacKeyName(collectionId);
        var result = new CollectionCryptoKeyGenerateResult
        {
            Id = collectionId,
        };

        if (await TryGenerateKeys(
                a => a.GenerateAesSecretKey(encKeyName),
                a => a.GetAesSecretKeyId(encKeyName),
                collectionId) is { } encryptionKeyId)
        {
            result.EncryptionKeyId = encryptionKeyId;
        }

        if (await TryGenerateKeys(
                a => a.GenerateMacSecretKey(macKeyName),
                a => a.GetMacSecretKeyId(macKeyName),
                collectionId) is { } macKeyId)
        {
            result.MacKeyId = macKeyId;
        }

        return result;
    }

    private async Task<string?> TryGenerateKeys(
        Func<ICryptoProvider, Task<string>> generateKey,
        Func<ICryptoProvider, Task<string>> resolveKey,
        Guid collectionId)
    {
        try
        {
            return await generateKey(_cryptoProvider);
        }
        catch (KmsKeyAlreadyExistsException ex)
        {
            _logger.LogError(
                ex,
                "The encryption or secret key for collection {CollectionId} already exists. Using the existing one.",
                collectionId);
            return await resolveKey(_cryptoProvider);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Could not generate the secret keys for collection {CollectionId}.",
                collectionId);
            return null;
        }
    }

    private string BuildEncryptionKeyName(Guid collectionId)
        => $"K{_config.Kms.KeyEnvironmentPrefix}VotingECollecting_{collectionId}_Encryption12";

    private string BuildMacKeyName(Guid collectionId)
        => $"K{_config.Kms.KeyEnvironmentPrefix}VotingECollecting_{collectionId}_DuplicateDetection128";
}
