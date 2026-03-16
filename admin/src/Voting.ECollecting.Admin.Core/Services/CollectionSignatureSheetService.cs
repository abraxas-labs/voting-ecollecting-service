// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingStimmregister;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Admin.Core.Exceptions;
using Voting.ECollecting.Admin.Core.Mappings;
using Voting.ECollecting.Admin.Core.Permissions;
using Voting.ECollecting.Admin.Core.Services.Signature;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Admin.Domain.Queries;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.Lib.Common.Files;
using Voting.Lib.Database.Models;
using Voting.Lib.Database.Postgres.Locking;
using IPermissionService = Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam.IPermissionService;
using IVotingStimmregisterPersonInfo = Voting.ECollecting.Admin.Domain.Models.IVotingStimmregisterPersonInfo;

namespace Voting.ECollecting.Admin.Core.Services;

public class CollectionSignatureSheetService : ICollectionSignatureSheetService
{
    private const string NumberUniqueConstraintName = "IX_CollectionSignatureSheets_CollectionMunicipalityId_Number";

    private static readonly Pageable _allPage = new(1, 1_000);
    private readonly ICollectionRepository _collectionRepository;
    private readonly ICollectionSignatureSheetRepository _signatureSheetRepository;
    private readonly IDataContext _dataContext;
    private readonly IPermissionService _permissionService;
    private readonly IDomainOfInfluenceRepository _domainOfInfluenceRepository;
    private readonly ISignatureSheetAttestationGenerationService _signatureSheetAttestationGenerationService;
    private readonly TimeProvider _timeProvider;
    private readonly IVotingStimmregisterAdapter _stimmregister;
    private readonly CollectionSignService _signService;
    private readonly IReferendumRepository _referendumRepository;
    private readonly IInitiativeRepository _initiativeRepository;
    private readonly ICollectionMunicipalityRepository _collectionMunicipalityRepository;
    private readonly ICollectionCitizenRepository _citizenRepository;
    private readonly ICollectionCryptoService _cryptoService;
    private readonly ICollectionCountRepository _collectionCountRepository;

    public CollectionSignatureSheetService(
        ICollectionSignatureSheetRepository signatureSheetRepository,
        ICollectionRepository collectionRepository,
        IPermissionService permissionService,
        IDataContext dataContext,
        ISignatureSheetAttestationGenerationService signatureSheetAttestationGenerationService,
        TimeProvider timeProvider,
        IVotingStimmregisterAdapter stimmregister,
        CollectionSignService signService,
        IReferendumRepository referendumRepository,
        IInitiativeRepository initiativeRepository,
        ICollectionMunicipalityRepository collectionMunicipalityRepository,
        ICollectionCitizenRepository citizenRepository,
        ICollectionCryptoService cryptoService,
        ICollectionCountRepository collectionCountRepository,
        IDomainOfInfluenceRepository domainOfInfluenceRepository)
    {
        _signatureSheetRepository = signatureSheetRepository;
        _collectionRepository = collectionRepository;
        _permissionService = permissionService;
        _dataContext = dataContext;
        _signatureSheetAttestationGenerationService = signatureSheetAttestationGenerationService;
        _timeProvider = timeProvider;
        _stimmregister = stimmregister;
        _signService = signService;
        _referendumRepository = referendumRepository;
        _initiativeRepository = initiativeRepository;
        _collectionMunicipalityRepository = collectionMunicipalityRepository;
        _citizenRepository = citizenRepository;
        _cryptoService = cryptoService;
        _collectionCountRepository = collectionCountRepository;
        _domainOfInfluenceRepository = domainOfInfluenceRepository;
    }

    public async Task<Page<CollectionSignatureSheet>> List(
        Guid collectionId,
        IReadOnlySet<DateTime>? attestedAt,
        IReadOnlyCollection<CollectionSignatureSheetState> states,
        Pageable? pageable,
        CollectionSignatureSheetSort sort = CollectionSignatureSheetSort.Number,
        SortDirection sortDirection = SortDirection.Ascending)
    {
        await EnsureCanReadSignatureSheets(collectionId);

        var query = _signatureSheetRepository
            .Query()
            .Include(x => x.CollectionMunicipality)
            .WhereCanRead(_permissionService)
            .Where(x => x.CollectionMunicipality!.CollectionId == collectionId && states.Contains(x.State));

        if (attestedAt?.Count > 0)
        {
            query = query.Where(x => x.AttestedAt.HasValue && attestedAt.Contains(x.AttestedAt.Value));
        }

        var page = await query
            .OrderBy(sort, sortDirection)
            .ToPageAsync(pageable ?? _allPage);

        if (pageable == null && page.HasNextPage)
        {
            throw new ValidationException("Tried to fetch a large dataset without paging");
        }

        var models = page.Items.ConvertAll(entity =>
        {
            var model = Mapper.MapToCollectionSignatureSheet(entity);
            model.UserPermissions = CollectionSignatureSheetPermissions.Build(_permissionService, entity);
            return model;
        });
        return new Page<CollectionSignatureSheet>(models, page.TotalItemsCount, page.CurrentPage, page.PageSize);
    }

    public async Task<IReadOnlyCollection<DateTime>> ListAttestedAt(Guid collectionId)
    {
        await EnsureCanReadSignatureSheets(collectionId);
        return await _signatureSheetRepository
            .Query()
            .WhereCanRead(_permissionService)
            .Where(x => x.AttestedAt.HasValue)
            .Select(x => x.AttestedAt!.Value)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();
    }

