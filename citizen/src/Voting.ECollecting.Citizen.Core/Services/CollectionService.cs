// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Citizen.Abstractions.Core.Services;
using Voting.ECollecting.Citizen.Core.Configuration;
using Voting.ECollecting.Citizen.Core.Mappings;
using Voting.ECollecting.Citizen.Core.Permissions;
using Voting.ECollecting.Citizen.Core.Services.Signature;
using Voting.ECollecting.Citizen.Domain.Models;
using Voting.ECollecting.Citizen.Domain.Queries;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.ECollecting.Shared.Domain.Queries;
using IPermissionService = Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin.IPermissionService;
using Strings = Voting.ECollecting.Shared.Core.Resources.Strings;

namespace Voting.ECollecting.Citizen.Core.Services;

public class CollectionService : ICollectionService
{
    private readonly IInitiativeRepository _initiativeRepository;
    private readonly IDecreeRepository _decreeRepository;
    private readonly ICollectionMessageRepository _messageRepository;
    private readonly Shared.Abstractions.Core.Services.IUserNotificationService _coreUserNotificationService;
    private readonly IDataContext _dataContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly ICollectionRepository _collectionRepository;
    private readonly IPermissionService _permissionService;
    private readonly CollectionSignService _collectionSignService;
    private readonly TimeProvider _timeProvider;
    private readonly CoreAppConfig _config;

    public CollectionService(
        IInitiativeRepository initiativeRepository,
        IDecreeRepository decreeRepository,
        ICollectionMessageRepository messageRepository,
        ICollectionRepository collectionRepository,
        TimeProvider timeProvider,
        CoreAppConfig config,
        IPermissionService permissionService,
        Shared.Abstractions.Core.Services.IUserNotificationService coreUserNotificationService,
        IDataContext dataContext,
        IServiceProvider serviceProvider,
        CollectionSignService collectionSignService)
    {
        _initiativeRepository = initiativeRepository;
        _decreeRepository = decreeRepository;
        _timeProvider = timeProvider;
        _config = config;
        _permissionService = permissionService;
        _coreUserNotificationService = coreUserNotificationService;
        _dataContext = dataContext;
        _serviceProvider = serviceProvider;
        _collectionSignService = collectionSignService;
        _collectionRepository = collectionRepository;
        _messageRepository = messageRepository;
    }

