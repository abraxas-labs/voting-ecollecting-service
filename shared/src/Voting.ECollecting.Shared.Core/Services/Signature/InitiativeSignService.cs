// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Shared.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Signature;
using Voting.ECollecting.Shared.Core.Exceptions;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Queries;
using Voting.Lib.Database.Postgres.Locking;

namespace Voting.ECollecting.Shared.Core.Services.Signature;

public class InitiativeSignService : ISignService<InitiativeEntity>
{
    private readonly ICollectionCitizenLogRepository _logRepository;
    private readonly IInitiativeRepository _initiativeRepository;
    private readonly ICollectionCryptoService _cryptoService;

    public InitiativeSignService(ICollectionCitizenLogRepository logRepository, IInitiativeRepository initiativeRepository, ICollectionCryptoService cryptoService)
    {
        _logRepository = logRepository;
        _initiativeRepository = initiativeRepository;
        _cryptoService = cryptoService;
    }

    public async Task LockAndEnsureCanSign(
        InitiativeEntity collection,
        IVotingStimmregisterPersonInfo personInfo,
        byte[] personCollectionMac)
    {
        await _initiativeRepository.Query()
            .ForUpdate()
            .Where(x => x.Id == collection.Id)
            .Select(_ => 1)
            .FirstAsync();

        if (await IsSigned(collection.Id, personCollectionMac))
        {
            throw new CollectionAlreadySignedException();
        }
    }

    public async Task LockAndEnsureCanSign(
        InitiativeEntity collection,
        IReadOnlySet<Guid> personRegisterIds,
        IReadOnlyList<byte[]> personCollectionMacs)
    {
        await _initiativeRepository.Query()
            .ForUpdate()
            .Where(x => x.Id == collection.Id)
            .Select(_ => 1)
            .FirstAsync();

        if (await IsAnySigned(collection.Id, personCollectionMacs))
        {
            throw new CollectionAlreadySignedException();
        }
    }

    public async Task<bool> IsCollectionSigned(InitiativeEntity collection, IVotingStimmregisterPersonInfo personInfo)
    {
        var mac = await _cryptoService.StimmregisterIdHmac(collection, personInfo.RegisterId);
        return await IsSigned(collection.Id, mac);
    }

    private async Task<bool> IsSigned(Guid collectionId, byte[] personCollectionMac)
    {
        return await _logRepository
            .Query()
            .WhereIsSigned(collectionId, personCollectionMac)
            .AnyAsync();
    }

    private async Task<bool> IsAnySigned(Guid collectionId, IReadOnlyList<byte[]> personCollectionMacs)
    {
        return await _logRepository
            .Query()
            .WhereIsSigned(collectionId, personCollectionMacs)
            .AnyAsync();
    }
}
