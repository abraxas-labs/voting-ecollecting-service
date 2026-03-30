// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingStimmregister;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Admin.Core.Extensions;
using Voting.ECollecting.Admin.Core.Permissions;
using Voting.ECollecting.Admin.Core.Resources;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Admin.Domain.Queries;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Core.Permissions;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.ECollecting.Shared.Domain.Models;
using IPermissionService = Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam.IPermissionService;
using IVotingStimmregisterPersonInfo = Voting.ECollecting.Admin.Domain.Models.IVotingStimmregisterPersonInfo;

namespace Voting.ECollecting.Admin.Core.Services;

public class InitiativeCommitteeService : IInitiativeCommitteeService
{
    private readonly IDomainOfInfluenceRepository _domainOfInfluenceRepository;
    private readonly IInitiativeRepository _initiativeRepository;
    private readonly IInitiativeCommitteeMemberRepository _committeeMemberRepository;
    private readonly CoreAppConfig _config;
    private readonly IDataContext _dataContext;
    private readonly IPermissionService _permissionService;
    private readonly IVotingStimmregisterAdapter _stimmregister;
    private readonly IInitiativeCommitteeMemberService _initiativeCommitteeMemberService;
    private readonly ICollectionMessageRepository _collectionMessageRepository;
    private readonly IUserNotificationService _userNotificationService;

    public InitiativeCommitteeService(
        IInitiativeRepository initiativeRepository,
        IInitiativeCommitteeMemberRepository committeeMemberRepository,
        CoreAppConfig config,
        IDataContext dataContext,
        IPermissionService permissionService,
        IVotingStimmregisterAdapter stimmregister,
        IInitiativeCommitteeMemberService initiativeCommitteeMemberService,
        IDomainOfInfluenceRepository domainOfInfluenceRepository,
        ICollectionMessageRepository collectionMessageRepository,
        IUserNotificationService userNotificationService)
    {
        _initiativeRepository = initiativeRepository;
        _committeeMemberRepository = committeeMemberRepository;
        _config = config;
        _dataContext = dataContext;
        _permissionService = permissionService;
        _stimmregister = stimmregister;
        _initiativeCommitteeMemberService = initiativeCommitteeMemberService;
        _domainOfInfluenceRepository = domainOfInfluenceRepository;
        _collectionMessageRepository = collectionMessageRepository;
        _userNotificationService = userNotificationService;
    }

    public async Task<InitiativeCommittee> GetCommittee(Guid initiativeId)
    {
        var domainOfInfluencesByBfs = await _domainOfInfluenceRepository
            .Query()
            .Where(x => !string.IsNullOrWhiteSpace(x.Bfs))
            .GroupBy(x => x.Bfs)
            .ToDictionaryAsync(x => x.Key!, x => x.First());

        var committee = await _initiativeRepository.Query()
                            .WhereCanReadCommittee(_permissionService)
                            .AsSplitQuery()
                            .Include(x => x.CommitteeMembers)
                            .ThenInclude(m => m.Permission)
                            .Include(x => x.CommitteeMembers)
                            .ThenInclude(m => m.Initiative)
                            .Where(x => x.Id == initiativeId)
                            .Select(x => new InitiativeCommittee(
                                x.Bfs!,
                                x.CommitteeLists.OrderByDescending(y => y.AuditInfo.CreatedAt).ToList(),
                                x.CommitteeMembers.OrderBy(y => y.SortIndex)
                                    .ThenBy(y => y.PoliticalLastName)
                                    .Select(y => _initiativeCommitteeMemberService.EnrichCommitteeMember(y, domainOfInfluencesByBfs)).ToList()))
                            .FirstOrDefaultAsync()
                        ?? throw new EntityNotFoundException(nameof(InitiativeEntity), initiativeId);
        committee.RequiredApprovedMembersCount = await _domainOfInfluenceRepository.Query()
                                                     .Where(x => x.Bfs == committee.Bfs)
                                                     .Select(x => x.InitiativeNumberOfMembersCommittee)
                                                     .FirstOrDefaultAsync()
                                                 ?? _config.InitiativeCommitteeMinApprovedMembersCount;
        SetUserPermissions(committee);
        return committee;
    }

    public async Task<FileEntity> GetCommitteeList(Guid initiativeId, Guid fileId)
    {
        return await _initiativeRepository.Query()
                   .WhereCanReadCommittee(_permissionService)
                   .Where(x => x.Id == initiativeId)
                   .Include(x => x.CommitteeLists).ThenInclude(x => x.Content)
                   .SelectMany(x => x.CommitteeLists)
                   .FirstOrDefaultAsync(x => x.Id == fileId)
               ?? throw new EntityNotFoundException(nameof(FileEntity), new { initiativeId, fileId });
    }

