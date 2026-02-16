// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Shared.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Signature;
using Voting.ECollecting.Shared.Core.Exceptions;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.ECollecting.Shared.Domain.Queries;
using Voting.Lib.Database.Postgres.Locking;

namespace Voting.ECollecting.Shared.Core.Services.Signature;

public class ReferendumSignService : IReferendumSignService
{
    private readonly ICollectionCitizenLogRepository _logRepository;
    private readonly IReferendumRepository _referendumRepository;
    private readonly ICollectionCryptoService _cryptoService;

    public ReferendumSignService(
        ICollectionCitizenLogRepository logRepository,
        IReferendumRepository referendumRepository,
        ICollectionCryptoService cryptoService)
    {
        _logRepository = logRepository;
        _referendumRepository = referendumRepository;
        _cryptoService = cryptoService;
    }

    public async Task LockAndEnsureCanSign(
        ReferendumEntity collection,
        IVotingStimmregisterPersonInfo personInfo,
        byte[] personCollectionMac)
    {
        // lock all referendums of the decree to enforce unique signatures and max electronic signature count across all collections
        await _referendumRepository.Query()
            .Where(x => x.DecreeId == collection.DecreeId)
            .ForUpdate()
            .Select(_ => 1) // do not materialize
            .ToListAsync();

        if (await IsSigned(collection.Id, personCollectionMac))
        {
            throw new CollectionAlreadySignedException();
        }

        if (await IsOtherReferendumOfDecreeSigned(collection, personInfo))
        {
            throw new DecreeAlreadySignedException();
        }
    }

    public async Task LockAndEnsureCanSign(
        ReferendumEntity collection,
        IReadOnlySet<Guid> personRegisterIds,
        IReadOnlyList<byte[]> personCollectionMacs)
    {
        // lock all referendums of the decree to enforce unique signatures and max electronic signature count across all collections
        await _referendumRepository.Query()
            .Where(x => x.DecreeId == collection.DecreeId)
            .ForUpdate()
            .Select(_ => 1) // do not materialize
            .ToListAsync();

        if (await IsAnySigned(collection.Id, personCollectionMacs))
        {
            throw new CollectionAlreadySignedException();
        }

        if (await IsOtherReferendumOfDecreeAnySigned(collection, personRegisterIds))
        {
            throw new DecreeAlreadySignedException();
        }
    }

    public async Task<(bool IsCollectionSigned, bool IsDecreeSigned, CollectionSignatureType? SignatureType)> IsReferendumOrDecreeSigned(
        ReferendumEntity referendum,
        IVotingStimmregisterPersonInfo personInfo)
    {
        var (isSigned, signatureType) = await IsCollectionSigned(referendum, personInfo);
        if (isSigned)
        {
            return (true, true, signatureType);
        }

        return (false, await IsOtherReferendumOfDecreeSigned(referendum, personInfo), null);
    }

    public async Task<(bool IsSigned, CollectionSignatureType? SignatureType)> IsCollectionSigned(ReferendumEntity referendum, IVotingStimmregisterPersonInfo personInfo)
    {
        var registerIdMac = await _cryptoService.StimmregisterIdHmac(referendum, personInfo.RegisterId);
        return await IsSignedWithSignatureType(referendum.Id, registerIdMac);
    }

    private async Task<(bool IsSigned, CollectionSignatureType? SignatureType)> IsSignedWithSignatureType(Guid collectionId, byte[] personCollectionMac)
    {
        var citizen = await _logRepository
            .Query()
            .WhereIsSigned(collectionId, personCollectionMac)
            .Select(x => x.CollectionCitizen)
            .FirstOrDefaultAsync();

        if (citizen == null)
        {
            return (false, null);
        }

        return (true, citizen.SignatureSheetId.HasValue ? CollectionSignatureType.Physical : CollectionSignatureType.Electronic);
    }

    private async Task<bool> IsSigned(Guid referendumId, byte[] personCollectionMac)
    {
        return await _logRepository
            .Query()
            .WhereIsSigned(referendumId, personCollectionMac)
            .AnyAsync();
    }

    private async Task<bool> IsAnySigned(Guid referendumId, IReadOnlyList<byte[]> personCollectionMacs)
    {
        return await _logRepository
            .Query()
            .WhereIsSigned(referendumId, personCollectionMacs)
            .AnyAsync();
    }

    private async Task<bool> IsOtherReferendumOfDecreeSigned(
        ReferendumEntity referendum,
        IVotingStimmregisterPersonInfo personInfo)
    {
        Debug.Assert(referendum.Decree != null, "Decree must be loaded");
        Debug.Assert(referendum.Decree.Collections.Count > 0, "Collections of Decree must be loaded");
        foreach (var otherReferendum in referendum.Decree.Collections)
        {
            if (otherReferendum.Id == referendum.Id)
            {
                continue;
            }

            var registerIdMac = await _cryptoService.StimmregisterIdHmac(otherReferendum, personInfo.RegisterId);
            if (await IsSigned(otherReferendum.Id, registerIdMac))
            {
                return true;
            }
        }

        return false;
    }

    private async Task<bool> IsOtherReferendumOfDecreeAnySigned(
        ReferendumEntity referendum,
        IReadOnlySet<Guid> personRegisterIds)
    {
        Debug.Assert(referendum.Decree != null, "Decree must be loaded");
        Debug.Assert(referendum.Decree.Collections.Count > 0, "Collections of Decree must be loaded");
        foreach (var otherReferendum in referendum.Decree.Collections)
        {
            if (otherReferendum.Id == referendum.Id)
            {
                continue;
            }

            var macs = await _cryptoService.StimmregisterIdHmacs(otherReferendum, personRegisterIds);
            if (await IsAnySigned(otherReferendum.Id, macs))
            {
                return true;
            }
        }

        return false;
    }
}
