// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Queries;

namespace Voting.ECollecting.Admin.Core.Services.Signature;

public class ReferendumSignService
{
    private readonly ICollectionCryptoService _cryptoService;
    private readonly ICollectionCitizenLogRepository _citizenLogRepository;

    public ReferendumSignService(ICollectionCryptoService cryptoService, ICollectionCitizenLogRepository citizenLogRepository)
    {
        _cryptoService = cryptoService;
        _citizenLogRepository = citizenLogRepository;
    }

    internal async Task<CollectionCitizenEntity?> TryGetSignature(
        ReferendumEntity referendum,
        IVotingStimmregisterPersonInfo personInfo,
        CancellationToken cancellationToken)
    {
        var referendumMac = await _cryptoService.StimmregisterIdHmac(referendum, personInfo);
        var collectionSignature = await _citizenLogRepository.Query()
            .Include(x => x.CollectionCitizen!.CollectionMunicipality!.Collection)
            .Include(x => x.CollectionCitizen!.SignatureSheet)
            .WhereIsSigned(referendum.Id, referendumMac)
            .Select(x => x.CollectionCitizen)
            .FirstOrDefaultAsync(cancellationToken);
        if (collectionSignature != null)
        {
            return collectionSignature;
        }

        Debug.Assert(referendum.Decree != null, "Decree and collections need to be loaded.");
        foreach (var otherReferendum in referendum.Decree!.Collections)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (otherReferendum.Id == referendum.Id)
            {
                continue;
            }

            var otherReferendumMac = await _cryptoService.StimmregisterIdHmac(otherReferendum, personInfo);
            collectionSignature = await _citizenLogRepository.Query()
                .Include(x => x.CollectionCitizen!.CollectionMunicipality!.Collection)
                .Include(x => x.CollectionCitizen!.SignatureSheet)
                .WhereIsSigned(otherReferendum.Id, otherReferendumMac)
                .Select(x => x.CollectionCitizen)
                .FirstOrDefaultAsync(cancellationToken);
            if (collectionSignature != null)
            {
                return collectionSignature;
            }
        }

        return null;
    }
}
