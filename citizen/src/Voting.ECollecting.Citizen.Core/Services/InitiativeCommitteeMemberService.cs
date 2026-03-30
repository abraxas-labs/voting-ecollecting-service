// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Citizen.Abstractions.Adapter.VotingStimmregister;
using Voting.ECollecting.Citizen.Core.Configuration;
using Voting.ECollecting.Citizen.Core.Exceptions;
using Voting.ECollecting.Citizen.Core.Mappings;
using Voting.ECollecting.Citizen.Core.Permissions;
using Voting.ECollecting.Citizen.Domain.Models;
using Voting.ECollecting.Citizen.Domain.Queries;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.ECollecting.Shared.Domain.Queries;
using Voting.Lib.Common;
using IInitiativeCommitteeMemberService = Voting.ECollecting.Citizen.Abstractions.Core.Services.IInitiativeCommitteeMemberService;
using IPermissionService = Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin.IPermissionService;

namespace Voting.ECollecting.Citizen.Core.Services;

public class InitiativeCommitteeMemberService : IInitiativeCommitteeMemberService
{
    private readonly IDomainOfInfluenceRepository _domainOfInfluenceRepository;
    private readonly IInitiativeRepository _initiativeRepository;
    private readonly IInitiativeCommitteeMemberRepository _committeeMemberRepository;
    private readonly ICollectionPermissionRepository _collectionPermissionRepository;
    private readonly IDataContext _dataContext;
    private readonly IPermissionService _permissionService;
    private readonly CollectionPermissionService _collectionPermissionService;
    private readonly CoreAppConfig _config;
    private readonly TimeProvider _timeProvider;
    private readonly IUserNotificationService _userNotificationService;
    private readonly IVotingStimmregisterAdapter _stimmregister;
    private readonly Shared.Abstractions.Core.Services.IInitiativeCommitteeMemberService _initiativeCommitteeMemberService;

    public InitiativeCommitteeMemberService(
        IInitiativeRepository initiativeRepository,
        IInitiativeCommitteeMemberRepository committeeMemberRepository,
        ICollectionPermissionRepository collectionPermissionRepository,
        IDataContext dataContext,
        IPermissionService permissionService,
        CollectionPermissionService collectionPermissionService,
        CoreAppConfig config,
        TimeProvider timeProvider,
        IUserNotificationService userNotificationService,
        IVotingStimmregisterAdapter stimmregister,
        Shared.Abstractions.Core.Services.IInitiativeCommitteeMemberService initiativeCommitteeMemberService,
        IDomainOfInfluenceRepository domainOfInfluenceRepository)
    {
        _initiativeRepository = initiativeRepository;
        _committeeMemberRepository = committeeMemberRepository;
        _collectionPermissionRepository = collectionPermissionRepository;
        _dataContext = dataContext;
        _permissionService = permissionService;
        _collectionPermissionService = collectionPermissionService;
        _config = config;
        _timeProvider = timeProvider;
        _userNotificationService = userNotificationService;
        _stimmregister = stimmregister;
        _initiativeCommitteeMemberService = initiativeCommitteeMemberService;
        _domainOfInfluenceRepository = domainOfInfluenceRepository;
    }

    public async Task<InitiativeCommittee> GetCommittee(Guid initiativeId)
    {
        var domainOfInfluencesByBfs = await _domainOfInfluenceRepository.Query()
            .Where(x => !string.IsNullOrWhiteSpace(x.Bfs))
            .GroupBy(x => x.Bfs)
            .ToDictionaryAsync(x => x.Key!, x => x.First());

        var committee = await _initiativeRepository.Query()
                            .WhereCanRead(_permissionService)
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

    public async Task<InitiativeCommitteeMemberEntity> AddCommitteeMember(
        InitiativeCommitteeMemberEntity member,
        CollectionPermissionRole? role)
    {
        if (string.IsNullOrWhiteSpace(member.Email))
        {
            member.Email = null;
        }

        member.SetInitialValues();
        member.SetToken(_timeProvider.GetUtcNowDateTime() + _config.InitiativeCommitteeMemberTokenLifetime);

        await using var transaction = await _dataContext.BeginTransaction();

        var initiative = await _initiativeRepository.Query()
                             .WhereCanEdit(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == member.InitiativeId)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), member.InitiativeId);

