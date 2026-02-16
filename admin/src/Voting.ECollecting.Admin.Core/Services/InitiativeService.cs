// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Core.Models;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Admin.Core.Exceptions;
using Voting.ECollecting.Admin.Core.Mappings;
using Voting.ECollecting.Admin.Core.Permissions;
using Voting.ECollecting.Admin.Core.Resources;
using Voting.ECollecting.Admin.Core.Services.Crypto;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Admin.Domain.Queries;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.Lib.Common.Files;
using Voting.Lib.Database.Postgres.Locking;
using Voting.Lib.Iam.SecondFactor.Models;
using Voting.Lib.Iam.SecondFactor.Services;
using IAccessControlListDoiService = Voting.ECollecting.Shared.Abstractions.Core.Services.IAccessControlListDoiService;
using IPermissionService = Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam.IPermissionService;
using SecondFactorTransactionInfo = Voting.ECollecting.Admin.Domain.Models.SecondFactorTransactionInfo;

namespace Voting.ECollecting.Admin.Core.Services;

public class InitiativeService : IInitiativeService
{
    private readonly IInitiativeRepository _initiativeRepository;
    private readonly IInitiativeSubTypeRepository _initiativeSubTypeRepository;
    private readonly IAccessControlListDoiRepository _accessControlListDoiRepository;
    private readonly IStatisticalDataCsvGenerator _statisticalDataCsvGenerator;
    private readonly IStatisticalDataTimeLapseCsvGenerator _statisticalDataTimeLapseCsvGenerator;
    private readonly IAccessControlListDoiService _coreAccessControlListDoiService;
    private readonly IOfficialJournalPublicationProtocolGenerator _officialJournalPublicationProtocolGenerator;
    private readonly IElectronicSignaturesProtocolGenerator _electronicSignaturesProtocolGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly CollectionService _collectionService;
    private readonly IDataContext _dataContext;
    private readonly IPermissionService _permissionService;
    private readonly ISecondFactorTransactionService _secondFactorTransactionService;
    private readonly CollectionCryptoService _collectionCryptoService;
    private readonly AccessControlListDoiService _accessControlListDoiService;
    private readonly IUserNotificationService _userNotificationService;

    public InitiativeService(
        IInitiativeRepository initiativeRepository,
        TimeProvider timeProvider,
        CollectionService collectionService,
        IPermissionService permissionService,
        IDataContext dataContext,
        IInitiativeSubTypeRepository initiativeSubTypeRepository,
        IOfficialJournalPublicationProtocolGenerator officialJournalPublicationProtocolGenerator,
        IElectronicSignaturesProtocolGenerator electronicSignaturesProtocolGenerator,
        IAccessControlListDoiRepository accessControlListDoiRepository,
        IStatisticalDataCsvGenerator statisticalDataCsvGenerator,
        IStatisticalDataTimeLapseCsvGenerator statisticalDataTimeLapseCsvGenerator,
        IAccessControlListDoiService coreAccessControlListDoiService,
        ISecondFactorTransactionService secondFactorTransactionService,
        IUserNotificationService userNotificationService,
        CollectionCryptoService collectionCryptoService,
        AccessControlListDoiService accessControlListDoiService)
    {
        _initiativeRepository = initiativeRepository;
        _timeProvider = timeProvider;
        _collectionService = collectionService;
        _permissionService = permissionService;
        _dataContext = dataContext;
        _initiativeSubTypeRepository = initiativeSubTypeRepository;
        _officialJournalPublicationProtocolGenerator = officialJournalPublicationProtocolGenerator;
        _electronicSignaturesProtocolGenerator = electronicSignaturesProtocolGenerator;
        _accessControlListDoiRepository = accessControlListDoiRepository;
        _statisticalDataCsvGenerator = statisticalDataCsvGenerator;
        _statisticalDataTimeLapseCsvGenerator = statisticalDataTimeLapseCsvGenerator;
        _coreAccessControlListDoiService = coreAccessControlListDoiService;
        _secondFactorTransactionService = secondFactorTransactionService;
        _userNotificationService = userNotificationService;
        _collectionCryptoService = collectionCryptoService;
        _accessControlListDoiService = accessControlListDoiService;
    }

