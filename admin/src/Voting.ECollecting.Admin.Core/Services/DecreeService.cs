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
using Voting.ECollecting.Admin.Core.Mappings;
using Voting.ECollecting.Admin.Core.Permissions;
using Voting.ECollecting.Admin.Core.Resources;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.ECollecting.Shared.Domain.Queries;
using Voting.Lib.Common.Files;
using Voting.Lib.Database.Postgres.Locking;
using Voting.Lib.Iam.SecondFactor.Models;
using Voting.Lib.Iam.SecondFactor.Services;
using CollectionCryptoService = Voting.ECollecting.Admin.Core.Services.Crypto.CollectionCryptoService;
using IDomainOfInfluenceService = Voting.ECollecting.Shared.Abstractions.Core.Services.IDomainOfInfluenceService;
using IPermissionService = Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam.IPermissionService;
using SecondFactorTransactionInfo = Voting.ECollecting.Admin.Domain.Models.SecondFactorTransactionInfo;

namespace Voting.ECollecting.Admin.Core.Services;

public class DecreeService : IDecreeService
{
    private readonly IDecreeRepository _decreeRepository;
    private readonly IReferendumRepository _referendumRepository;
    private readonly CollectionService _collectionService;
    private readonly IOfficialJournalPublicationProtocolGenerator _officialJournalPublicationProtocolGenerator;
    private readonly IElectronicSignaturesProtocolGenerator _electronicSignaturesProtocolGenerator;
    private readonly IStatisticalDataCsvGenerator _statisticalDataCsvGenerator;
    private readonly IStatisticalDataTimeLapseCsvGenerator _statisticalDataTimeLapseCsvGenerator;
    private readonly IPermissionService _permissionService;
    private readonly IDataContext _dataContext;
    private readonly IDomainOfInfluenceService _coreDomainOfInfluenceService;
    private readonly TimeProvider _timeProvider;
    private readonly IDomainOfInfluenceRepository _domainOfInfluenceRepository;
    private readonly ISecondFactorTransactionService _secondFactorTransactionService;
    private readonly CollectionCryptoService _collectionCryptoService;
    private readonly IUserNotificationService _userNotificationService;
    private readonly DomainOfInfluenceService _domainOfInfluenceService;

    public DecreeService(
        IDecreeRepository decreeRepository,
        IPermissionService permissionService,
        TimeProvider timeProvider,
        IReferendumRepository referendumRepository,
        CollectionService collectionService,
        IOfficialJournalPublicationProtocolGenerator officialJournalPublicationProtocolGenerator,
        IElectronicSignaturesProtocolGenerator electronicSignaturesProtocolGenerator,
        IStatisticalDataCsvGenerator statisticalDataCsvGenerator,
        IStatisticalDataTimeLapseCsvGenerator statisticalDataTimeLapseCsvGenerator,
        IDataContext dataContext,
        IDomainOfInfluenceService coreDomainOfInfluenceService,
        ISecondFactorTransactionService secondFactorTransactionService,
        CollectionCryptoService collectionCryptoService,
        IUserNotificationService userNotificationService,
        IDomainOfInfluenceRepository domainOfInfluenceRepository,
        DomainOfInfluenceService domainOfInfluenceService)
    {
        _decreeRepository = decreeRepository;
        _permissionService = permissionService;
        _timeProvider = timeProvider;
        _referendumRepository = referendumRepository;
        _collectionService = collectionService;
        _officialJournalPublicationProtocolGenerator = officialJournalPublicationProtocolGenerator;
        _electronicSignaturesProtocolGenerator = electronicSignaturesProtocolGenerator;
        _statisticalDataCsvGenerator = statisticalDataCsvGenerator;
        _statisticalDataTimeLapseCsvGenerator = statisticalDataTimeLapseCsvGenerator;
        _dataContext = dataContext;
        _coreDomainOfInfluenceService = coreDomainOfInfluenceService;
        _secondFactorTransactionService = secondFactorTransactionService;
        _collectionCryptoService = collectionCryptoService;
        _userNotificationService = userNotificationService;
        _domainOfInfluenceRepository = domainOfInfluenceRepository;
        _domainOfInfluenceService = domainOfInfluenceService;
    }

    public async Task<Decree> Create(Decree decree)
    {
        await SetBfsAndSignatureCounts(decree);
        decree.State = DecreeState.CollectionApplicable;

        SetPeriodStateAndUserPermissions(decree, _timeProvider.GetUtcTodayDateOnly());
        ValidateDecree(decree);
        _permissionService.SetCreated(decree);
        await _decreeRepository.Create(decree);
        decree.DomainOfInfluenceName = await _domainOfInfluenceService.LoadDomainOfInfluenceName(decree.DomainOfInfluenceType, decree.Bfs);
        return decree;
    }

