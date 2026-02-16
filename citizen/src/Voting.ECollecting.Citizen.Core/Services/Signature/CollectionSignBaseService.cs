// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Citizen.Abstractions.Adapter.VotingStimmregister;
using Voting.ECollecting.Citizen.Core.Exceptions;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.Lib.Database.Postgres.Locking;
using IPermissionService = Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin.IPermissionService;

namespace Voting.ECollecting.Citizen.Core.Services.Signature;

public abstract class CollectionSignBaseService<TEntity>
    where TEntity : CollectionBaseEntity
{
    private readonly ICollectionCitizenRepository _citizenRepository;
    private readonly ICollectionCountRepository _collectionCountRepository;
    private readonly IPermissionService _permissionService;
    private readonly IVotingStimmregisterAdapter _stimmregister;
    private readonly PersonInfoResolver _personInfoResolver;
    private readonly ICollectionMunicipalityRepository _collectionMunicipalityRepository;
    private readonly IDataContext _dataContext;
    private readonly ICollectionCryptoService _cryptoService;

    protected CollectionSignBaseService(
        IPermissionService permissionService,
        IVotingStimmregisterAdapter stimmregister,
        ICollectionCryptoService cryptoService,
        ICollectionCitizenRepository citizenRepository,
        TimeProvider timeProvider,
        ICollectionCountRepository collectionCountRepository,
        PersonInfoResolver personInfoResolver,
        ICollectionMunicipalityRepository collectionMunicipalityRepository,
        IDataContext dataContext)
    {
        _permissionService = permissionService;
        _stimmregister = stimmregister;
        _cryptoService = cryptoService;
        _citizenRepository = citizenRepository;
        TimeProvider = timeProvider;
        _collectionCountRepository = collectionCountRepository;
        _personInfoResolver = personInfoResolver;
        _collectionMunicipalityRepository = collectionMunicipalityRepository;
        _dataContext = dataContext;
    }

    protected TimeProvider TimeProvider { get; }

    internal async Task<(bool IsSigned, CollectionSignatureType? SignatureType)> IsCollectionSigned(TEntity collection)
    {
        var personInfo = await GetPersonInfo(collection);
        if (personInfo == null)
        {
            return (false, null);
        }

        return await IsCollectionSigned(collection, personInfo);
    }

    internal abstract Task<(bool IsSigned, CollectionSignatureType? SignatureType)> IsCollectionSigned(TEntity collection, IVotingStimmregisterPersonInfo personInfo);

    protected async Task Sign(TEntity collection)
    {
        var userSocialSecurityNumber = await _permissionService.GetSocialSecurityNumber();
        if (userSocialSecurityNumber == null)
        {
            // this should never happen as the ACR cannot be fulfilled without a SSN
            throw new ValidationException("Social security number not set");
        }

        if (!collection.DomainOfInfluenceType.HasValue || string.IsNullOrWhiteSpace(collection.Bfs))
        {
            // this should never happen as the referendum should always have a decree assigned
            throw new ValidationException("Domain of influence type or bfs is not set");
        }

        var personInfo = await _stimmregister.GetPersonInfo(
            userSocialSecurityNumber,
            collection.DomainOfInfluenceType.Value,
            collection.Bfs);
        var stimmregisterIdMac = await _cryptoService.StimmregisterIdHmac(collection, personInfo.RegisterId);

        var collectionCount = await _collectionCountRepository.Query()
            .ForUpdate() // serialize access to enforce max electronic signature count and unique signatures
            .SingleAsync(x => x.CollectionId == collection.Id);
        await LockAndEnsureCanSign(collection, personInfo, stimmregisterIdMac);

        if (collection.MaxElectronicSignatureCount <= collectionCount.ElectronicCitizenCount)
        {
            throw new CollectionMaxElectronicSignatureCountReachedException();
        }

        var collectionMunicipality = await _collectionMunicipalityRepository.Query()
            .AsTracking()
            .Where(x => x.CollectionId == collection.Id && x.Bfs == personInfo.MunicipalityId.ToString())
            .ForUpdate()
            .SingleAsync();

        collectionMunicipality.ElectronicCitizenCount++;
        await _dataContext.SaveChangesAsync();

        var entity = BuildCollectionCitizenEntity(collection, personInfo, stimmregisterIdMac, collectionMunicipality.Id);
        await _citizenRepository.Create(entity);

        await _collectionCountRepository.AuditedUpdateRange(
            q => q.Where(x => x.CollectionId == collection.Id),
            x =>
            {
                x.ElectronicCitizenCount++;
                x.TotalCitizenCount++;
            });
    }

    protected abstract Task LockAndEnsureCanSign(
        TEntity collection,
        IVotingStimmregisterPersonInfo personInfo,
        byte[] mac);

    protected async Task<IVotingStimmregisterPersonInfo?> GetPersonInfo(TEntity collection)
    {
        if (collection.DomainOfInfluenceType == null || string.IsNullOrEmpty(collection.Bfs))
        {
            return null;
        }

        return await _personInfoResolver.GetPersonInfo(collection.DomainOfInfluenceType.Value, collection.Bfs);
    }

    private CollectionCitizenEntity BuildCollectionCitizenEntity(
        TEntity collection,
        IVotingStimmregisterPersonInfo personInfo,
        byte[] stimmregisterIdMac,
        Guid collectionMunicipalityId)
    {
        var entity = new CollectionCitizenEntity
        {
            CollectionMunicipalityId = collectionMunicipalityId,
            Age = personInfo.Age,
            CollectionDateTime = TimeProvider.GetUtcNowDateTime(),
            Sex = personInfo.Sex,
            Log = new CollectionCitizenLogEntity
            {
                CollectionId = collection.Id,

                // The encrypted Stimmregister ID does not need to be set, since this is only required for physical signatures.
                VotingStimmregisterIdMac = stimmregisterIdMac,
            },
        };

        _permissionService.SetCreatedWithoutPII(entity);
        _permissionService.SetCreatedWithoutPII(entity.Log);
        return entity;
    }
}