    public async Task<Dictionary<DomainOfInfluenceType, CollectionsGroup>> ListByDoiType(CollectionPeriodState periodState, IReadOnlySet<DomainOfInfluenceType>? doiTypes, string? bfs)
    {
        var now = _timeProvider.GetUtcNowDateTime();
        var decreeQuery = _decreeRepository
            .Query()
            .WhereInPeriodState(periodState, now)
            .WhereDoiTypeIsEnabled(_config.EnabledDomainOfInfluenceTypes);

        if (doiTypes?.Count > 0)
        {
            decreeQuery = decreeQuery.Where(x => doiTypes.Contains(x.DomainOfInfluenceType));
        }

        if (!string.IsNullOrEmpty(bfs))
        {
            decreeQuery = decreeQuery.Where(x => x.Bfs == bfs);
        }

        var decreeEntities = await decreeQuery
            .Include(x => x.Collections.Where(c => (c.State == CollectionState.EnabledForCollection
                                                    || c.State == CollectionState.SignatureSheetsSubmitted
                                                    || c.State == CollectionState.EndedCameAbout
                                                    || c.State == CollectionState.EndedCameNotAbout)
                                                   && c.IsElectronicSubmission)
                .OrderBy(y => y.AuditInfo.CreatedAt).ThenBy(y => y.Description))
            .ThenIncludePermission(_permissionService.UserId)
            .Include(x => x.Collections)
            .ThenInclude(x => x.SignatureSheetTemplate)
            .Include(x => x.Collections)
            .ThenInclude(x => x.CollectionCount)
            .Where(x => x.Collections.Any(c => (c.State == CollectionState.EnabledForCollection
                                                || c.State == CollectionState.SignatureSheetsSubmitted
                                                || c.State == CollectionState.EndedCameAbout
                                                || c.State == CollectionState.EndedCameNotAbout)
                                               && c.IsElectronicSubmission))
            .GroupBy(x => x.DomainOfInfluenceType)
            .ToDictionaryAsync(x => x.Key, x => x.OrderByDescending(y => y.CollectionStartDate).ThenBy(y => y.Description).ToList());

        var decrees = decreeEntities.ToDictionary(
            x => x.Key,
            x => Mapper.MapToDecrees(x.Value));
        foreach (var decree in decrees.Values.SelectMany(x => x))
        {
            decree.SetPeriodState(now);

            foreach (var referendum in decree.Referendums)
            {
                referendum.SetPeriodState(now);
                SetCollectionCount(referendum);
            }
        }

        var initiativeQuery = _initiativeRepository
            .Query()
            .Include(x => x.CollectionCount)
            .Include(x => x.SignatureSheetTemplate)
            .IncludePermission(_permissionService.UserId)
            .WhereDoiTypeIsEnabled(_config.EnabledDomainOfInfluenceTypes)
            .Where(x => x.DomainOfInfluenceType.HasValue)
            .WhereInPeriodState(periodState, false, now)
            .WhereIsEnabledForCollectionOrEnded()
            .WhereIsElectronicSubmission();

        if (doiTypes?.Count > 0)
        {
            initiativeQuery = initiativeQuery.Where(x => doiTypes.Contains(x.DomainOfInfluenceType!.Value));
        }

        if (!string.IsNullOrEmpty(bfs))
        {
            initiativeQuery = initiativeQuery.Where(x => x.Bfs == bfs);
        }

        var initiativeEntities = await initiativeQuery
            .GroupBy(x => x.DomainOfInfluenceType)
            .ToDictionaryAsync(x => x.Key!.Value, x => x.OrderByDescending(y => y.CollectionStartDate).ThenBy(y => y.Description).ToList());

        var initiatives = initiativeEntities.ToDictionary(
            x => x.Key,
            x => Mapper.MapToInitiatives(x.Value));
        foreach (var initiative in initiatives.Values.SelectMany(x => x))
        {
            initiative.SetPeriodState(now);
            SetCollectionCount(initiative);
        }

        if (_permissionService.IsAuthenticated)
        {
            await _collectionSignService.ResolveSigned(initiatives, decrees);
        }

        return _config.EnabledDomainOfInfluenceTypes
            .OrderBy(x => x)
            .ToDictionary(x => x, x => new CollectionsGroup(
                initiatives.GetValueOrDefault(x) ?? [],
                decrees.GetValueOrDefault(x) ?? []));
    }

    public async Task<(List<CollectionMessageEntity> Messages, bool InformalReviewRequested)> ListMessages(Guid collectionId)
    {
        var collection = await _collectionRepository.Query()
                         .WhereCanRead(_permissionService)
                         .Include(x => x.Messages!.OrderBy(y => y.AuditInfo.CreatedAt))
                         .FirstOrDefaultAsync(x => x.Id == collectionId)
                     ?? throw new EntityNotFoundException(typeof(CollectionBaseEntity), collectionId);
        return (collection.Messages!, collection.InformalReviewRequested);
    }