    public async Task<List<Decree>> List()
    {
        var decreeEntities = await _decreeRepository
            .Query()
            .Include(x => x.Collections)
            .ThenInclude(x => x.CollectionCount)
            .WhereCanRead(_permissionService)
            .OrderByDescending(x => x.CollectionStartDate)
            .ThenBy(x => x.Description)
            .ToListAsync();
        var decrees = Mapper.MapToDecrees(decreeEntities);
        await LoadDomainOfInfluenceNames(decrees);
        SetStates(decrees);

        return decrees;
    }

    public async Task Update(Decree decree)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var existingDecree = await _decreeRepository.GetByKey(decree.Id)
            ?? throw new EntityNotFoundException(nameof(DecreeEntity), decree.Id);

        await SetBfsAndSignatureCounts(decree);
        SetPeriodStateAndUserPermissions(decree, _timeProvider.GetUtcTodayDateOnly());
        if (decree.UserPermissions?.CanEdit != true)
        {
            throw new EntityNotFoundException(nameof(DecreeEntity), decree.Id);
        }

        ValidateDecree(decree);
        _permissionService.SetModified(decree);

        await _decreeRepository.AuditedUpdate(existingDecree, () =>
        {
            existingDecree.Description = decree.Description;
            existingDecree.CollectionStartDate = decree.CollectionStartDate;
            existingDecree.CollectionEndDate = decree.CollectionEndDate;
            existingDecree.Link = decree.Link;
            existingDecree.DomainOfInfluenceType = decree.DomainOfInfluenceType;
            existingDecree.MinSignatureCount = decree.MinSignatureCount;
            existingDecree.MaxElectronicSignatureCount = decree.MaxElectronicSignatureCount;
            existingDecree.AuditInfo = decree.AuditInfo;
        });

        await _referendumRepository.AuditedUpdateRange(
            q => q.Where(x => x.DecreeId == decree.Id),
            x =>
        {
            x.CollectionStartDate = decree.CollectionStartDate;
            x.CollectionEndDate = decree.CollectionEndDate;
            x.MaxElectronicSignatureCount = decree.MaxElectronicSignatureCount;
            x.Bfs = decree.Bfs;
            x.DomainOfInfluenceType = decree.DomainOfInfluenceType;
        });

