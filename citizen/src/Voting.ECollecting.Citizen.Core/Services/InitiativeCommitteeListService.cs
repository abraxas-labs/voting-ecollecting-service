// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Citizen.Abstractions.Core.Services;
using Voting.ECollecting.Citizen.Core.Configuration;
using Voting.ECollecting.Citizen.Core.Exceptions;
using Voting.ECollecting.Citizen.Core.Permissions;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.ECollecting.Shared.Domain.Queries;
using Voting.Lib.Common;
using Voting.Lib.Common.Files;
using IDomainOfInfluenceRepository = Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories.IDomainOfInfluenceRepository;
using IInitiativeCommitteeMemberService = Voting.ECollecting.Shared.Abstractions.Core.Services.IInitiativeCommitteeMemberService;
using IInitiativeRepository = Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories.IInitiativeRepository;
using IPermissionService = Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin.IPermissionService;

namespace Voting.ECollecting.Citizen.Core.Services;

public class InitiativeCommitteeListService : IInitiativeCommitteeListService
{
    private readonly IFileRepository _fileRepository;
    private readonly IInitiativeCommitteeMemberRepository _committeeMemberRepository;
    private readonly IInitiativeRepository _initiativeRepository;
    private readonly IDomainOfInfluenceRepository _domainOfInfluenceRepository;
    private readonly IPermissionService _permissionService;
    private readonly IDataContext _dataContext;
    private readonly IFileService _fileService;
    private readonly ICommitteeListTemplateGenerator _committeeListTemplateGenerator;
    private readonly TimeProvider _timeProvider;
    private readonly CoreAppConfig _config;
    private readonly IInitiativeCommitteeMemberService _initiativeCommitteeMemberService;

    public InitiativeCommitteeListService(
        IFileRepository fileRepository,
        IInitiativeCommitteeMemberRepository committeeMemberRepository,
        IInitiativeRepository initiativeRepository,
        IPermissionService permissionService,
        IDataContext dataContext,
        IFileService fileService,
        ICommitteeListTemplateGenerator committeeListTemplateGenerator,
        TimeProvider timeProvider,
        CoreAppConfig config,
        IInitiativeCommitteeMemberService initiativeCommitteeMemberService,
        IDomainOfInfluenceRepository domainOfInfluenceRepository)
    {
        _fileRepository = fileRepository;
        _committeeMemberRepository = committeeMemberRepository;
        _initiativeRepository = initiativeRepository;
        _permissionService = permissionService;
        _dataContext = dataContext;
        _fileService = fileService;
        _committeeListTemplateGenerator = committeeListTemplateGenerator;
        _timeProvider = timeProvider;
        _config = config;
        _initiativeCommitteeMemberService = initiativeCommitteeMemberService;
        _domainOfInfluenceRepository = domainOfInfluenceRepository;
    }

