// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Citizen.Abstractions.Adapter.VotingStimmregister;
using Voting.ECollecting.Citizen.Abstractions.Core.Services.Signature;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Signature;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.ECollecting.Shared.Domain.Queries;
using IPermissionService = Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin.IPermissionService;

namespace Voting.ECollecting.Citizen.Core.Services.Signature;

public class InitiativeSignService : CollectionSignBaseService<InitiativeEntity>, IInitiativeSignService
{
    private readonly IDataContext _dataContext;
    private readonly IInitiativeRepository _initiativeRepository;
    private readonly ISignService<InitiativeEntity> _signService;

    public InitiativeSignService(
        IPermissionService permissionService,
        IVotingStimmregisterAdapter stimmregister,
        ICollectionCryptoService cryptoService,
        ICollectionCitizenRepository citizenRepository,
        TimeProvider timeProvider,
        ICollectionCountRepository collectionCountRepository,
        IDataContext dataContext,
        IInitiativeRepository initiativeRepository,
        ISignService<InitiativeEntity> signService,
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
        _initiativeRepository = initiativeRepository;
        _signService = signService;
    }

    public async Task Sign(Guid id)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var initiative = await _initiativeRepository.Query()
                             .WhereInPeriodState(CollectionPeriodState.InCollection, true, TimeProvider.GetUtcNowDateTime())
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), id);

        await Sign(initiative);
        await transaction.CommitAsync();
    }

    internal override Task<bool> IsCollectionSigned(InitiativeEntity initiative, IVotingStimmregisterPersonInfo personInfo)
        => _signService.IsCollectionSigned(initiative, personInfo);

    protected override Task LockAndEnsureCanSign(InitiativeEntity collection, IVotingStimmregisterPersonInfo personInfo, byte[] mac)
        => _signService.LockAndEnsureCanSign(collection, personInfo, mac);
}