    public async Task Delete(Guid collectionId, Guid sheetId)
    {
        await EnsureCanEditSignatureSheets(collectionId);

        var existing = await _signatureSheetRepository.Query()
            .WhereCanDelete(_permissionService)
            .FirstOrDefaultAsync(x => x.CollectionMunicipality!.CollectionId == collectionId && x.Id == sheetId)
            ?? throw new EntityNotFoundException(nameof(CollectionSignatureSheetEntity), sheetId);

        await _signatureSheetRepository.AuditedDelete(existing);
    }

    public async Task<CollectionSignatureSheetNumberInfo> ReserveNumber(Guid collectionId)
    {
        await EnsureCanEditSignatureSheets(collectionId);
        var doi = await _domainOfInfluenceRepository.GetSingleByType(_permissionService.AclBfsLists, DomainOfInfluenceType.Mu);

        await using var transaction = await _dataContext.BeginTransaction();
        var collectionMunicipality = await _collectionMunicipalityRepository.Query()
            .AsTracking()
            .ForUpdate()
            .FirstAsync(x => x.Bfs == doi.Bfs && x.CollectionId == collectionId);
        collectionMunicipality.NextSheetNumber++;
        await _dataContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return new CollectionSignatureSheetNumberInfo(doi.Bfs!, doi.Name, collectionMunicipality.NextSheetNumber - 1);
    }

    public async Task TryReleaseNumber(Guid collectionId, int number)
    {
        await EnsureCanEditSignatureSheets(collectionId);
        var bfs = await _domainOfInfluenceRepository.GetSingleBfsByType(_permissionService.AclBfsLists, DomainOfInfluenceType.Mu);

        await using var transaction = await _dataContext.BeginTransaction();

        var inUse = await _signatureSheetRepository.Query()
            .AnyAsync(x => x.CollectionMunicipality!.CollectionId == collectionId && x.CollectionMunicipality.Bfs == bfs && x.Number == number);
        if (inUse)
        {
            throw new ValidationException("The number is already in use");
        }

        // only the most recent reserved number can be released again
        // and only if it is not used.
        await _collectionMunicipalityRepository.AuditedUpdateRange(
            q => q.Where(x => x.Bfs == bfs && x.CollectionId == collectionId && x.NextSheetNumber == number + 1),
            x => --x.NextSheetNumber);

        await transaction.CommitAsync();
    }

    public async Task<CollectionSignatureSheetEntity> Add(Guid collectionId, int number, DateOnly receivedAt, int signatureCountTotal)
    {
        await EnsureCanEditSignatureSheets(collectionId);

        if (receivedAt > _timeProvider.GetUtcTodayDateOnly())
        {
            throw new ValidationException("Received at date can't be in the future.");
        }

        var bfs = await _domainOfInfluenceRepository.GetSingleBfsByType(_permissionService.AclBfsLists, DomainOfInfluenceType.Mu);
        var collectionMunicipality = await _collectionMunicipalityRepository.Query().FirstAsync(x => x.Bfs == bfs && x.CollectionId == collectionId);
        await using var transaction = await _dataContext.BeginTransaction();

        var maxNumber = collectionMunicipality.NextSheetNumber - 1;
        if (maxNumber < number)
        {
            throw new ValidationException("Cannot use a number higher than the current counter");
        }

        var newEntity = new CollectionSignatureSheetEntity
        {
            CollectionMunicipalityId = collectionMunicipality.Id,
            Count = new CollectionSignatureSheetCount
            {
                // no valid signatures yet, so invalid = total
                Invalid = signatureCountTotal,
            },
            State = CollectionSignatureSheetState.Created,
            Number = number,
            ReceivedAt = receivedAt,
        };
        _permissionService.SetCreated(newEntity);

        try
        {
            await _signatureSheetRepository.Create(newEntity);
        }
        catch (Exception e) when (e.InnerException is PostgresException { ConstraintName: NumberUniqueConstraintName })
        {
            throw new ValidationException("The number is already in use");
        }

        await transaction.CommitAsync();
        return newEntity;
    }

    public async Task Update(Guid collectionId, Guid sheetId, DateOnly receivedAt, int signatureCountTotal)
    {
        await EnsureCanEditSignatureSheets(collectionId);

        if (receivedAt > _timeProvider.GetUtcTodayDateOnly())
        {
            throw new ValidationException("Received at date can't be in the future.");
        }

        var sheet = await _signatureSheetRepository
            .Query()
            .AsTracking()
            .WhereCanEdit(_permissionService)
            .FirstOrDefaultAsync(x => x.Id == sheetId && x.CollectionMunicipality!.CollectionId == collectionId)
            ?? throw new EntityNotFoundException(nameof(CollectionSignatureSheetEntity), sheetId);

        if (signatureCountTotal < sheet.Count.Valid)
        {
            throw new ValidationException("Cannot set total count lower than valid count");
        }

        _permissionService.SetModified(sheet);
        sheet.ReceivedAt = receivedAt;
        sheet.Count.Invalid = signatureCountTotal - sheet.Count.Valid;

        await _dataContext.SaveChangesAsync();
    }

