// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Citizen.Abstractions.Adapter.VotingStimmregister;
using Voting.ECollecting.Citizen.Abstractions.Core.Services.Signature;
using Voting.ECollecting.Citizen.Core.Exceptions;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.ECollecting.Shared.Domain.Queries;
using IPermissionService = Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin.IPermissionService;
using SharedIReferendumSignService = Voting.ECollecting.Shared.Abstractions.Core.Services.Signature.IReferendumSignService;

namespace Voting.ECollecting.Citizen.Core.Services.Signature;

public class ReferendumSignService : CollectionSignBaseService<ReferendumEntity>, IReferendumSignService
{
    private readonly IDataContext _dataContext;
    private readonly IReferendumRepository _referendumRepository;
    private readonly SharedIReferendumSignService _signService;

    public ReferendumSignService(
        IPermissionService permissionService,
        IVotingStimmregisterAdapter stimmregister,
        ICollectionCryptoService cryptoService,
        ICollectionCitizenRepository citizenRepository,
        TimeProvider timeProvider,
        ICollectionCountRepository collectionCountRepository,
        IDataContext dataContext,
        IReferendumRepository referendumRepository,
        SharedIReferendumSignService signService,
        PersonInfoResolver personInfoResolver,
        ICollectionMunicipalityRepository collectionMunicipalityRepository)
        : base(
            permissionService,
            stimmregister,
            cryptoService,
            citizenRepository,
            timeProvider,
            collectionCountRepository,
            personInfoResolver,
            collectionMunicipalityRepository,
            dataContext)
    {
        _dataContext = dataContext;
        _referendumRepository = referendumRepository;
        _signService = signService;
    }

    public async Task Sign(Guid id)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var referendum = await _referendumRepository.Query()
                             .AsNoTrackingWithIdentityResolution()

                             // load other collections to check existing signature on same decree
                             .Include(x => x.Decree!.Collections.Where(y => !string.IsNullOrWhiteSpace(y.EncryptionKeyId) && !string.IsNullOrWhiteSpace(y.MacKeyId)))
                             .WhereInPeriodState(CollectionPeriodState.InCollection, true, TimeProvider.GetUtcTodayDateOnly())
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(ReferendumEntity), id);

        await Sign(referendum);
        await transaction.CommitAsync();
    }

    internal override Task<(bool IsSigned, CollectionSignatureType? SignatureType)> IsCollectionSigned(ReferendumEntity referendum, IVotingStimmregisterPersonInfo personInfo)
        => _signService.IsCollectionSigned(referendum, personInfo);

    internal async Task<(bool? IsSigned, bool? IsDecreeSigned, CollectionSignatureType? SignatureType)> IsReferendumOrDecreeSigned(ReferendumEntity referendum)
    {
        var personInfo = await GetPersonInfo(referendum, true);
        if (personInfo == null)
        {
            return (false, false, null);
        }

        return await _signService.IsReferendumOrDecreeSigned(referendum, personInfo);
    }

    protected override async Task LockAndEnsureCanSign(
        ReferendumEntity collection,
        IVotingStimmregisterPersonInfo personInfo,
        byte[] mac)
    {
        await _signService.LockAndEnsureCanSign(collection, personInfo, mac);

        var electronicCitizenCount = await _referendumRepository.Query()
            .Where(x => x.Id == collection.Id)
            .SelectMany(x => x.Decree!.Collections)
            .SumAsync(x => x.CollectionCount!.ElectronicCitizenCount);

        if (collection.Decree!.MaxElectronicSignatureCount <= electronicCitizenCount)
        {
            throw new DecreeMaxElectronicSignatureCountReachedException();
        }
    }
}
