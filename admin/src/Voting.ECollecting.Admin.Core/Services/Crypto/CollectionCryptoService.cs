// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.Extensions.Logging;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Admin.Domain.Constants;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Entities.Audit;
using Voting.Lib.Cryptography;
using Voting.Lib.Cryptography.Kms.Exceptions;
using IPermissionService = Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam.IPermissionService;
using IVotingStimmregisterPersonInfo = Voting.ECollecting.Shared.Domain.Entities.IVotingStimmregisterPersonInfo;

namespace Voting.ECollecting.Admin.Core.Services.Crypto;

public class CollectionCryptoService
{
    private readonly ICryptoProvider _cryptoProvider;
    private readonly ILogger<CollectionCryptoService> _logger;
    private readonly CoreAppConfig _config;
    private readonly ICollectionCryptoService _coreCollectionCryptoService;
    private readonly IDataContext _dataContext;
    private readonly IPermissionService _permissionService;

    public CollectionCryptoService(
        ICryptoProvider cryptoProvider,
        ILogger<CollectionCryptoService> logger,
        CoreAppConfig config,
        ICollectionCryptoService coreCollectionCryptoService,
        IDataContext dataContext,
        IPermissionService permissionService)
    {
        _cryptoProvider = cryptoProvider;
        _logger = logger;
        _config = config;
        _coreCollectionCryptoService = coreCollectionCryptoService;
        _dataContext = dataContext;
        _permissionService = permissionService;
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

    internal async Task<EncryptStimmregisterIdResult> EncryptStimmregisterId(CollectionBaseEntity collection, IVotingStimmregisterPersonInfo stimmregisterInfo)
    {
        var idBytes = CollectionCryptoIdSerializer.SerializeStimmregisterId(stimmregisterInfo.RegisterId);
        var macTask = _coreCollectionCryptoService.StimmregisterIdHmac(collection, stimmregisterInfo.RegisterId);
        var encryptedTask = _cryptoProvider.EncryptAesGcm(idBytes, GetEncryptionKeyId(collection));
        await Task.WhenAll(macTask, encryptedTask);
        return new EncryptStimmregisterIdResult(encryptedTask.Result, macTask.Result);
    }

    internal async Task<IReadOnlyList<EncryptStimmregisterIdResult>> EncryptStimmregisterIds(CollectionBaseEntity collection, IReadOnlySet<Guid> personRegisterIds)
    {
        var idBytesList = personRegisterIds.Select(CollectionCryptoIdSerializer.SerializeStimmregisterId).ToList();
        var macTask = _coreCollectionCryptoService.StimmregisterIdHmacs(collection, personRegisterIds);
        var encryptedTask = _cryptoProvider.BulkEncryptAesGcm(idBytesList, GetEncryptionKeyId(collection));
        await Task.WhenAll(macTask, encryptedTask);
        return macTask.Result.Zip(encryptedTask.Result, (mac, encrypted) => new EncryptStimmregisterIdResult(encrypted, mac)).ToList();
    }

    internal async Task<IReadOnlyList<Guid>> DecryptStimmregisterIds(CollectionBaseEntity collection, IEnumerable<CollectionCitizenLogEntity> citizenLogEntities)
    {
        var decryptedIds = new List<Guid>();
        var auditEntries = new List<CollectionCitizenLogAuditTrailEntryEntity>();
        foreach (var citizenLogEntity in citizenLogEntities)
        {
            var decryptedId = await _cryptoProvider.DecryptAesGcm(citizenLogEntity.VotingStimmregisterIdEncrypted, GetEncryptionKeyId(collection));
            decryptedIds.Add(CollectionCryptoIdSerializer.DeserializeStimmregisterId(decryptedId));
            var auditEntry = new CollectionCitizenLogAuditTrailEntryEntity
            {
                CollectionId = collection.Id,
                Action = AuditTrailAction.Decryption,
                SourceEntityId = citizenLogEntity.Id,
            };
            _permissionService.SetCreated(auditEntry);
            auditEntries.Add(auditEntry);
        }

        _dataContext.CollectionCitizenLogAuditTrailEntries.AddRange(auditEntries);
        await _dataContext.SaveChangesAsync();

        return decryptedIds;
    }

    private static string GetEncryptionKeyId(CollectionBaseEntity collection)
        => collection.EncryptionKeyId ?? throw new InvalidOperationException($"EncryptionKeyId is not set on collection with id {collection.Id} in state {collection.State}");

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
