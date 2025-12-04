// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Queries;

namespace Voting.ECollecting.Admin.Core.Services.Signature;

public class InitiativeSignService
{
    private readonly ICollectionCryptoService _cryptoService;
    private readonly ICollectionCitizenLogRepository _citizenLogRepository;

    public InitiativeSignService(ICollectionCryptoService cryptoService, ICollectionCitizenLogRepository citizenLogRepository)
    {
        _cryptoService = cryptoService;
        _citizenLogRepository = citizenLogRepository;
    }

    internal async Task<CollectionCitizenEntity?> TryGetSignature(
        InitiativeEntity initiative,
        IVotingStimmregisterPersonInfo personInfo,
        CancellationToken cancellationToken)
    {
        var mac = await _cryptoService.StimmregisterIdHmac(initiative, personInfo);
        return await _citizenLogRepository.Query()
            .Include(x => x.CollectionCitizen!.CollectionMunicipality!.Collection)
            .Include(x => x.CollectionCitizen!.SignatureSheet)
            .WhereIsSigned(initiative.Id, mac)
            .Select(x => x.CollectionCitizen)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