    public Task<List<InitiativeSubTypeEntity>> ListSubTypes()
    {
        return _initiativeSubTypeRepository.Query()
            .OrderBy(x => x.DomainOfInfluenceType)
            .ThenBy(x => x.Description)
            .ToListAsync();
    }

    public async Task<IReadOnlyDictionary<DomainOfInfluenceType, List<Initiative>>> ListByDoiType(
        IReadOnlySet<DomainOfInfluenceType>? doiTypes, string? bfs)
    {
        var today = _timeProvider.GetUtcTodayDateOnly();
        var query = _initiativeRepository.Query()
            .WhereCanRead(_permissionService)
            .Where(x => x.DomainOfInfluenceType.HasValue);

        if (doiTypes?.Count > 0)
        {
            query = query.Where(x => doiTypes.Contains(x.DomainOfInfluenceType!.Value));
        }

        if (!string.IsNullOrWhiteSpace(bfs))
        {
            query = query.Where(x => x.Bfs == bfs);
        }

        var initiativesByDoiTypes = await query
            .Include(x => x.CollectionCount)
            .Include(x => x.Permissions)
            .IncludeMunicipalities(_permissionService.AclBfsLists)

            // sort by closest expiration,
            // expired after non-expired.
            .OrderBy(x => x.DomainOfInfluenceType)
            .ThenBy(x => x.CollectionEndDate < today)
            .ThenBy(x => x.CollectionEndDate)
            .ThenBy(x => x.Description)
            .GroupBy(x => x.DomainOfInfluenceType)
            .ToDictionaryAsync(x => x.Key!.Value, x => x.ToList());

        return Enum.GetValues<DomainOfInfluenceType>()
            .Where(x => x != DomainOfInfluenceType.Unspecified)
            .OrderBy(x => x)
            .ToDictionary(x => x, x =>
            {
                var initiatives = Mapper.MapToInitiatives(initiativesByDoiTypes.GetValueOrDefault(x) ?? []);
                initiatives.SetPeriodStates(today);

                foreach (var initiative in initiatives)
                {
                    _collectionService.LoadPermission(initiative);
                    _collectionService.SetCollectionCount(initiative);
                }

                return initiatives;
            });
    }

    public async Task<Initiative> Get(Guid id)
    {
        var initiativeEntity = await _initiativeRepository.Query()
                                   .WhereCanRead(_permissionService)
                                   .Include(x => x.CollectionCount)
                                   .Include(x => x.SubType)
                                   .IncludeMunicipalities(_permissionService.AclBfsLists)

                                   // include files but not the file content
                                   .Include(x => x.Image)
                                   .Include(x => x.Logo)
                                   .Include(x => x.SignatureSheetTemplate)
                                   .FirstOrDefaultAsync(x => x.Id == id)
                               ?? throw new EntityNotFoundException(nameof(InitiativeEntity), id);
        var initiative = Mapper.MapToInitiative(initiativeEntity);
        initiative.SetPeriodState(_timeProvider.GetUtcTodayDateOnly());
        _collectionService.LoadPermission(initiative);
        _collectionService.SetCollectionCount(initiative);
        initiative.DomainOfInfluenceName = await _accessControlListDoiService.LoadDomainOfInfluenceName(initiative.DomainOfInfluenceType!.Value, initiative.Bfs!);
        return initiative;
    }