        if (initiative.LockedFields.CommitteeMembers)
        {
            throw new CannotEditLockedFieldException(nameof(initiative.CommitteeMembers));
        }

        if (!initiative.IsElectronicSubmission && member.MemberSignatureRequested)
        {
            throw new ValidationException(
                "Cannot request member signature if initiative is not submitted electronically.");
        }

        var currentMaxIndex = await _committeeMemberRepository.Query()
            .Where(x => x.InitiativeId == member.InitiativeId)
            .MaxAsync(x => x.SortIndex)
            ?? -1;
        member.SortIndex = currentMaxIndex + 1;
        _permissionService.SetCreated(member);

        if (role.HasValue)
        {
            member.Permission = await _collectionPermissionService.CreatePermissionInternal(
                initiative,
                member.FirstName,
                member.LastName,
                member.Email ?? throw new ValidationException("If a role is set, an email is required"),
                role.Value);
        }

        await _committeeMemberRepository.Create(member);

        _permissionService.SetModified(initiative);
        await _dataContext.SaveChangesAsync();
        await SendCommitteeMemberAddedNotification(initiative, member);
        await transaction.CommitAsync();
        return member;
    }

    public async Task UpdateCommitteeMember(
        InitiativeCommitteeMemberEntity member,
        CollectionPermissionRole? newRole)
    {
        if (string.IsNullOrWhiteSpace(member.Email))
        {
            member.Email = null;
        }

        // set initial states
        // even though this is an update.
        // the update is only possible if the existing member is in an initial state
        // (checked later by checking CanEdit)
        member.SetInitialValues();

        await using var transaction = await _dataContext.BeginTransaction();

        var initiative = await _initiativeRepository.Query()
                             .WhereCanEdit(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == member.InitiativeId)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), member.InitiativeId);

        if (initiative.LockedFields.CommitteeMembers)
        {
            throw new CannotEditLockedFieldException(nameof(initiative.CommitteeMembers));
        }

        if (!initiative.IsElectronicSubmission && member.MemberSignatureRequested)
        {
            throw new ValidationException(
                "Cannot request member signature if initiative is not submitted electronically.");
        }

        var existingMemberEntity = await _committeeMemberRepository.Query()
                                 .AsTracking()
                                 .Where(x => x.Id == member.Id && x.InitiativeId == member.InitiativeId)
                                 .Include(x => x.Permission)
                                 .Include(x => x.Initiative)
                                 .FirstOrDefaultAsync()
                             ?? throw new EntityNotFoundException(nameof(InitiativeCommitteeMemberEntity), new { member.InitiativeId, member.Id });

        var existingMember = Mapper.MapToInitiativeCommitteeMember(existingMemberEntity);
        SetUserPermissions(existingMember);

        if (existingMember.UserPermissions?.CanEdit != true)
        {
            throw new ValidationException("Cannot edit approved or rejected member.");
        }

        var roleEdited = newRole != existingMemberEntity.Permission?.Role;
        var emailEdited = !string.Equals(existingMemberEntity.Email, member.Email, StringComparison.Ordinal);
        var memberSignatureRequestedEdited = existingMemberEntity.MemberSignatureRequested != member.MemberSignatureRequested;

        Mapper.ApplyUpdate(member, existingMemberEntity);
        if (emailEdited || roleEdited || memberSignatureRequestedEdited)
        {
            existingMemberEntity.SetToken(_timeProvider.GetUtcNowDateTime() + _config.InitiativeCommitteeMemberTokenLifetime);
        }

        _permissionService.SetModified(initiative);
        _permissionService.SetModified(existingMemberEntity);
        await _dataContext.SaveChangesAsync();

        await UpdateCommitteeMemberPermission(initiative, existingMemberEntity, newRole, emailEdited);

        await SendCommitteeMemberAddedNotification(
            initiative,
            existingMemberEntity,
            roleEdited,
            emailEdited || memberSignatureRequestedEdited);
        await transaction.CommitAsync();
    }

    public async Task UpdateCommitteeMemberPoliticalDuty(Guid initiativeId, Guid id, string politicalDuty)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var initiative = await _initiativeRepository.Query()
                             .WhereCanEditCommitteeMemberPoliticalDuty(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == initiativeId)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), initiativeId);

        if (initiative.LockedFields.CommitteeMembers)
        {
            throw new CannotEditLockedFieldException(nameof(initiative.CommitteeMembers));
        }

        var existingMemberEntity = await _committeeMemberRepository.Query()
                                       .AsTracking()
                                       .Where(x => x.Id == id && x.InitiativeId == initiativeId)
                                       .FirstOrDefaultAsync()
                                   ?? throw new EntityNotFoundException(nameof(InitiativeCommitteeMemberEntity), new { initiativeId, id });

        existingMemberEntity.PoliticalDuty = politicalDuty;
        _permissionService.SetModified(existingMemberEntity);
        _permissionService.SetModified(initiative);
        await _dataContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task RemoveCommitteeMember(Guid initiativeId, Guid id)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var initiative = await _initiativeRepository.Query()
                             .WhereCanEdit(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == initiativeId && x.CommitteeMembers.Any(m => m.Id == id))
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), initiativeId);

        if (initiative.LockedFields.CommitteeMembers)
        {
            throw new CannotEditLockedFieldException(nameof(initiative.CommitteeMembers));
        }

        _permissionService.SetModified(initiative);
        await _dataContext.SaveChangesAsync();

        var indexToRemove = await _committeeMemberRepository.Query()
            .Where(x => x.InitiativeId == initiativeId && x.Id == id)
            .Select(x => x.SortIndex)
            .FirstOrDefaultAsync()
            ?? throw new EntityNotFoundException(nameof(InitiativeCommitteeMemberEntity), new { initiativeId, id });
        await _committeeMemberRepository.AuditedDeleteRange(q => q.Where(x => x.Id == id));

        await _committeeMemberRepository.AuditedUpdateRange(
            q => q.Where(x => x.InitiativeId == initiativeId && x.SortIndex > indexToRemove).OrderBy(y => y.SortIndex),
            x => --x.SortIndex);

        await transaction.CommitAsync();
    }

    public async Task UpdateCommitteeMemberSort(Guid initiativeId, Guid id, int newIndex)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var initiative = await _initiativeRepository.Query()
                             .WhereCanEdit(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == initiativeId)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), initiativeId);

        if (initiative.LockedFields.CommitteeMembers)
        {
            throw new CannotEditLockedFieldException(nameof(initiative.CommitteeMembers));
        }

        _permissionService.SetModified(initiative);
        await _dataContext.SaveChangesAsync();

        var oldIndex = await _committeeMemberRepository.Query()
            .Where(x => x.InitiativeId == initiativeId && x.Id == id)
            .Select(x => x.SortIndex)
            .FirstOrDefaultAsync()
            ?? throw new EntityNotFoundException(nameof(InitiativeCommitteeMemberEntity), new { initiativeId, id });

        if (newIndex == oldIndex)
        {
            return;
        }

        var currentMaxIndex = await _committeeMemberRepository.Query()
            .Where(x => x.InitiativeId == initiativeId)
            .MaxAsync(x => x.SortIndex);
        if (currentMaxIndex < newIndex)
        {
            throw new ValidationException("Cannot use sort index greater than number of members");
        }

        if (newIndex < oldIndex)
        {
            // Shift up members between newIndex and oldIndex
            await _committeeMemberRepository.AuditedUpdateRange(
                q => q.Where(e => e.InitiativeId == initiativeId && e.Id != id && e.SortIndex >= newIndex && e.SortIndex < oldIndex).OrderBy(e => e.SortIndex),
                e => ++e.SortIndex);
        }
        else
        {
            // Shift down members between oldIndex and newIndex
            await _committeeMemberRepository.AuditedUpdateRange(
                q => q.Where(e => e.InitiativeId == initiativeId && e.Id != id && e.SortIndex <= newIndex && e.SortIndex > oldIndex).OrderBy(e => e.SortIndex),
                e => --e.SortIndex);
        }

        await _committeeMemberRepository.AuditedUpdateRange(
            q => q.Where(x => x.InitiativeId == initiativeId && x.Id == id).OrderBy(e => e.SortIndex),
            x => x.SortIndex = newIndex);

        await transaction.CommitAsync();
    }

    public async Task ResendCommitteeMemberInvitation(Guid initiativeId, Guid id)
    {
        var initiative = await _initiativeRepository.Query()
                             .WhereCanEdit(_permissionService)
                             .AsTracking()
                             .IncludeRequestedOrSignatureRejectedOrExpiredCommitteeMember(id)
                             .FirstOrDefaultAsync(x => x.Id == initiativeId)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), initiativeId);

        if (initiative.LockedFields.CommitteeMembers)
        {
            throw new CannotEditLockedFieldException(nameof(initiative.CommitteeMembers));
        }

        var member = initiative.CommitteeMembers.FirstOrDefault()
            ?? throw new EntityNotFoundException(nameof(InitiativeCommitteeMemberEntity), new { initiativeId, id });

        member.SetToken(_timeProvider.GetUtcNowDateTime() + _config.InitiativeCommitteeMemberTokenLifetime);
        member.ApprovalState = InitiativeCommitteeMemberApprovalState.Requested;
        if (!member.SortIndex.HasValue)
        {
            var currentMaxIndex = await _committeeMemberRepository.Query()
                .Where(x => x.InitiativeId == initiativeId)
                .MaxAsync(x => x.SortIndex) ?? -1;

            member.SortIndex = currentMaxIndex + 1;
        }

        await _dataContext.SaveChangesAsync();

        await SendCommitteeMemberAddedNotification(initiative, member);
    }

    public async Task<PendingCommitteeMembership> GetPendingCommitteeMembershipByToken(UrlToken token)
    {
        return await _committeeMemberRepository.Query()
                   .WhereIsRequestedAndCanReadWithToken(_permissionService, token)
                   .Include(x => x.Initiative!.SubType)
                   .Select(x => new PendingCommitteeMembership(
                       x.InitiativeId,
                       x.Initiative!.Description,
                       x.Initiative!.SubType,
                       x.Initiative!.Wording,
                       x.Initiative!.Reason,
                       x.Initiative!.Link,
                       x.FirstName,
                       x.LastName,
                       x.AuditInfo.CreatedByName,
                       _config.Acr.AcceptInitiativeCommitteeMembership))
                   .FirstOrDefaultAsync()
               ?? throw new EntityNotFoundException(nameof(CollectionPermissionEntity), token);
    }

    public async Task<bool> AcceptCommitteeMemberInvitation(UrlToken token)
    {
        var member = await _committeeMemberRepository.Query()
                             .WhereIsRequestedAndCanReadWithToken(_permissionService, token)
                             .Include(x => x.Initiative)
                             .AsTracking()
                             .FirstOrDefaultAsync()
                         ?? throw new EntityNotFoundException(nameof(CollectionPermissionEntity), token);

        if (!member.Initiative!.State.InPreparationOrReturnForCorrection())
        {
            throw new EntityNotFoundException(nameof(CollectionBaseEntity), member.InitiativeId);
        }

        if (member.Email != null)
        {
            _permissionService.RequireEmail(member.Email);
        }
        else
        {
            member.Email = _permissionService.UserEmail;
        }

        if (await HasVotingRight(member))
        {
            member.ApprovalState = InitiativeCommitteeMemberApprovalState.Approved;
        }
        else
        {
            member.ApprovalState = InitiativeCommitteeMemberApprovalState.Rejected;
        }

        member.SignatureType = InitiativeCommitteeMemberSignatureType.VerifiedIamIdentity;
        member.IamUserId = _permissionService.UserId;
        member.Token = null;
        member.TokenExpiry = null;
        _permissionService.SetModified(member);
        await _dataContext.SaveChangesAsync();

        return member.ApprovalState == InitiativeCommitteeMemberApprovalState.Approved;
    }

    public async Task RejectCommitteeMemberInvitation(UrlToken token)
    {
        var member = await _committeeMemberRepository.Query()
                             .WhereIsRequestedAndCanReadWithToken(_permissionService, token)
                             .AsTracking()
                             .FirstOrDefaultAsync()
                         ?? throw new EntityNotFoundException(nameof(InitiativeCommitteeMemberEntity), token);

        if (!await _initiativeRepository
                .Query()
                .WhereInPreparationOrReturnedForCorrection()
                .AnyAsync(x => x.Id == member.InitiativeId))
        {
            throw new EntityNotFoundException(nameof(InitiativeEntity), member.InitiativeId);
        }

        if (member.SortIndex.HasValue)
        {
            await _committeeMemberRepository.AuditedUpdateRange(
                q => q.Where(x => x.InitiativeId == member.InitiativeId && x.SortIndex > member.SortIndex).OrderBy(y => y.SortIndex),
                x => --x.SortIndex);
            member.SortIndex = null;
        }

        member.ApprovalState = InitiativeCommitteeMemberApprovalState.SignatureRejected;
        member.TokenExpiry = null;
        member.AuditInfo.ModifiedAt = _timeProvider.GetUtcNowDateTime();
        await _dataContext.SaveChangesAsync();
    }

    private async Task SendCommitteeMemberAddedNotification(
        InitiativeEntity initiative,
        InitiativeCommitteeMemberEntity member,
        bool sendPermissionNotification = true,
        bool sendMemberSignatureRequest = true)
    {
        UserNotificationType? notificationType = (sendPermissionNotification && member.Permission != null, sendMemberSignatureRequest && member.MemberSignatureRequested) switch
        {
            (true, false) => UserNotificationType.PermissionAdded,
            (true, true) => UserNotificationType.CommitteeMembershipAddedWithPermission,
            (false, true) => UserNotificationType.CommitteeMembershipAdded,
            (false, false) => null,
        };

        if (!notificationType.HasValue)
        {
            return;
        }

        await _userNotificationService.SendUserNotification(
            member.Email ?? throw new ValidationException("If a role or approval is set, an email is required"),
            true,
            notificationType.Value,
            new UserNotificationContext(
                Collection: initiative,
                PermissionToken: member.Permission?.Token,
                InitiativeCommitteeMembershipToken: member.Token));
    }

    private async Task UpdateCommitteeMemberPermission(
        InitiativeEntity initiative,
        InitiativeCommitteeMemberEntity member,
        CollectionPermissionRole? newRole,
        bool emailEdited)
    {
        // no new role with existing permission,
        // => delete existing permission.
        if (!newRole.HasValue && member.Permission != null)
        {
            await _collectionPermissionRepository.AuditedDelete(member.Permission);
            member.Permission = null;
            return;
        }

        // no new role and no existing permission,
        // => do nothing
        if (!newRole.HasValue)
        {
            return;
        }

        // no existing permission but a new role
        // => add new permission
        if (member.Permission == null)
        {
            member.Permission = await _collectionPermissionService.CreatePermissionInternal(
                initiative,
                member.FirstName,
                member.LastName,
                member.Email ?? throw new ValidationException("If a role is set, an email is required"),
                newRole.Value);
            return;
        }

        // existing permission, but no email change
        // => update role, keep approval state of permission
        if (!emailEdited)
        {
            await _collectionPermissionRepository.AuditedUpdate(
                member.Permission,
                () =>
                {
                    Mapper.ApplyUpdate(member, member.Permission);
                    member.Permission.Role = newRole.Value;
                    _permissionService.SetModified(member.Permission);
                });
            return;
        }

        // existing permission, email change
        // => delete and recreate permission
        await _collectionPermissionRepository.AuditedDelete(member.Permission);
        member.Permission = await _collectionPermissionService.CreatePermissionInternal(
            initiative,
            member.FirstName,
            member.LastName,
            member.Email ?? throw new ValidationException("If a role is set, an email is required"),
            newRole.Value);
    }

    private async Task<bool> HasVotingRight(InitiativeCommitteeMemberEntity member)
    {
        var userSocialSecurityNumber = await _permissionService.GetSocialSecurityNumber();
        if (userSocialSecurityNumber == null)
        {
            return false;
        }

        return await _stimmregister.HasVotingRight(
            userSocialSecurityNumber,
            member.Initiative!.DomainOfInfluenceType!.Value,
            member.Initiative!.Bfs!);
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
        member.UserPermissions = Shared.Core.Permissions.InitiativeCommitteeMemberPermissions.Build(member);
    }
}