        await transaction.CommitAsync();
    }

    public async Task<Decree> GetForEdit(Guid id)
    {
        var decreeEntity = await _decreeRepository.Query()
            .WhereCanEdit(_permissionService)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new EntityNotFoundException(nameof(DecreeEntity), id);
        var decree = Mapper.MapToDecree(decreeEntity);
        SetPeriodStateAndUserPermissions(decree, _timeProvider.GetUtcTodayDateOnly());
        return decree;
    }

    public async Task DeletePublished(Guid id)
    {
        await using var transaction = await _dataContext.BeginTransaction();
        var collection = await _decreeRepository.Query()
            .WhereCanEdit(_permissionService)
            .FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new EntityNotFoundException(nameof(DecreeEntity), id);

        await _decreeRepository.AuditedDelete(collection);
        await transaction.CommitAsync();
    }

    public async Task SetSensitiveDataExpiryDate(Guid id, DateOnly date)
    {
        await using var transaction = await _dataContext.BeginTransaction();
        var collection = await _decreeRepository
                             .Query()
                             .WhereCanSetSensitiveDataExpiryDate(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(DecreeEntity), id);

        ValidateSensitiveDataExpiryDate(date);
        collection.SensitiveDataExpiryDate = date;
        _permissionService.SetModified(collection);
        await _dataContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<SecondFactorTransactionInfo> PrepareDelete(Guid id)
    {
        var actionId = await CreateDeleteActionId(id, false);
        var info = await _secondFactorTransactionService.Create(
            actionId,
            Strings.SecondFactorTransaction_DeleteDecree);
        return new SecondFactorTransactionInfo(info.Transaction.Id, info.CorrelationCode, info.QrCode);
    }

    public async Task Delete(Guid id, Guid secondFactorId, CancellationToken cancellationToken)
    {
        await using var transaction = await _dataContext.BeginTransaction(cancellationToken);

        await _secondFactorTransactionService.EnsureVerified(
            secondFactorId,
            async () => await CreateDeleteActionId(id, true),
            cancellationToken);

        var decree = await _decreeRepository.Query()
                             .WhereCanDelete(_permissionService)
                             .Include(x => x.Collections)
                             .ThenInclude(x => x.Municipalities)
                             .Include(x => x.Collections)
                             .ThenInclude(x => x.CollectionCount)
                             .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken)
                         ?? throw new EntityNotFoundException(nameof(DecreeEntity), id);

        if (!string.IsNullOrEmpty(decree.Bfs))
        {
            var recipients = await _domainOfInfluenceRepository.Query()
                                 .Where(x => x.Bfs == decree.Bfs && x.Type == decree.DomainOfInfluenceType)
                                 .Select(x => x.NotificationEmails)
                                 .SingleOrDefaultAsync(cancellationToken)
                             ?? [];

            var attachment = ZipFile.Create(GenerateFiles(decree, cancellationToken), "archive.zip");
            await _userNotificationService.SendUserNotifications(
                recipients,
                false,
                UserNotificationType.DecreeDeleted,
                new UserNotificationContext(Decree: decree, Attachments: [attachment]),
                cancellationToken: cancellationToken);
        }

        foreach (var collection in decree.Collections)
        {
            await _collectionCryptoService.DeleteKeys(collection);
        }

        await _decreeRepository.AuditedDelete(decree);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task CameAbout(Guid id, DateOnly sensitiveDataExpiryDate)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        ValidateSensitiveDataExpiryDate(sensitiveDataExpiryDate);
        var decree = await _decreeRepository.Query()
                             .WhereCanFinish(_permissionService)
                             .AsTracking()
                             .Include(x => x.Collections)
                             .ThenInclude(x => x.Permissions)
                             .WhereInCollectionOrExpired(_timeProvider.GetUtcTodayDateOnly())
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(typeof(InitiativeEntity), id);

        decree.SensitiveDataExpiryDate = sensitiveDataExpiryDate;
        decree.State = DecreeState.EndedCameAbout;
        _permissionService.SetModified(decree);

        foreach (var collection in decree.Collections)
        {
            collection.State = CollectionState.EndedCameAbout;
            _permissionService.SetModified(collection);
        }

        await _dataContext.SaveChangesAsync();
        await _collectionService.AddStateChangedMessages(decree.Collections);

        await transaction.CommitAsync();
    }

    public async Task CameNotAbout(Guid id, CollectionCameNotAboutReason reason, DateOnly sensitiveDataExpiryDate)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        ValidateSensitiveDataExpiryDate(sensitiveDataExpiryDate);
        var decree = await _decreeRepository.Query()
                             .WhereCanFinish(_permissionService)
                             .AsTracking()
                             .Include(x => x.Collections)
                             .ThenInclude(x => x.Permissions)
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(typeof(InitiativeEntity), id);

        decree.SensitiveDataExpiryDate = sensitiveDataExpiryDate;
        decree.State = DecreeState.EndedCameNotAbout;
        decree.CameNotAboutReason = reason;
        _permissionService.SetModified(decree);

        foreach (var collection in decree.Collections)
        {
            collection.State = CollectionState.EndedCameNotAbout;
            _permissionService.SetModified(collection);
        }

        await _dataContext.SaveChangesAsync();
        await _collectionService.AddStateChangedMessages(decree.Collections);

        await transaction.CommitAsync();
    }

    public async IAsyncEnumerable<IFile> GetDocuments(Guid id, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var decree = await _decreeRepository.Query()
                         .WhereCanGenerateDocuments(_permissionService)
                         .Include(x => x.Collections)
                         .ThenInclude(x => x.Permissions)
                         .Include(x => x.Collections)
                         .ThenInclude(x => x.Municipalities)
                         .Include(x => x.Collections)
                         .ThenInclude(x => x.CollectionCount)
                         .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                     ?? throw new EntityNotFoundException(typeof(DecreeEntity), id);

        await foreach (var file in GenerateFiles(decree, cancellationToken))
        {
            yield return file;
        }
    }

    internal void SetPeriodStateAndUserPermissions(Decree decree, DateOnly today)
    {
        decree.SetPeriodState(today);
        decree.UserPermissions = DecreePermissions.Build(_permissionService, decree);
    }

    private void SetStates(IEnumerable<Decree> decrees)
    {
        var today = _timeProvider.GetUtcTodayDateOnly();

        foreach (var decree in decrees)
        {
            SetPeriodStateAndUserPermissions(decree, today);

            foreach (var referendum in decree.Referendums)
            {
                referendum.SetPeriodState(today);
                _collectionService.LoadPermission(referendum);
                _collectionService.SetCollectionCount(referendum);
            }
        }
    }

    private void ValidateDecree(Decree decree)
    {
        if (decree.CollectionStartDate < _timeProvider.GetUtcTodayDateOnly())
        {
            throw new ValidationException("Start date can't be in the past.");
        }

        if (decree.CollectionStartDate >= decree.CollectionEndDate)
        {
            throw new ValidationException("End date needs to be past the start date.");
        }

        if (decree.DomainOfInfluenceType == DomainOfInfluenceType.Ch && !string.IsNullOrEmpty(decree.Link))
        {
            throw new ValidationException("No link allowed for federal level.");
        }
    }

    private void ValidateSensitiveDataExpiryDate(DateOnly sensitiveDataExpiryDate)
    {
        if (sensitiveDataExpiryDate <= _timeProvider.GetUtcTodayDateOnly())
        {
            throw new ValidationException("Sensitive data expiry date must be in the future.");
        }
    }

    private async Task SetBfsAndSignatureCounts(Decree decree)
    {
        // if this inheritance logic is adjusted, also adjust the Admin DomainOfInfluenceService.List
        var doi = await _domainOfInfluenceRepository.GetSingleByType(_permissionService.AclBfsLists, decree.DomainOfInfluenceType);

        var quorumDoi = doi;
        if (doi.Type == DomainOfInfluenceType.Mu)
        {
            quorumDoi = await _domainOfInfluenceRepository.GetCanton();
        }

        decree.Bfs = doi.Bfs!;
        decree.MinSignatureCount = doi.ReferendumMinSignatureCount.GetValueOrDefault();
        decree.MaxElectronicSignatureCount = quorumDoi.GetMaxReferendumElectronicSignatureCount(decree.MinSignatureCount);
    }

    private async Task<SecondFactorTransactionActionId> CreateDeleteActionId(Guid decreeId, bool forUpdate)
    {
        var query = _decreeRepository.Query().WhereCanDelete(_permissionService);
        if (forUpdate)
        {
            query = query.ForUpdate();
        }

        var collection = await query.FirstOrDefaultAsync(x => x.Id == decreeId)
                         ?? throw new EntityNotFoundException(nameof(DecreeEntity), decreeId);

        // no need to include anything despite the id since the decree and its collections is immutable in the state where it can be deleted.
        return SecondFactorTransactionActionId.Create(
            SecondFactorTransactionActionTypes.DeleteDecree,
            collection.Id);
    }

    private async Task LoadDomainOfInfluenceNames(List<Decree> decrees)
    {
        var domainOfInfluenceNamesByBfs = await _domainOfInfluenceRepository
            .Query()
            .Where(x => !string.IsNullOrWhiteSpace(x.Bfs) && x.Type == DomainOfInfluenceType.Mu)
            .GroupBy(x => x.Bfs)
            .ToDictionaryAsync(x => x.Key!, x => x.First().Name);
        foreach (var decree in decrees)
        {
            decree.DomainOfInfluenceName = decree.DomainOfInfluenceType switch
            {
                DomainOfInfluenceType.Mu => domainOfInfluenceNamesByBfs[decree.Bfs],
                DomainOfInfluenceType.Ct => Strings.DomainOfInfluenceName_Ct,
                DomainOfInfluenceType.Ch => Strings.DomainOfInfluenceName_Ch,
                _ => throw new InvalidOperationException($"Unexpected {nameof(DomainOfInfluenceType)}: {decree.DomainOfInfluenceType}"),
            };
        }
    }

    private async IAsyncEnumerable<IFile> GenerateFiles(DecreeEntity decree, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var collectionIds = decree.Collections.Select(x => x.Id).ToHashSet();
        yield return _statisticalDataCsvGenerator.GenerateFile(new StatisticalDataTemplateData(collectionIds, decree.Description, decree.Id));
        yield return await _statisticalDataTimeLapseCsvGenerator.GenerateFile(new StatisticalDataTimeLapseTemplateData(collectionIds, decree.Description));
        if (decree.DomainOfInfluenceType is DomainOfInfluenceType.Mu)
        {
            yield break;
        }

        var accessControlListDoi = await _coreDomainOfInfluenceService.GetWithChildren(decree.Bfs);
        var templateData = new ECollectingProtocolTemplateData(
            decree.Collections.Cast<CollectionBaseEntity>().ToList(),
            accessControlListDoi,
            decree.Description,
            decree.DomainOfInfluenceType,
            true);
        yield return await _officialJournalPublicationProtocolGenerator.GenerateFileModel(templateData, cancellationToken);

        foreach (var collection in decree.Collections)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return await _electronicSignaturesProtocolGenerator.GenerateFileModel(new ECollectingProtocolTemplateData([collection], accessControlListDoi, collection.Description, collection.DomainOfInfluenceType!.Value, true), cancellationToken);
        }
    }
}