    public async Task<FileEntity> AddCommitteeList(Guid initiativeId, Stream file, string? contentType, string? fileName, CancellationToken ct)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var initiative = await _initiativeRepository.Query()
                             .WhereCanEdit(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == initiativeId, ct)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), initiativeId);

        if (initiative.LockedFields.CommitteeMembers)
        {
            throw new CannotEditLockedFieldException(nameof(initiative.CommitteeMembers));
        }

        var fileEntity = await ValidateAndCreateCommitteeMemberListFile(initiativeId, file, contentType, fileName, ct);
        _permissionService.SetModified(initiative);
        await _dataContext.SaveChangesAsync();
        await transaction.CommitAsync(ct);

        return fileEntity;
    }

    public async Task AcceptCommitteeMembershipWithCommitteeList(
        Guid initiativeId,
        UrlToken token,
        Stream file,
        string contentType,
        string fileName,
        CancellationToken ct)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var member = await _committeeMemberRepository.Query()
                         .WhereIsRequestedAndCanReadWithToken(_permissionService, token)
                         .AsTracking()
                         .FirstOrDefaultAsync(x => x.InitiativeId == initiativeId, ct)
                     ?? throw new EntityNotFoundException(nameof(CollectionPermissionEntity), token);

        if (!await _initiativeRepository
                .Query()
                .WhereInPreparationOrReturnedForCorrection()
                .AnyAsync(x => x.Id == member.InitiativeId, ct))
        {
            throw new EntityNotFoundException(nameof(CollectionBaseEntity), member.InitiativeId);
        }

        var fileEntity = await ValidateAndCreateCommitteeMemberListFile(member.InitiativeId, file, contentType, fileName, ct);

        member.Token = null;
        member.TokenExpiry = null;
        member.SignatureType = InitiativeCommitteeMemberSignatureType.UploadedSignature;
        member.ApprovalState = InitiativeCommitteeMemberApprovalState.Signed;
        member.SignatureFileId = fileEntity.Id;

        await _dataContext.SaveChangesAsync();
        await transaction.CommitAsync(ct);
    }

    public async Task<FileEntity> GetCommitteeList(Guid initiativeId, Guid fileId)
    {
        return await _initiativeRepository.Query()
                   .WhereCanWrite(_permissionService)
                   .Where(x => x.Id == initiativeId)
                   .Include(x => x.CommitteeLists).ThenInclude(x => x.Content)
                   .SelectMany(x => x.CommitteeLists)
                   .FirstOrDefaultAsync(x => x.Id == fileId)
               ?? throw new EntityNotFoundException(nameof(FileEntity), new { initiativeId, fileId });
    }

    public async Task<IFile> GetCommitteeListTemplate(Guid initiativeId, CancellationToken ct)
    {
        var initiative = await _initiativeRepository.Query()
                             .WhereCanWrite(_permissionService)
                             .Where(x => x.Id == initiativeId)
                             .Include(x => x.CommitteeMembers.Where(y =>
                                 y.ApprovalState == InitiativeCommitteeMemberApprovalState.Requested ||
                                 y.ApprovalState == InitiativeCommitteeMemberApprovalState.Expired ||
                                 y.ApprovalState == InitiativeCommitteeMemberApprovalState.Signed ||
                                 y.ApprovalState == InitiativeCommitteeMemberApprovalState.Approved))
                             .Include(x => x.Permissions!.Where(y =>
                                 y.Role == CollectionPermissionRole.Deputy &&
                                 y.State == CollectionPermissionState.Accepted))
                             .Include(x => x.SubType)
                             .FirstOrDefaultAsync(x => x.Id == initiativeId, cancellationToken: ct)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), initiativeId);

        var templateData = await GetCommitteeListTemplateData(initiative);
        return await _committeeListTemplateGenerator.GenerateFileModel(templateData, ct);
    }

    public async Task<IFile> GetCommitteeListTemplateForMemberByToken(Guid initiativeId, UrlToken token, CancellationToken ct)
    {
        var initiative = await _initiativeRepository.Query()
                             .WhereCanReadWithMembershipToken(_permissionService, token)
                             .Where(x => x.Id == initiativeId)
                             .Include(x => x.CommitteeMembers.Where(y =>
                                 y.ApprovalState == InitiativeCommitteeMemberApprovalState.Requested ||
                                 y.ApprovalState == InitiativeCommitteeMemberApprovalState.Expired ||
                                 y.ApprovalState == InitiativeCommitteeMemberApprovalState.Signed ||
                                 y.ApprovalState == InitiativeCommitteeMemberApprovalState.Approved))
                             .Include(x => x.Permissions!.Where(y =>
                                 y.Role == CollectionPermissionRole.Deputy &&
                                 y.State == CollectionPermissionState.Accepted))
                             .Include(x => x.SubType)
                             .FirstOrDefaultAsync(x => x.Id == initiativeId, cancellationToken: ct)
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), initiativeId);

        var templateData = await GetCommitteeListTemplateData(initiative);
        return await _committeeListTemplateGenerator.GenerateFileModel(templateData, ct);
    }

    public async Task DeleteCommitteeList(Guid initiativeId, Guid listId)
    {
        await using var transaction = await _dataContext.BeginTransaction();

        var initiative = await _initiativeRepository.Query()
                             .WhereCanEdit(_permissionService)
                             .AsTracking()
                             .FirstOrDefaultAsync(x => x.Id == initiativeId && x.CommitteeLists.Any(c => c.Id == listId))
                         ?? throw new EntityNotFoundException(nameof(InitiativeEntity), initiativeId);

        if (initiative.LockedFields.CommitteeMembers)
        {
            throw new CannotEditLockedFieldException(nameof(initiative.CommitteeMembers));
        }

        _permissionService.SetModified(initiative);
        await _dataContext.SaveChangesAsync();

        await _fileRepository.DeleteByKey(listId);

        await transaction.CommitAsync();
    }

    private async Task<FileEntity> ValidateAndCreateCommitteeMemberListFile(
        Guid initiativeId,
        Stream file,
        [NotNull] string? contentType,
        [NotNull] string? fileName,
        CancellationToken ct)
    {
        var fileEntity = await _fileService.Validate(file, contentType, fileName, _config.AllowedCommitteeListFileFileExtensions, ct: ct);

        var timestampSuffix = TimeZoneInfo.ConvertTimeFromUtc(_timeProvider.GetUtcNowDateTime(), _config.TimeZoneInfo)
            .ToString(_config.CommitteeListFileNameSuffixDateFormat);
        fileEntity.Name = Path.GetFileNameWithoutExtension(fileEntity.Name)
                          + timestampSuffix
                          + Path.GetExtension(fileEntity.Name);
        fileEntity.CommitteeListOfInitiativeId = initiativeId;
        _permissionService.SetCreated(fileEntity);
        await _fileRepository.Create(fileEntity);
        return fileEntity;
    }

    private async Task<CommitteeListTemplateData> GetCommitteeListTemplateData(InitiativeEntity initiative)
    {
        var requiredApprovedMembersCount = await _domainOfInfluenceRepository.Query()
                                               .Where(x => x.Bfs == initiative.Bfs)
                                               .Select(x => x.InitiativeNumberOfMembersCommittee)
                                               .FirstOrDefaultAsync()
                                           ?? _config.InitiativeCommitteeMinApprovedMembersCount;

        var domainOfInfluencesByBfs = await _domainOfInfluenceRepository.Query()
            .Where(x => !string.IsNullOrWhiteSpace(x.Bfs))
            .GroupBy(x => x.Bfs)
            .ToDictionaryAsync(x => x.Key!, x => x.First());

        var committeeMembers = initiative.CommitteeMembers
            .Select(y => _initiativeCommitteeMemberService.EnrichCommitteeMember(y, domainOfInfluencesByBfs))
            .OrderBy(x => x.SortIndex)
            .ThenBy(x => x.PoliticalLastName);

        return new CommitteeListTemplateData(
            initiative,
            committeeMembers,
            requiredApprovedMembersCount,
            initiative.Permissions!,
            initiative.SubType);
    }
}