    public async Task<CollectionMessageEntity> AddMessage(Guid collectionId, string content)
    {
        var collection = await _collectionRepository
                             .Query()
                             .WhereCanCreateMessages(_permissionService)
                             .Include(x => x.Permissions)
                             .FirstOrDefaultAsync(x => x.Id == collectionId)
                         ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), collectionId);

        var msg = new CollectionMessageEntity { Content = content, CollectionId = collectionId };
        _permissionService.SetCreated(msg);
        await _messageRepository.Create(msg);
        await _coreUserNotificationService.ScheduleNotification(collection, UserNotificationType.MessageAdded);
        return msg;
    }

    public async Task<CollectionMessageEntity> UpdateRequestInformalReview(Guid id, bool requestInformalReview)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var collection = await _collectionRepository.Query()
                             .WhereCanRequestInformalReview(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), id);

        if (collection.InformalReviewRequested && requestInformalReview)
        {
            throw new ValidationException($"Informal review is already requested for this collection {id}");
        }

        if (!collection.InformalReviewRequested && !requestInformalReview)
        {
            throw new ValidationException($"Informal review is already withdrawn for this collection {id}");
        }

        collection.InformalReviewRequested = requestInformalReview;
        _permissionService.SetModified(collection);
        await _dataContext.SaveChangesAsync();

        var content = requestInformalReview
            ? Strings.UserNotification_InformalReviewRequested
            : Strings.UserNotification_InformalReviewWithdrawn;
        var msg = await AddMessage(collection.Id, content);

        await transaction.CommitAsync();
        return msg;
    }

    public async Task Withdraw(Guid id)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var collection = await _collectionRepository.Query()
                             .WhereCanWithdraw(_permissionService)
                             .Include(x => x.Permissions)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), id);

        collection.State = CollectionState.Withdrawn;
        _permissionService.SetModified(collection);
        await _dataContext.SaveChangesAsync();

        await AddStateChangedMessage(collection);

        await transaction.CommitAsync();
    }

    public async Task<ValidationSummary> Validate(Guid id)
    {
        var collection = await _collectionRepository.Query()
                             .WhereCanEdit(_permissionService)
                             .Include(x => x.Permissions)
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(CollectionBaseEntity), id);

        var validationService = _serviceProvider.GetRequiredKeyedService<ICollectionValidationService>(collection.Type);
        return await validationService.ValidateForSubmission(collection);
    }

    internal void LoadPermission<T>(T collection)
        where T : CollectionBaseEntity, ICollection
    {
        var validationService = _serviceProvider.GetRequiredKeyedService<ICollectionValidationService>(collection.Type);
        var generalInformationIsValid = validationService.ValidateGeneralInformation(collection).IsValid;
        collection.UserPermissions = CollectionPermissions.Build(
            _permissionService,
            collection,
            generalInformationIsValid);
    }

    internal async Task AddStateChangedMessage(CollectionBaseEntity collection)
    {
        var key = $"{collection.State.GetType().Name}.{collection.State.ToString()}";
        var localizedStateValue = Strings.ResourceManager.GetString(key);

        var content = string.Format(Strings.UserNotification_StateChanged, localizedStateValue);
        var msg = new CollectionMessageEntity { Content = content, CollectionId = collection.Id };
        _permissionService.SetCreated(msg);
        await _messageRepository.Create(msg);
        await _coreUserNotificationService.ScheduleNotification(collection, UserNotificationType.StateChanged);
    }

    /// <summary>
    /// Sets the collection count on the domain collection.
    /// It is set to the electronic count if the collection state is enabled or later and not withdrawn.
    /// If it has ended, the total count is set too.
    /// </summary>
    /// <param name="collection">The collection.</param>
    /// <typeparam name="T">The type of collection.</typeparam>
    internal void SetCollectionCount<T>(T collection)
        where T : CollectionBaseEntity, ICollection
    {
        if (collection.CollectionCount == null)
        {
            return;
        }

        if (!collection.State.IsEnabledForCollectionOrEnded())
        {
            collection.AttestedCollectionCount = null;
            return;
        }

        if (!collection.State.IsEndedCameAboutOrCameNotAbout())
        {
            collection.AttestedCollectionCount = new NullableCollectionCount
            {
                Id = collection.CollectionCount.Id,
                CollectionId = collection.CollectionCount.CollectionId,
                ElectronicCitizenCount = collection.CollectionCount.ElectronicCitizenCount,
            };
            return;
        }

        collection.AttestedCollectionCount = new NullableCollectionCount
        {
            Id = collection.CollectionCount.Id,
            CollectionId = collection.CollectionCount.CollectionId,
            ElectronicCitizenCount = collection.CollectionCount.ElectronicCitizenCount,
            TotalCitizenCount = collection.CollectionCount.TotalCitizenCount,
        };
    }
}