    public async Task FinishCorrection(Guid id)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var initiative = await _initiativeRepository.Query()
                             .WhereCanFinishCorrection(_permissionService)
                             .AsTracking()
                             .Include(x => x.Permissions)
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), id);

        initiative.State = CollectionState.ReadyForRegistration;

        _permissionService.SetModified(initiative);
        await _dataContext.SaveChangesAsync();

        await _collectionService.AddStateChangedMessage(initiative);

        await transaction.CommitAsync();
    }

    public async Task SetCollectionPeriod(Guid id, DateOnly collectionStartDate, DateOnly collectionEndDate)
    {
        var initiative = await _initiativeRepository.Query()
                             .WhereCanSetCollectionPeriod(_permissionService)
                             .AsTracking()
                             .Include(x => x.Permissions)
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), id);

        EnsureValidCollectionDates(_timeProvider.GetUtcTodayDateOnly(), collectionStartDate, collectionEndDate);

        initiative.CollectionStartDate = collectionStartDate;
        initiative.CollectionEndDate = collectionEndDate;

        await _collectionService.PrepareForCollection(initiative);

        _permissionService.SetModified(initiative);
        await _dataContext.SaveChangesAsync();
    }

    public async Task Enable(Guid id, DateOnly? collectionStartDate, DateOnly? collectionEndDate)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var initiative = await _initiativeRepository.Query()
                             .WhereCanEnable(_permissionService)
                             .AsTracking()
                             .Include(x => x.Permissions)
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), id);

        initiative.State = CollectionState.PreparingForCollection;
        if (initiative.IsElectronicSubmission)
        {
            EnsureValidCollectionDates(_timeProvider.GetUtcTodayDateOnly(), collectionStartDate, collectionEndDate);

            initiative.CollectionStartDate = collectionStartDate;
            initiative.CollectionEndDate = collectionEndDate;
        }

        _permissionService.SetModified(initiative);
        await _dataContext.SaveChangesAsync();

        await _collectionService.AddStateChangedMessage(initiative);

        await transaction.CommitAsync();
        await _collectionService.NotifyPreparingForCollection();
    }

    public async Task CameAbout(Guid id, DateOnly sensitiveDataExpiryDate)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        ValidateSensitiveDataExpiryDate(sensitiveDataExpiryDate);
        var collection = await _initiativeRepository.Query()
                             .WhereCanFinish(_permissionService)
                             .AsTracking()
                             .Include(x => x.Permissions)
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(typeof(InitiativeEntity), id);

        collection.State = CollectionState.EndedCameAbout;
        collection.SensitiveDataExpiryDate = sensitiveDataExpiryDate;
        _permissionService.SetModified(collection);

        await _dataContext.SaveChangesAsync();
        await _collectionService.AddStateChangedMessage(collection);

        await transaction.CommitAsync();
    }

    public async Task CameNotAbout(Guid id, CollectionCameNotAboutReason reason, DateOnly sensitiveDataExpiryDate)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        ValidateSensitiveDataExpiryDate(sensitiveDataExpiryDate);
        var collection = await _initiativeRepository.Query()
                             .WhereCanFinish(_permissionService)
                             .AsTracking()
                             .Include(x => x.Permissions)
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(typeof(InitiativeEntity), id);

        collection.State = CollectionState.EndedCameNotAbout;
        collection.SensitiveDataExpiryDate = sensitiveDataExpiryDate;
        collection.CameNotAboutReason = reason;
        _permissionService.SetModified(collection);

        await _dataContext.SaveChangesAsync();
        await _collectionService.AddStateChangedMessage(collection);

        await transaction.CommitAsync();
    }

    public async Task Update(Guid id, UpdateInitiativeParams updateParams)
    {
        var initiative = await _initiativeRepository
                             .Query()
                             .WhereCanEditGeneralInformation(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), id);

        initiative.SubTypeId = updateParams.SubTypeId;
        initiative.Description = updateParams.Description;
        initiative.Wording = updateParams.Wording;
        initiative.Address = updateParams.Address ?? new();

        await ValidateGeneralInformation(initiative);
        await _dataContext.SaveChangesAsync();
    }

    public async IAsyncEnumerable<IFile> GetDocuments(Guid id, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var initiative = await _initiativeRepository.Query()
                         .WhereCanGenerateDocuments(_permissionService)
                         .Include(x => x.Permissions)
                         .Include(x => x.Municipalities)
                         .Include(x => x.CollectionCount)
                         .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                     ?? throw new EntityNotFoundException(typeof(InitiativeEntity), id);

        await foreach (var file in GenerateFiles(initiative, cancellationToken))
        {
            yield return file;
        }
    }

    public async Task ReturnForCorrection(Guid id, InitiativeLockedFields? lockedFields)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var initiative = await _initiativeRepository.Query()
                             .WhereCanReturnForCorrection(_permissionService)
                             .AsTracking()
                             .Include(x => x.Permissions)
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), id);

        initiative.State = CollectionState.ReturnedForCorrection;
        initiative.LockedFields = lockedFields ?? new InitiativeLockedFields();
        _permissionService.SetModified(initiative);
        await _dataContext.SaveChangesAsync();

        await _collectionService.AddStateChangedMessage(initiative);

        await transaction.CommitAsync();
    }

    public async Task<SecondFactorTransactionInfo> PrepareDelete(Guid initiativeId)
    {
        var actionId = await CreateDeleteActionId(initiativeId, false);
        var info = await _secondFactorTransactionService.Create(
            actionId,
            Strings.SecondFactorTransaction_DeleteInitiative);
        return new SecondFactorTransactionInfo(info.Transaction.Id, info.CorrelationCode, info.QrCode);
    }

    public async Task Delete(Guid initiativeId, Guid secondFactorId, CancellationToken cancellationToken)
    {
        await using var transaction = await _dataContext.BeginTransaction(cancellationToken);

        await _secondFactorTransactionService.EnsureVerified(
            secondFactorId,
            async () => await CreateDeleteActionId(initiativeId, true),
            cancellationToken);

        var collection = await _initiativeRepository.Query()
                             .WhereCanDelete(_permissionService)
                             .Include(x => x.Municipalities)
                             .Include(x => x.CollectionCount)
                             .FirstOrDefaultAsync(x => x.Id == initiativeId, cancellationToken: cancellationToken)
                         ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), initiativeId);

        var aclDoiType = Mapper.MapToAclDomainOfInfluenceType(collection.DomainOfInfluenceType!.Value);
        var recipient = await _accessControlListDoiRepository.Query()
                      .Where(x => x.Bfs == collection.Bfs && x.Type == aclDoiType)
                      .Select(x => x.ECollectingEmail)
                      .SingleOrDefaultAsync(cancellationToken)
                  ?? throw new EntityNotFoundException(nameof(AclDomainOfInfluenceType), new { collection.Bfs, collection.DomainOfInfluenceType, });

        if (!string.IsNullOrEmpty(recipient) && !string.IsNullOrEmpty(collection.Bfs))
        {
            var attachment = ZipFile.Create(GenerateFiles(collection, cancellationToken), "archive.zip");
            await _userNotificationService.SendUserNotification(
                recipient,
                false,
                UserNotificationType.CollectionDeleted,
                collection: collection,
                attachments: [attachment],
                cancellationToken: cancellationToken);
        }

        await _collectionCryptoService.DeleteKeys(collection);
        await _initiativeRepository.AuditedDelete(collection);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task SetSensitiveDataExpiryDate(Guid initiativeId, DateOnly date)
    {
        var collection = await _initiativeRepository
                             .Query()
                             .WhereCanSetSensitiveDataExpiryDate(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == initiativeId)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), initiativeId);

        ValidateSensitiveDataExpiryDate(date);
        collection.SensitiveDataExpiryDate = date;
        _permissionService.SetModified(collection);
        await _dataContext.SaveChangesAsync();
    }

    internal async Task ValidateGeneralInformation(InitiativeEntity initiative)
    {
        if (initiative is { DomainOfInfluenceType: DomainOfInfluenceType.Ct, SubTypeId: null })
        {
            throw new ValidationException("SubType is required for cantonal initiatives.");
        }

        if (string.IsNullOrWhiteSpace(initiative.Wording) && initiative.DomainOfInfluenceType != DomainOfInfluenceType.Ch)
        {
            throw new ValidationException("Wording is required for non-federal initiatives.");
        }

        if (initiative.DomainOfInfluenceType != DomainOfInfluenceType.Ch && initiative.Address is not { IsComplete: true })
        {
            throw new ValidationException("Address is required for non-federal initiatives.");
        }

        var aclDoiType = Mapper.MapToAclDomainOfInfluenceType(initiative.DomainOfInfluenceType!.Value);
        var bfs = await _accessControlListDoiRepository.GetSingleBfsForDoiType(_permissionService.AclBfsLists, aclDoiType);
        if (initiative.Bfs != bfs && !string.IsNullOrWhiteSpace(initiative.Bfs))
        {
            throw new ValidationException("Cannot update bfs of initiative.");
        }

        initiative.Bfs = bfs;

        await ValidateUnique(initiative, bfs);
        await ValidateSubType(initiative);
    }

    private async Task ValidateUnique(InitiativeEntity initiative, string bfs)
    {
        // this is not a mission-critical check,
        // therefore no need to do it race-condition safe.
        // case-insensitive comparison is not yet supported by npgsql (CA1862)
        // since there should only be very few initiatives per bfs,
        // this should not be a performance issue.
        var existsAlready = await _initiativeRepository.Query()
            .WhereCanEdit(_permissionService)
            .AnyAsync(x => x.Id != initiative.Id && x.Bfs == bfs && EF.Functions.ILike(x.Description, initiative.Description));

        if (existsAlready)
        {
            throw new CollectionAlreadyExistsException();
        }
    }

    private async Task ValidateSubType(InitiativeEntity initiative)
    {
        if (!initiative.SubTypeId.HasValue)
        {
            return;
        }

        var hasSubType = await _initiativeSubTypeRepository.Query()
            .Where(x => x.Id == initiative.SubTypeId!.Value &&
                        x.DomainOfInfluenceType == initiative.DomainOfInfluenceType)
            .AnyAsync();
        if (!hasSubType)
        {
            throw new EntityNotFoundException(nameof(InitiativeSubTypeEntity), initiative.SubTypeId!.Value);
        }
    }

    private void EnsureValidCollectionDates(DateOnly utcNow, DateOnly? collectionStartDate, DateOnly? collectionEndDate)
    {
        if (!collectionStartDate.HasValue)
        {
            throw new ValidationException("Collection start date cannot be null for an electronic submission.");
        }

        if (!collectionEndDate.HasValue)
        {
            throw new ValidationException("Collection end date cannot be null for an electronic submission.");
        }

        if (collectionStartDate < utcNow)
        {
            throw new ValidationException("Collection start date can't be in the past.");
        }

        if (collectionStartDate >= collectionEndDate)
        {
            throw new ValidationException("Collection end date needs to be past the start date.");
        }
    }

    private void ValidateSensitiveDataExpiryDate(DateOnly sensitiveDataExpiryDate)
    {
        if (sensitiveDataExpiryDate <= _timeProvider.GetUtcTodayDateOnly())
        {
            throw new ValidationException("Sensitive data expiry date must be in the future.");
        }
    }

    private async Task<SecondFactorTransactionActionId> CreateDeleteActionId(Guid initiativeId, bool forUpdate)
    {
        var query = _initiativeRepository.Query().WhereCanDelete(_permissionService);
        if (forUpdate)
        {
            query = query.ForUpdate();
        }

        var collection = await query.FirstOrDefaultAsync(x => x.Id == initiativeId)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), initiativeId);

        // no need to include anything despite the id since the collection is immutable in the state where it can be deleted.
        return SecondFactorTransactionActionId.Create(
            SecondFactorTransactionActionTypes.DeleteInitiative,
            collection.Id);
    }

    private async IAsyncEnumerable<IFile> GenerateFiles(
        InitiativeEntity initiative,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        yield return _statisticalDataCsvGenerator.GenerateFile(new StatisticalDataTemplateData([initiative.Id], initiative.Description));
        yield return await _statisticalDataTimeLapseCsvGenerator.GenerateFile(new StatisticalDataTimeLapseTemplateData([initiative.Id], initiative.Description));
        if (initiative.DomainOfInfluenceType is DomainOfInfluenceType.Mu)
        {
            yield break;
        }

        InitiativeSubTypeEntity? subType = null;
        if (initiative.SubTypeId is not null)
        {
            subType = await _initiativeSubTypeRepository.Query()
                .FirstOrDefaultAsync(x => x.Id == initiative.SubTypeId, cancellationToken);
        }

        var accessControlListDoi = await _coreAccessControlListDoiService.GetAccessControlListDoiWithChildren(initiative.Bfs!);
        var templateData = new ECollectingProtocolTemplateData(
            [initiative],
            accessControlListDoi,
            initiative.Description,
            initiative.DomainOfInfluenceType!.Value,
            false,
            subType);
        yield return await _officialJournalPublicationProtocolGenerator.GenerateFileModel(templateData, cancellationToken);
        yield return await _electronicSignaturesProtocolGenerator.GenerateFileModel(templateData, cancellationToken);
    }
}