    public async Task ResetCommitteeMember(Guid initiativeId, Guid id)
    {
        var initiative = await _initiativeRepository.Query()
                             .WhereCanEditCommittee(_permissionService)
                             .AsTracking()
                             .IncludeApprovedOrRejectedCommitteeMember(id)
                             .FirstOrDefaultAsync(x => x.Id == initiativeId)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), initiativeId);
        var member = initiative.CommitteeMembers.FirstOrDefault()
                     ?? throw new EntityNotFoundException(nameof(InitiativeCommitteeMemberEntity), new { initiativeId, id });

        member.ApprovalState = InitiativeCommitteeMemberApprovalState.Requested;

        if (!member.SortIndex.HasValue)
        {
            var currentMaxIndex = await _committeeMemberRepository.Query()
                .Where(x => x.InitiativeId == initiativeId)
                .MaxAsync(x => x.SortIndex) ?? -1;

            member.SortIndex = currentMaxIndex + 1;
        }

        _permissionService.SetModified(member);
        await _dataContext.SaveChangesAsync();
    }

    public async Task<IVotingStimmregisterPersonInfo> VerifyCommitteeMember(Guid initiativeId, Guid id)
    {
        var initiative = await _initiativeRepository.Query()
                             .WhereCanEditCommittee(_permissionService)
                             .AsTracking()
                             .IncludeRequestedOrSignedCommitteeMember(id)
                             .FirstOrDefaultAsync(x => x.Id == initiativeId)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), initiativeId);
        var member = initiative.CommitteeMembers.FirstOrDefault()
                     ?? throw new EntityNotFoundException(nameof(InitiativeCommitteeMemberEntity), new { initiativeId, id });

        return await _stimmregister.GetPersonInfo(new VotingStimmregisterPersonFilterData(
            member.Bfs,
            member.LastName,
            member.FirstName,
            member.DateOfBirth));
    }

    public async Task ApproveCommitteeMember(Guid initiativeId, Guid id)
    {
        var initiative = await _initiativeRepository.Query()
                             .WhereCanEditCommittee(_permissionService)
                             .AsTracking()
                             .IncludeRequestedOrSignedCommitteeMember(id)
                             .FirstOrDefaultAsync(x => x.Id == initiativeId)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), initiativeId);
        var member = initiative.CommitteeMembers.FirstOrDefault()
                     ?? throw new EntityNotFoundException(nameof(InitiativeCommitteeMemberEntity), new { initiativeId, id });

        member.ApprovalState = InitiativeCommitteeMemberApprovalState.Approved;
        _permissionService.SetModified(member);
        await _dataContext.SaveChangesAsync();
    }

    public async Task RejectCommitteeMember(Guid initiativeId, Guid id)
    {
        var initiative = await _initiativeRepository.Query()
                             .WhereCanEditCommittee(_permissionService)
                             .AsTracking()
                             .IncludeRequestedOrSignedCommitteeMember(id)
                             .FirstOrDefaultAsync(x => x.Id == initiativeId)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), initiativeId);
        var member = initiative.CommitteeMembers.FirstOrDefault()
                     ?? throw new EntityNotFoundException(nameof(InitiativeCommitteeMemberEntity), new { initiativeId, id });

        if (member.SortIndex.HasValue)
        {
            await _committeeMemberRepository.AuditedUpdateRange(
                q => q.Where(x => x.InitiativeId == initiativeId && x.SortIndex > member.SortIndex).OrderBy(y => y.SortIndex),
                x => --x.SortIndex);
            member.SortIndex = null;
        }

        member.ApprovalState = InitiativeCommitteeMemberApprovalState.Rejected;
        _permissionService.SetModified(member);
        await _dataContext.SaveChangesAsync();
    }

    public async Task UpdateCommitteeMember(Guid initiativeId, Guid id, UpdateCommitteeMemberParams updateParams)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var initiative = await _initiativeRepository.Query()
                             .WhereCanEditCommittee(_permissionService)
                             .AsTracking()
                             .Include(x => x.CommitteeMembers.Where(m => m.Id == id))
                             .Include(x => x.Permissions)
                             .FirstOrDefaultAsync(x => x.Id == initiativeId)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), initiativeId);
        var member = initiative.CommitteeMembers.FirstOrDefault()
                     ?? throw new EntityNotFoundException(nameof(InitiativeCommitteeMemberEntity), new { initiativeId, id });

        var changedFields = GetChangedFields(member, updateParams);
        if (changedFields == InitiativeCommitteeMemberFields.None)
        {
            return;
        }

        member.PoliticalFirstName = updateParams.PoliticalFirstName;
        member.PoliticalLastName = updateParams.PoliticalLastName;
        member.PoliticalResidence = updateParams.PoliticalResidence;
        member.PoliticalDuty = updateParams.PoliticalDuty;
        _permissionService.SetModified(member);
        await _dataContext.SaveChangesAsync();

        var content = string.Format(
            Strings.UserNotification_CommitteeMemberUpdated,
            changedFields.ToLocalizedString(),
            member.FirstName,
            member.LastName);
        var msg = new CollectionMessageEntity { Content = content, CollectionId = initiative.Id };
        _permissionService.SetCreated(msg);
        await _collectionMessageRepository.Create(msg);
        await _userNotificationService.ScheduleNotification(initiative, UserNotificationType.MessageAdded);
        await transaction.CommitAsync();
    }

    private static InitiativeCommitteeMemberFields GetChangedFields(InitiativeCommitteeMemberEntity existing, UpdateCommitteeMemberParams updateParams)
    {
        var changedFields = InitiativeCommitteeMemberFields.None;

        if (updateParams.PoliticalFirstName != existing.PoliticalFirstName)
        {
            changedFields |= InitiativeCommitteeMemberFields.PoliticalFirstName;
        }

        if (updateParams.PoliticalLastName != existing.PoliticalLastName)
        {
            changedFields |= InitiativeCommitteeMemberFields.PoliticalLastName;
        }

        if (updateParams.PoliticalResidence != existing.PoliticalResidence)
        {
            changedFields |= InitiativeCommitteeMemberFields.PoliticalResidence;
        }

        if (updateParams.PoliticalDuty != existing.PoliticalDuty)
        {
            changedFields |= InitiativeCommitteeMemberFields.PoliticalDuty;
        }

        return changedFields;
    }

    private void SetUserPermissions(InitiativeCommittee committee)
    {
        foreach (var member in committee.CommitteeMembers)
        {
            SetUserPermissions(member);
        }
    }

    private void SetUserPermissions(InitiativeCommitteeMember member)
    {
        member.UserPermissions = InitiativeCommitteeMemberPermissions.Build(member);
    }
}