    public async Task<IFile> Attest(Guid collectionId, IReadOnlySet<Guid> signatureSheetIds)
    {
        if (signatureSheetIds.Count == 0)
        {
            throw new ValidationException("No signature sheets were selected for attestation.");
        }

        var collection = await _collectionRepository
                             .Query()
                             .WhereCanEditSignatureSheets(_permissionService)
                             .FirstOrDefaultAsync(x => x.Id == collectionId) ??
                         throw new EntityNotFoundException(nameof(CollectionBaseEntity), collectionId);

        // logo is needed for pdf
        var doi = await _domainOfInfluenceRepository.GetSingleWithLogoContentsByType(_permissionService.AclBfsLists, DomainOfInfluenceType.Mu);

        await using var transaction = await _dataContext.BeginTransaction();

        var signatureSheets = await _signatureSheetRepository.Query()
            .WhereCanAttest(_permissionService)
            .Where(x =>
                x.CollectionMunicipality!.CollectionId == collectionId
                && x.CollectionMunicipality.Bfs == doi.Bfs
                && signatureSheetIds.Contains(x.Id)
                && x.State == CollectionSignatureSheetState.Created)
            .OrderBy(x => x.Number)
            .AsTracking()
            .ForUpdate()
            .ToListAsync();

        if (signatureSheets.Count != signatureSheetIds.Count)
        {
            var foundIds = signatureSheets.Select(x => x.Id).ToHashSet();
            var notFoundId = signatureSheetIds.Except(foundIds).First();
            throw new EntityNotFoundException(nameof(CollectionSignatureSheetEntity), notFoundId);
        }

        var collectionMunicipalityId = EnsureAllSheetsFoundAndOfSameMunicipality(signatureSheetIds, signatureSheets);
        await LockMunicipalityAndCollectionCount(collectionId, collectionMunicipalityId);

        var attestedAt = _timeProvider.GetUtcNowDateTime();

        var validCount = 0;
        var invalidCount = 0;

        foreach (var sheet in signatureSheets)
        {
            validCount += sheet.Count.Valid;
            invalidCount += sheet.Count.Invalid;
            sheet.State = CollectionSignatureSheetState.Attested;
            sheet.AttestedAt = attestedAt;
            _permissionService.SetModified(sheet);
        }

        await _dataContext.SaveChangesAsync();

        await UpdateMunicipalityAndCollectionCounts(collectionMunicipalityId, collectionId, validCount, invalidCount);
        await transaction.CommitAsync();

        return await _signatureSheetAttestationGenerationService.GenerateFile(
            collection,
            doi,
            signatureSheets);
    }

    public async Task<IFile> Reattest(Guid collectionId, IReadOnlySet<Guid> signatureSheetIds)
    {
        var collection = await _collectionRepository
                             .Query()
                             .WhereCanEditSignatureSheets(_permissionService)
                             .FirstOrDefaultAsync(x => x.Id == collectionId) ??
                         throw new EntityNotFoundException(nameof(CollectionBaseEntity), collectionId);

        // logo is needed for pdf
        var doi = await _domainOfInfluenceRepository.GetSingleWithLogoContentsByType(_permissionService.AclBfsLists, DomainOfInfluenceType.Mu);

        var signatureSheets = await _signatureSheetRepository.Query()
            .WhereCanAttest(_permissionService)
            .Where(x =>
                x.CollectionMunicipality!.CollectionId == collectionId
                && x.CollectionMunicipality.Bfs == doi.Bfs
                && signatureSheetIds.Contains(x.Id)
                && x.State != CollectionSignatureSheetState.Created)
            .OrderBy(x => x.Number)
            .ToListAsync();

        EnsureAllSheetsFoundAndOfSameMunicipality(signatureSheetIds, signatureSheets);

        return await _signatureSheetAttestationGenerationService.GenerateFile(
            collection,
            doi,
            signatureSheets);
    }

    public async Task<CollectionSignatureSheet> Get(Guid collectionId, Guid sheetId)
    {
        await EnsureCanReadSignatureSheetsOrCheckSamples(collectionId);
        var entity = await _signatureSheetRepository
            .Query()
            .Include(x => x.CollectionMunicipality)
            .FirstOrDefaultAsync(x => x.Id == sheetId && x.CollectionMunicipality!.CollectionId == collectionId)
            ?? throw new EntityNotFoundException(nameof(CollectionSignatureSheetEntity), sheetId);

        if (!CollectionSignatureSheetPermissions.CanCheckSamples(_permissionService, entity) &&
            !CollectionSignatureSheetPermissions.CanRead(_permissionService, entity))
        {
            throw new EntityNotFoundException(nameof(CollectionSignatureSheetEntity), sheetId);
        }

        var model = Mapper.MapToCollectionSignatureSheet(entity);
        model.UserPermissions = CollectionSignatureSheetPermissions.Build(_permissionService, entity);
        return model;
    }

    public async Task<Page<CollectionSignatureSheetCandidate>> SearchPersonCandidates(
        CollectionType collectionType,
        Guid collectionId,
        Guid sheetId,
        VotingStimmregisterPersonFilterData filter,
        Pageable? pageable = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(filter.FirstName)
            && string.IsNullOrEmpty(filter.OfficialName)
            && string.IsNullOrEmpty(filter.ResidenceAddressStreet)
            && string.IsNullOrEmpty(filter.ResidenceAddressHouseNumber))
        {
            throw new ValidationException($"At least one search criteria of {nameof(filter.FirstName)}, {nameof(filter.OfficialName)}, {nameof(filter.ResidenceAddressStreet)} and {nameof(filter.ResidenceAddressHouseNumber)} must be provided");
        }

