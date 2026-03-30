// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Core.Exceptions;
using Voting.ECollecting.Admin.Core.Extensions;
using Voting.ECollecting.Admin.Core.Mappings;
using Voting.ECollecting.Admin.Core.Permissions;
using Voting.ECollecting.Admin.Core.Resources;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Admin.Domain.Queries;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using IPermissionService = Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam.IPermissionService;

namespace Voting.ECollecting.Admin.Core.Services;

public class ReferendumService : IReferendumService
{
    private readonly IDecreeRepository _decreeRepository;
    private readonly IReferendumRepository _referendumRepository;
    private readonly CollectionService _collectionService;
    private readonly DecreeService _decreeService;
    private readonly TimeProvider _timeProvider;
    private readonly IPermissionService _permissionService;
    private readonly IDataContext _dataContext;
    private readonly ICollectionMessageRepository _collectionMessageRepository;
    private readonly IUserNotificationService _userNotificationService;

    public ReferendumService(
        IDecreeRepository decreeRepository,
        IReferendumRepository referendumRepository,
        CollectionService collectionService,
        DecreeService decreeService,
        TimeProvider timeProvider,
        IPermissionService permissionService,
        IDataContext dataContext,
        ICollectionMessageRepository collectionMessageRepository,
        IUserNotificationService userNotificationService)
    {
        _decreeRepository = decreeRepository;
        _referendumRepository = referendumRepository;
        _collectionService = collectionService;
        _decreeService = decreeService;
        _timeProvider = timeProvider;
        _permissionService = permissionService;
        _dataContext = dataContext;
        _collectionMessageRepository = collectionMessageRepository;
        _userNotificationService = userNotificationService;
    }

    public async Task<Dictionary<DomainOfInfluenceType, List<Decree>>> ListDecreesByDoiType(IReadOnlySet<DomainOfInfluenceType>? doiTypes, string? bfs)
    {
        var query = _decreeRepository.Query().WhereCanReadOnReferendums(_permissionService);

        if (doiTypes?.Count > 0)
        {
            query = query.Where(d => doiTypes.Contains(d.DomainOfInfluenceType));
        }

        if (!string.IsNullOrEmpty(bfs))
        {
            query = query.Where(x => x.Bfs == bfs);
        }

        var decreesByDoiType = await query

            // include own referendums or parent referendums which are in collection or expired
            // same logic as in AclPermissions.WhereCanAccessOwnBfsOrChildrenOrParentsInPeriodStateInCollectionOrExpired
            .IncludeFilteredReferendums(_permissionService.AclBfsLists, _timeProvider.GetUtcTodayDateOnly())
            .ThenIncludeMunicipalities(_permissionService.AclBfsLists)
            .Include<DecreeEntity, List<ReferendumEntity>>(x => x.Collections)
            .ThenInclude(x => x.CollectionCount)
            .GroupBy(x => x.DomainOfInfluenceType)
            .ToDictionaryAsync(x => x.Key, x => x.OrderByDescending(y => y.CollectionStartDate).ToList());

        var today = _timeProvider.GetUtcTodayDateOnly();
        return Enum.GetValues<DomainOfInfluenceType>()
            .Where(x => x != DomainOfInfluenceType.Unspecified)
            .OrderBy(x => x)
            .ToDictionary(x => x, x =>
            {
                var decrees = Mapper.MapToDecrees(decreesByDoiType.GetValueOrDefault(x) ?? []);

                foreach (var decree in decrees)
                {
                    _decreeService.SetPeriodStateAndUserPermissions(decree, today);

                    foreach (var referendum in decree.Referendums)
                    {
                        referendum.Decree = decree;
                        referendum.SetPeriodState(today);
                        _collectionService.LoadPermission(referendum);
                        _collectionService.SetCollectionCount(referendum);
                    }
                }

                return decrees;
            });
    }

    public async Task<Referendum> Get(Guid id)
    {
        var referendumEntity = await _referendumRepository.Query()
                                   .WhereCanRead(_permissionService)
                                   .Include(x => x.Decree)
                                   .Include(x => x.CollectionCount)
                                   .IncludeMunicipalities(_permissionService.AclBfsLists)

                                   // include files but not the file content
                                   .Include(x => x.Image)
                                   .Include(x => x.Logo)
                                   .Include(x => x.SignatureSheetTemplate)
                                   .FirstOrDefaultAsync(x => x.Id == id)
                               ?? throw new EntityNotFoundException(nameof(ReferendumEntity), id);
        var referendum = Mapper.MapToReferendum(referendumEntity);
        referendum.SetPeriodState(_timeProvider.GetUtcTodayDateOnly());
        _collectionService.LoadPermission(referendum);
        _collectionService.SetCollectionCount(referendum);
        return referendum;
    }

