// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingStimmregister;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Admin.Core.Permissions;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Admin.Domain.Queries;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.ECollecting.Shared.Domain.Models;
using IVotingStimmregisterPersonInfo = Voting.ECollecting.Admin.Domain.Models.IVotingStimmregisterPersonInfo;

namespace Voting.ECollecting.Admin.Core.Services;

public class InitiativeCommitteeService : IInitiativeCommitteeService
{
    private readonly IAccessControlListDoiRepository _accessControlListDoiRepository;
    private readonly IInitiativeRepository _initiativeRepository;
    private readonly CoreAppConfig _config;
    private readonly IDataContext _dataContext;
    private readonly IPermissionService _permissionService;
    private readonly IVotingStimmregisterAdapter _stimmregister;
    private readonly Shared.Abstractions.Core.Services.IInitiativeCommitteeMemberService _initiativeCommitteeMemberService;

    public InitiativeCommitteeService(
        IAccessControlListDoiRepository accessControlListDoiRepository,
        IInitiativeRepository initiativeRepository,
        CoreAppConfig config,
        IDataContext dataContext,
        IPermissionService permissionService,
        IVotingStimmregisterAdapter stimmregister,
        Shared.Abstractions.Core.Services.IInitiativeCommitteeMemberService initiativeCommitteeMemberService)
    {
        _accessControlListDoiRepository = accessControlListDoiRepository;
        _initiativeRepository = initiativeRepository;
        _config = config;
        _dataContext = dataContext;
        _permissionService = permissionService;
        _stimmregister = stimmregister;
        _initiativeCommitteeMemberService = initiativeCommitteeMemberService;
    }

    public async Task<InitiativeCommittee> GetCommittee(Guid initiativeId)
    {
        var domainOfInfluencesByBfs = await _accessControlListDoiRepository
            .Query()
            .Where(x => !string.IsNullOrWhiteSpace(x.Bfs))
            .GroupBy(x => x.Bfs)
            .ToDictionaryAsync(x => x.Key!, x => x.First());

        var committee = await _initiativeRepository.Query()
                            .WhereCanReadCommittee(_permissionService)
                            .AsSplitQuery()
                            .Include(x => x.CommitteeMembers)
                            .ThenInclude(m => m.Permission)
                            .Where(x => x.Id == initiativeId)
                            .Select(x => new InitiativeCommittee(
                                x.Bfs!,
                                x.CommitteeLists.OrderByDescending(y => y.AuditInfo.CreatedAt).ToList(),
                                x.CommitteeMembers.OrderBy(y => y.SortIndex).Select(y => _initiativeCommitteeMemberService.EnrichCommitteeMember(y, domainOfInfluencesByBfs)).ToList()))
                            .FirstOrDefaultAsync()
                        ?? throw new EntityNotFoundException(nameof(InitiativeEntity), initiativeId);
        committee.RequiredApprovedMembersCount = await _accessControlListDoiRepository.Query()
                                                     .Where(x => x.Bfs == committee.Bfs)
                                                     .Select(x => x.ECollectingInitiativeNumberOfMembersCommittee)
                                                     .FirstOrDefaultAsync()
                                                 ?? _config.InitiativeCommitteeMinApprovedMembersCount;
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
                             .IncludeUploadedApprovedOrRejectedCommitteeMember(id)
                             .FirstOrDefaultAsync(x => x.Id == initiativeId)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), initiativeId);
        var member = initiative.CommitteeMembers.FirstOrDefault()
                     ?? throw new EntityNotFoundException(nameof(InitiativeCommitteeMemberEntity), new { initiativeId, id });

        member.ApprovalState = InitiativeCommitteeMemberApprovalState.Requested;
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

        member.ApprovalState = InitiativeCommitteeMemberApprovalState.Rejected;
        _permissionService.SetModified(member);
        await _dataContext.SaveChangesAsync();
    }
}