        var collection = await BuildSignatureInfoQuery(collectionType)
            .IncludeMunicipalities(_permissionService.AclBfsLists)
            .FirstOrDefaultAsync(x => x.Id == collectionId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), new { collectionType, collectionId });
        collection.SetPeriodState(_timeProvider.GetUtcTodayDateOnly());

        if (!CollectionPermissions.CanEditSignatureSheets(_permissionService, collection) && !CollectionPermissions.CanCheckSamples(_permissionService, collection))
        {
            throw new EntityNotFoundException(nameof(CollectionBaseEntity), collectionId);
        }

        var sheet = await _signatureSheetRepository
                         .Query()
                         .Include(x => x.CollectionMunicipality)
                         .FirstOrDefaultAsync(x => x.Id == sheetId && x.CollectionMunicipality!.CollectionId == collectionId, cancellationToken)
                     ?? throw new EntityNotFoundException(nameof(CollectionSignatureSheetEntity), sheetId);

        if (!CollectionSignatureSheetPermissions.CanConfirm(_permissionService, sheet) &&
            !CollectionSignatureSheetPermissions.CanEdit(_permissionService, sheet))
        {
            throw new EntityNotFoundException(nameof(CollectionSignatureSheetEntity), sheetId);
        }

        filter = filter with
        {
            Bfs = sheet.CollectionMunicipality!.Bfs,
            ActualityDate = sheet.ReceivedAt.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
        };
        var people = await _stimmregister.ListPersonInfos(filter, pageable, cancellationToken);
        var candidates = people.Items.ConvertAll(x => new CollectionSignatureSheetCandidate(x));
        await _signService.LoadSignatureInfos(collection, candidates, cancellationToken);

        foreach (var candidate in candidates.Where(x => x.ExistingSignature != null))
        {
            candidate.ExistingSignatureIsInSameMunicipality = candidate.ExistingSignature!.CollectionMunicipality!.Bfs == sheet.CollectionMunicipality!.Bfs;
            candidate.ExistingSignatureIsOnSameSheet = candidate.ExistingSignatureIsInSameMunicipality && candidate.ExistingSignature!.SignatureSheetId == sheetId;
        }

        return new Page<CollectionSignatureSheetCandidate>(candidates, people.TotalItemsCount, people.CurrentPage, people.PageSize);
    }

    public async Task<IEnumerable<IVotingStimmregisterPersonInfo>> ListCitizens(Guid collectionId, Guid sheetId)
    {
        await EnsureCanReadSignatureSheetsOrCheckSamples(collectionId);
        var sheet = await _signatureSheetRepository.Query()
            .AsSplitQuery()
            .Include(x => x.CollectionMunicipality!.Collection)
            .Include(x => x.Citizens.Where(y => y.Log != null)).ThenInclude(x => x.Log)
            .FirstOrDefaultAsync(x => x.CollectionMunicipality!.CollectionId == collectionId && x.Id == sheetId)
            ?? throw new EntityNotFoundException(nameof(CollectionSignatureSheetEntity), sheetId);

        if (!CollectionSignatureSheetPermissions.CanCheckSamples(_permissionService, sheet) &&
            !CollectionSignatureSheetPermissions.CanRead(_permissionService, sheet))
        {
            throw new EntityNotFoundException(nameof(CollectionSignatureSheetEntity), sheetId);
        }

        var decryptionTasks = sheet.Citizens
            .Select(citizen => _cryptoService.DecryptStimmregisterId(sheet.CollectionMunicipality!.Collection!, citizen.Log!.VotingStimmregisterIdEncrypted));
        var decryptedIds = await Task.WhenAll(decryptionTasks);
        var citizens = await _stimmregister.GetPersonInfos(sheet.CollectionMunicipality!.Bfs, decryptedIds.ToHashSet(), sheet.ReceivedAt.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        return citizens
            .OrderBy(x => x.OfficialName)
            .ThenBy(x => x.FirstName)
            .ThenBy(x => x.DateOfBirth);
    }

    public async Task AddCitizen(CollectionType collectionType, Guid collectionId, Guid sheetId, Guid personRegisterId)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var collection = await BuildSignatureInfoQuery(collectionType)
                             .WhereCanEditSignatureSheets(_permissionService)
                             .FirstOrDefaultAsync(x => x.Id == collectionId)
                         ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), new { collectionType, collectionId });

        var sheet = await _signatureSheetRepository
                        .Query()
                        .AsTracking()
                        .WhereCanEdit(_permissionService)
                        .Include(x => x.CollectionMunicipality)
                        .ForUpdate() // serialize access to validate count
                        .FirstOrDefaultAsync(x => x.Id == sheetId && x.CollectionMunicipality!.CollectionId == collectionId)
                    ?? throw new EntityNotFoundException(nameof(CollectionSignatureSheetEntity), sheetId);

        if (!sheet.Count.CanAdd)
        {
            throw new ValidationException("The signature sheet is full.");
        }

        var personInfo = await _stimmregister.GetPersonInfo(sheet.CollectionMunicipality!.Bfs, personRegisterId, sheet.ReceivedAt.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        if (!personInfo.IsVotingAllowed)
        {
            throw new ValidationException("The person does not have the right to vote");
        }

        var encryptResult = await _cryptoService.EncryptStimmregisterId(collection, personInfo);
        await _signService.LockAndEnsureCanSign(collection, personInfo, encryptResult.Mac);

        // do not count them yet against the collection count
        // they only count against the collection count when the sheet is attested.
        sheet.Count.Valid++;
        sheet.Count.Invalid--;
        _permissionService.SetModified(sheet);
        await _dataContext.SaveChangesAsync();

        var collectionMunicipalityId = await _collectionMunicipalityRepository.Query()
            .Where(x => x.CollectionId == collection.Id && x.Bfs == personInfo.MunicipalityId.ToString())
            .Select(x => x.Id)
            .FirstAsync();

        var citizenEntry = new CollectionCitizenEntity
        {
            CollectionMunicipalityId = collectionMunicipalityId,
            SignatureSheetId = sheetId,
            Age = personInfo.Age,
            CollectionDateTime = _timeProvider.GetUtcNowDateTime(),
            Sex = personInfo.Sex,
            Log = new CollectionCitizenLogEntity
            {
                VotingStimmregisterIdMac = encryptResult.Mac,
                VotingStimmregisterIdEncrypted = encryptResult.Encrypted,
                CollectionId = collectionId,
            },
        };
        _permissionService.SetCreated(citizenEntry);
        _permissionService.SetCreated(citizenEntry.Log);
        await _citizenRepository.Create(citizenEntry);

        await transaction.CommitAsync();
    }

    public async Task RemoveCitizen(Guid collectionId, Guid sheetId, Guid personRegisterId)
    {
        await EnsureCanEditSignatureSheets(collectionId);

        await using var transaction = await _dataContext.BeginTransaction();

        var sheet = await _signatureSheetRepository
                         .Query()
                         .Include(x => x.CollectionMunicipality!.Collection)
                         .AsTracking()
                         .WhereCanEdit(_permissionService)
                         .ForUpdate() // serialize access to set count correctly
                         .FirstOrDefaultAsync(x => x.Id == sheetId && x.CollectionMunicipality!.CollectionId == collectionId)
                     ?? throw new EntityNotFoundException(nameof(CollectionSignatureSheetEntity), sheetId);

        var personMac = await _cryptoService.StimmregisterIdHmac(sheet.CollectionMunicipality!.Collection!, personRegisterId);

        _permissionService.SetModified(sheet);
        sheet.Count.Valid--;
        sheet.Count.Invalid++;
        await _dataContext.SaveChangesAsync();

        // do not log person register id since it would be identifiable information.
        var keyToDelete = await _citizenRepository.Query()
            .Where(x => x.SignatureSheetId == sheetId && x.Log!.VotingStimmregisterIdMac == personMac)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync()
            ?? throw new EntityNotFoundException(nameof(CollectionCitizenEntity), new { collectionId, sheetId, personRegisterId = "<redacted>" });
        await _citizenRepository.AuditedDeleteRange(q => q.Where(x => keyToDelete == x.Id));
        await transaction.CommitAsync();
    }

    public async Task<SignatureSheetStateChangeResult> Submit(Guid collectionId, Guid sheetId)
    {
        await EnsureCanCheckSamples(collectionId);

        var sheet = await _signatureSheetRepository.Query()
                        .WhereCanSubmit(_permissionService)
                        .Where(x => x.CollectionMunicipality!.CollectionId == collectionId && x.Id == sheetId)
                        .Include(x => x.CollectionMunicipality)
                        .FirstOrDefaultAsync()
                    ?? throw new EntityNotFoundException(nameof(CollectionSignatureSheetEntity), sheetId);
        await SetSheetState(sheet, CollectionSignatureSheetState.Submitted);
        return new SignatureSheetStateChangeResult(CollectionSignatureSheetPermissions.Build(_permissionService, sheet));
    }

    public async Task<SignatureSheetStateChangeResult> Unsubmit(Guid collectionId, Guid sheetId)
    {
        await EnsureCanCheckSamples(collectionId);

        var sheet = await _signatureSheetRepository.Query()
                        .WhereCanUnsubmit(_permissionService)
                        .Where(x => x.CollectionMunicipality!.CollectionId == collectionId && x.Id == sheetId)
                        .Include(x => x.CollectionMunicipality)
                        .FirstOrDefaultAsync()
                    ?? throw new EntityNotFoundException(nameof(CollectionSignatureSheetEntity), sheetId);

        await SetSheetState(sheet, CollectionSignatureSheetState.Attested);
        return new SignatureSheetStateChangeResult(CollectionSignatureSheetPermissions.Build(_permissionService, sheet));
    }

    public async Task<SignatureSheetStateChangeResult> Discard(Guid collectionId, Guid sheetId)
    {
        await EnsureCanCheckSamples(collectionId);

        await using var transaction = await _dataContext.BeginTransaction();
        var sheet = await _signatureSheetRepository.Query()
                        .WhereCanDiscard(_permissionService)
                        .Where(x => x.CollectionMunicipality!.CollectionId == collectionId && x.Id == sheetId)
                        .Include(x => x.CollectionMunicipality)
                        .FirstOrDefaultAsync()
                    ?? throw new EntityNotFoundException(nameof(CollectionSignatureSheetEntity), sheetId);

        await LockMunicipalityAndCollectionCount(collectionId, sheet.CollectionMunicipalityId);
        await SetSheetStateAndUpdateCounts(sheet, collectionId, CollectionSignatureSheetState.NotSubmitted, -sheet.Count.Valid, -sheet.Count.Invalid);

        await transaction.CommitAsync();

        var collectionCount = await _collectionCountRepository.Query().FirstAsync(x => x.CollectionId == collectionId);
        return new SignatureSheetStateChangeResult(CollectionSignatureSheetPermissions.Build(_permissionService, sheet), collectionCount);
    }

    public async Task<SignatureSheetStateChangeResult> Restore(Guid collectionId, Guid sheetId)
    {
        await EnsureCanCheckSamples(collectionId);

        await using var transaction = await _dataContext.BeginTransaction();

        var sheet = await _signatureSheetRepository.Query()
                        .WhereCanRestore(_permissionService)
                        .Where(x => x.CollectionMunicipality!.CollectionId == collectionId && x.Id == sheetId)
                        .Include(x => x.CollectionMunicipality)
                        .FirstOrDefaultAsync()
                    ?? throw new EntityNotFoundException(nameof(CollectionSignatureSheetEntity), sheetId);

        await LockMunicipalityAndCollectionCount(collectionId, sheet.CollectionMunicipalityId);
        await SetSheetStateAndUpdateCounts(sheet, collectionId, CollectionSignatureSheetState.Attested, sheet.Count.Valid, sheet.Count.Invalid);

        await transaction.CommitAsync();

        var collectionCount = await _collectionCountRepository.Query().FirstAsync(x => x.CollectionId == collectionId);
        return new SignatureSheetStateChangeResult(CollectionSignatureSheetPermissions.Build(_permissionService, sheet), collectionCount);
    }

    public async Task<SignatureSheetConfirmResult> Confirm(SignatureSheetConfirmRequest request)
    {
        var collection = await BuildSignatureInfoQuery(request.CollectionType)
                             .WhereCanCheckSamples(_permissionService)
                             .FirstOrDefaultAsync(x => x.Id == request.CollectionId)
                         ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), new { request.CollectionType, request.CollectionId });

        var sheet = await _signatureSheetRepository.Query()
                        .AsTracking()
                        .WhereCanConfirm(_permissionService)
                        .Include(x => x.CollectionMunicipality!.Collection)
                        .ForUpdate() // serialize access to validate count
                        .FirstOrDefaultAsync(x => x.CollectionMunicipality!.CollectionId == request.CollectionId && x.Id == request.SheetId)
                    ?? throw new EntityNotFoundException(nameof(CollectionSignatureSheetEntity), request.SheetId);

        // nothing changed except the state
        if (request.AddedPersonRegisterIds.Count == 0 && request.RemovedPersonRegisterIds.Count == 0 && request.SignatureCountTotal == sheet.Count.Total)
        {
            await SetSheetState(sheet, CollectionSignatureSheetState.Confirmed);
            return await GetConfirmSignatureSheetResult(sheet.CollectionMunicipalityId, sheet.Number);
        }

        var previousValidCount = sheet.Count.Valid;
        var newValidCount = previousValidCount - request.RemovedPersonRegisterIds.Count + request.AddedPersonRegisterIds.Count;

        var personInfos = await _stimmregister.GetPersonInfos(sheet.CollectionMunicipality!.Bfs, request.AddedPersonRegisterIds, sheet.ReceivedAt.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
        EnsureConfirmRequestIsValid(request, newValidCount, personInfos);

        await using var transaction = await _dataContext.BeginTransaction();

        // lock municipality and collection count to set count correctly
        await LockMunicipalityAndCollectionCount(request.CollectionId, sheet.CollectionMunicipalityId);

        var previousInvalidCount = sheet.Count.Invalid;
        var newInvalidCount = request.SignatureCountTotal - newValidCount;
        var validDifference = newValidCount - previousValidCount;
        var invalidDifference = newInvalidCount - previousInvalidCount;

        await UpdateMunicipalityAndCollectionCounts(sheet.CollectionMunicipalityId, request.CollectionId, validDifference, invalidDifference);

        await _signatureSheetRepository.AuditedUpdate(sheet, () =>
        {
            sheet.State = CollectionSignatureSheetState.Confirmed;
            sheet.Count.Valid = newValidCount;
            sheet.Count.Invalid = newInvalidCount;
            sheet.ModifiedBySuperiorAuthority = true;
            _permissionService.SetModified(sheet);
        });

        await RemoveCitizens(request, sheet);

        var citizensToAdd = await GetCitizensToAdd(collection, request, personInfos, sheet);
        await _citizenRepository.CreateRange(citizensToAdd);

        await transaction.CommitAsync();
        return await GetConfirmSignatureSheetResult(sheet.CollectionMunicipalityId, sheet.Number);
    }

    public async Task<List<CollectionSignatureSheetEntity>> ListSamples(Guid collectionId)
    {
        await EnsureCanCheckSamples(collectionId);

        return await _signatureSheetRepository.Query()
                        .WhereCanCheckSamples(_permissionService)
                        .Where(x => x.CollectionMunicipality!.CollectionId == collectionId && x.IsSample)
                        .Include(x => x.CollectionMunicipality)
                        .OrderBy(x => x.State)
                        .ThenBy(x => x.CollectionMunicipality!.MunicipalityName)
                        .ThenBy(x => x.Number)
                        .ToListAsync();
    }

    public async Task<List<CollectionSignatureSheetEntity>> AddSamples(Guid collectionId, int signatureSheetsCount)
    {
        await EnsureCanCheckSamples(collectionId);
        await EnsureAllSignatureSheetsArePastAttested(collectionId);

        var potentialSampleIds = await _signatureSheetRepository.Query()
            .WhereCanCheckSamples(_permissionService)
            .Where(x => x.CollectionMunicipality!.CollectionId == collectionId &&
                        x.State == CollectionSignatureSheetState.Submitted &&
                        !x.IsSample)
            .Select(x => x.Id)
            .ToArrayAsync();

        if (potentialSampleIds.Length < signatureSheetsCount)
        {
            throw new TooManyCollectionSignatureSheetSamplesException();
        }

        // need to shuffle and take since .GetItems() will return duplicates
        RandomNumberGenerator.Shuffle(potentialSampleIds.AsSpan());
        var sampleIds = potentialSampleIds.Take(signatureSheetsCount);
        var sheets = await _signatureSheetRepository.Query()
            .AsTracking()
            .Where(x => x.CollectionMunicipality!.CollectionId == collectionId && sampleIds.Contains(x.Id))
            .Include(x => x.CollectionMunicipality)
            .OrderBy(x => x.CollectionMunicipality!.MunicipalityName)
            .ThenBy(x => x.Number)
            .ToListAsync();

        foreach (var sheet in sheets)
        {
            sheet.IsSample = true;
            _permissionService.SetModified(sheet);
        }

        await _dataContext.SaveChangesAsync();
        return sheets;
    }

    private static Guid EnsureAllSheetsFoundAndOfSameMunicipality(IReadOnlySet<Guid> signatureSheetIds, List<CollectionSignatureSheetEntity> signatureSheets)
    {
        if (signatureSheets.Count != signatureSheetIds.Count)
        {
            var foundIds = signatureSheets.Select(x => x.Id).ToHashSet();
            var notFoundId = signatureSheetIds.Except(foundIds).First();
            throw new EntityNotFoundException(nameof(CollectionSignatureSheetEntity), notFoundId);
        }

        var collectionMunicipalityIds = signatureSheets.Select(x => x.CollectionMunicipalityId).ToHashSet();
        if (collectionMunicipalityIds.Count != 1)
        {
            throw new ValidationException("All signature sheets must belong to the same municipality.");
        }

        return collectionMunicipalityIds.First();
    }

    private async Task EnsureCanEditSignatureSheets(Guid collectionId)
    {
        var hasCollectionAccess = await _collectionRepository
            .Query()
            .WhereCanEditSignatureSheets(_permissionService)
            .AnyAsync(x => x.Id == collectionId);
        if (!hasCollectionAccess)
        {
            throw new EntityNotFoundException(nameof(CollectionBaseEntity), collectionId);
        }
    }

    private async Task EnsureCanReadSignatureSheets(Guid collectionId)
    {
        var hasCollectionAccess = await _collectionRepository
            .Query()
            .WhereCanReadSignatureSheets(_permissionService)
            .AnyAsync(x => x.Id == collectionId);
        if (!hasCollectionAccess)
        {
            throw new EntityNotFoundException(nameof(CollectionBaseEntity), collectionId);
        }
    }

    private async Task EnsureCanReadSignatureSheetsOrCheckSamples(Guid collectionId)
    {
        var collection = await _collectionRepository
                             .Query()
                             .FirstOrDefaultAsync(x => x.Id == collectionId)
                         ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), collectionId);
        collection.SetPeriodState(_timeProvider.GetUtcTodayDateOnly());

        if (CollectionPermissions.CanReadSignatureSheets(_permissionService, collection) || CollectionPermissions.CanCheckSamples(_permissionService, collection))
        {
            return;
        }

        throw new EntityNotFoundException(nameof(CollectionBaseEntity), collectionId);
    }

    private async Task EnsureCanCheckSamples(Guid collectionId)
    {
        var hasCollectionAccess = await _collectionRepository
            .Query()
            .WhereCanCheckSamples(_permissionService)
            .AnyAsync(x => x.Id == collectionId);
        if (!hasCollectionAccess)
        {
            throw new EntityNotFoundException(nameof(CollectionBaseEntity), collectionId);
        }
    }

    private async Task EnsureAllSignatureSheetsArePastAttested(Guid collectionId)
    {
        var allSignatureSheetsArePastAttested = await _signatureSheetRepository
            .Query()
            .Where(x => x.CollectionMunicipality!.CollectionId == collectionId)
            .AllAsync(x =>
                x.State == CollectionSignatureSheetState.Submitted ||
                x.State == CollectionSignatureSheetState.NotSubmitted ||
                x.State == CollectionSignatureSheetState.Confirmed);

        if (!allSignatureSheetsArePastAttested)
        {
            throw new ValidationException("All signature sheets must be past attested.");
        }
    }

    private IQueryable<CollectionBaseEntity> BuildSignatureInfoQuery(CollectionType collectionType)
    {
        return collectionType switch
        {
            CollectionType.Initiative => _initiativeRepository.Query(),
            CollectionType.Referendum => _referendumRepository.Query()
                .AsSplitQuery()
                .AsNoTrackingWithIdentityResolution()
                .Include(x => x.Decree!.Collections
                    .Where(y => !string.IsNullOrWhiteSpace(y.EncryptionKeyId) && !string.IsNullOrWhiteSpace(y.MacKeyId))),
            _ => throw new ArgumentOutOfRangeException(nameof(collectionType), collectionType, "Unknown collection type"),
        };
    }

    private async Task LockMunicipalityAndCollectionCount(Guid collectionId, Guid collectionMunicipalityId)
    {
        _ = await _collectionMunicipalityRepository.Query()
                .ForUpdate()
                .Where(x => x.Id == collectionMunicipalityId)
                .Select(_ => (int?)1)
                .FirstOrDefaultAsync()
            ?? throw new EntityNotFoundException(nameof(CollectionMunicipalityEntity), collectionMunicipalityId);

        _ = await _collectionCountRepository.Query()
                .ForUpdate()
                .Where(x => x.CollectionId == collectionId)
                .Select(_ => (int?)1)
                .FirstOrDefaultAsync()
            ?? throw new EntityNotFoundException(nameof(CollectionCountEntity), collectionId);
    }

    private async Task SetSheetStateAndUpdateCounts(CollectionSignatureSheetEntity sheet, Guid collectionId, CollectionSignatureSheetState state, int validDifference, int invalidDifference)
    {
        await SetSheetState(sheet, state);
        await UpdateMunicipalityAndCollectionCounts(sheet.CollectionMunicipalityId, collectionId, validDifference, invalidDifference);
    }

    private async Task SetSheetState(CollectionSignatureSheetEntity sheet, CollectionSignatureSheetState state)
    {
        await _signatureSheetRepository.AuditedUpdateRange(
            q => q.Where(x => x.Id == sheet.Id),
            x =>
            {
                x.State = state;
                _permissionService.SetModified(x);
            });

        // building the user permissions later required an updated state
        sheet.State = state;
    }

    private async Task UpdateMunicipalityAndCollectionCounts(Guid collectionMunicipalityId, Guid collectionId, int validDifference, int invalidDifference)
    {
        await _collectionMunicipalityRepository.AuditedUpdateRange(
            q => q.Where(x => x.Id == collectionMunicipalityId),
            x =>
            {
                x.PhysicalCount.Valid += validDifference;
                x.PhysicalCount.Invalid += invalidDifference;
                _permissionService.SetModified(x);
            });

        await _collectionCountRepository.AuditedUpdateRange(
            q => q.Where(x => x.CollectionId == collectionId),
            x =>
            {
                x.TotalCitizenCount += validDifference;
                _permissionService.SetModified(x);
            });
    }

    private async Task<SignatureSheetConfirmResult> GetConfirmSignatureSheetResult(Guid municipalityId, int currentSignatureSheetNumber)
    {
        var nextSignatureSheetId = await _signatureSheetRepository.Query()
            .WhereCanConfirm(_permissionService)
            .Where(x => x.CollectionMunicipalityId == municipalityId && x.Number > currentSignatureSheetNumber)
            .OrderBy(x => x.Number)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        return new SignatureSheetConfirmResult(nextSignatureSheetId == Guid.Empty ? null : nextSignatureSheetId);
    }

    private void EnsureConfirmRequestIsValid(SignatureSheetConfirmRequest request, int newValidCount, IReadOnlyList<IVotingStimmregisterPersonInfo> personInfos)
    {
        if (request.AddedPersonRegisterIds.Intersect(request.RemovedPersonRegisterIds).Any())
        {
            throw new ValidationException("Cannot add and remove the same person register id");
        }

        if (request.SignatureCountTotal < newValidCount)
        {
            throw new ValidationException("Cannot set total count lower than valid count");
        }

        if (personInfos.Any(x => !x.IsVotingAllowed))
        {
            throw new ValidationException("One person does not have the right to vote");
        }
    }

    private async Task RemoveCitizens(SignatureSheetConfirmRequest request, CollectionSignatureSheetEntity sheet)
    {
        var personMacsToRemove = await _cryptoService.StimmregisterIdHmacs(sheet.CollectionMunicipality!.Collection!, request.RemovedPersonRegisterIds);
        var keysToDelete = await _citizenRepository.Query()
            .Where(x => x.SignatureSheetId == request.SheetId && personMacsToRemove.Contains(x.Log!.VotingStimmregisterIdMac))
            .Select(x => x.Id)
            .ToListAsync();

        if (request.RemovedPersonRegisterIds.Count != keysToDelete.Count)
        {
            // do not log person register id since it would be identifiable information.
            throw new EntityNotFoundException(nameof(CollectionCitizenEntity), new { request.CollectionId, request.SheetId, personRegisterId = "<redacted>" });
        }

        await _citizenRepository.AuditedDeleteRange(q => q.Where(x => keysToDelete.Contains(x.Id)));
    }

    private async Task<IEnumerable<CollectionCitizenEntity>> GetCitizensToAdd(CollectionBaseEntity collection, SignatureSheetConfirmRequest request, IReadOnlyList<IVotingStimmregisterPersonInfo> personInfos, CollectionSignatureSheetEntity sheet)
    {
        var encryptResults = await _cryptoService.EncryptStimmregisterIds(collection, request.AddedPersonRegisterIds);
        await _signService.LockAndEnsureCanSign(collection, request.AddedPersonRegisterIds, encryptResults.Select(x => x.Mac).ToList());

        var citizensToAdd = new List<CollectionCitizenEntity>();
        for (var i = 0; i < personInfos.Count; i++)
        {
            var encryptResult = encryptResults[i];
            var citizenEntry = new CollectionCitizenEntity
            {
                CollectionMunicipalityId = sheet.CollectionMunicipalityId,
                SignatureSheetId = request.SheetId,
                Age = personInfos[i].Age,
                CollectionDateTime = _timeProvider.GetUtcNowDateTime(),
                Sex = personInfos[i].Sex,
                Log = new CollectionCitizenLogEntity
                {
                    VotingStimmregisterIdMac = encryptResult.Mac,
                    VotingStimmregisterIdEncrypted = encryptResult.Encrypted,
                    CollectionId = request.CollectionId,
                },
            };
            _permissionService.SetCreated(citizenEntry);
            _permissionService.SetCreated(citizenEntry.Log);
            citizensToAdd.Add(citizenEntry);
        }

        return citizensToAdd;
    }
}