    public async Task<Guid> Create(Guid decreeId, string description, CollectionAddress collectionAddress)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var decree = await _decreeRepository.Query()
                         .WhereCanAddCollection(_permissionService)
                         .Include(x => x.Collections)
                         .FirstOrDefaultAsync(x => x.Id == decreeId)
                     ?? throw new EntityNotFoundException(nameof(DecreeEntity), decreeId);

        if (decree.Collections.Any(x => x.Description == description))
        {
            throw new CollectionAlreadyExistsException();
        }

        var referendum = new ReferendumEntity
        {
            Description = description,
            DecreeId = decreeId,
            CollectionCount = new CollectionCountEntity(),
            SignatureSheetTemplateGenerated = true,
            Type = CollectionType.Referendum,
            IsElectronicSubmission = false,
            State = CollectionState.PreRecorded,
            Address = collectionAddress,

            // decree data
            Bfs = decree.Bfs,
            DomainOfInfluenceType = decree.DomainOfInfluenceType,
            MaxElectronicSignatureCount = decree.MaxElectronicSignatureCount,
            CollectionStartDate = decree.CollectionStartDate,
            CollectionEndDate = decree.CollectionEndDate,
        };

        _permissionService.SetCreated(referendum);
        _permissionService.SetCreated(referendum.CollectionCount);
        await _collectionService.CreateWithSecretIdNumber(referendum);
        await _collectionService.PrepareForCollection(referendum);
        await _dataContext.SaveChangesAsync();

        await transaction.CommitAsync();
        return referendum.Id;
    }

    public async Task Update(Guid id, UpdateReferendumParams parameters)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var referendum = await _referendumRepository
                             .Query()
                             .WhereCanEditGeneralInformation(_permissionService)
                             .AsTracking()
                             .Include(x => x.Permissions)
                             .FirstOrDefaultAsync(x => x.Id == id)
                         ?? throw new EntityNotFoundException(nameof(ReferendumEntity), id);

        var changedFields = GetChangedFields(referendum, parameters);

        if (changedFields == ReferendumFields.None)
        {
            return;
        }

        referendum.Description = parameters.Description ?? referendum.Description;
        referendum.Reason = parameters.Reason ?? referendum.Reason;
        referendum.MembersCommittee = parameters.MembersCommittee ?? referendum.MembersCommittee;
        referendum.Link = parameters.Link ?? referendum.Link;
        referendum.Address = parameters.Address ?? referendum.Address;

        _permissionService.SetModified(referendum);
        await _dataContext.SaveChangesAsync();

        if (referendum.State != CollectionState.PreRecorded)
        {
            await AddGeneralInformationChangedMessage(referendum, changedFields);
        }

        await transaction.CommitAsync();
    }

    private static ReferendumFields GetChangedFields(ReferendumEntity existing, UpdateReferendumParams parameters)
    {
        var changedFields = ReferendumFields.None;

        if (parameters.Description != null && parameters.Description != existing.Description)
        {
            changedFields |= ReferendumFields.Description;
        }

        if (parameters.Reason != null && parameters.Reason != existing.Reason)
        {
            changedFields |= ReferendumFields.Reason;
        }

        if (parameters.MembersCommittee != null && parameters.MembersCommittee != existing.MembersCommittee)
        {
            changedFields |= ReferendumFields.MembersCommittee;
        }

        if (parameters.Link != null && parameters.Link != existing.Link)
        {
            changedFields |= ReferendumFields.Link;
        }

        if (parameters.Address != null && !existing.Address.Equals(parameters.Address))
        {
            changedFields |= ReferendumFields.Address;
        }

        return changedFields;
    }

    private async Task AddGeneralInformationChangedMessage(ReferendumEntity referendum, ReferendumFields changedFields)
    {
        var content = string.Format(Strings.UserNotification_GeneralInformationChanged, changedFields.ToLocalizedString());
        var msg = new CollectionMessageEntity { Content = content, CollectionId = referendum.Id };
        _permissionService.SetCreated(msg);
        await _collectionMessageRepository.Create(msg);
        await _userNotificationService.ScheduleNotification(referendum, UserNotificationType.MessageAdded);
    }
}
