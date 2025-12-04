// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Shared.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.ECollecting.Shared.Domain.Models;

namespace Voting.ECollecting.Shared.Core.Services.Documents;

public abstract class CollectionSignatureSheetGenerationService(
    IAccessControlListDoiRepository accessControlListDoiRepository,
    IInitiativeCommitteeMemberService initiativeCommitteeService,
    IInitiativeSignatureSheetTemplateGenerator initiativeSignatureSheetTemplateGenerator,
    IReferendumSignatureSheetTemplateGenerator referendumSignatureSheetTemplateGenerator)
    : ICollectionSignatureSheetGenerationService
{
    public async Task<FileEntity> GenerateSignatureSheetFile(Guid collectionId, CollectionType collectionType)
    {
        switch (collectionType)
        {
            case CollectionType.Initiative:
                return await GenerateInitiativeSignatureSheet(collectionId);
            case CollectionType.Referendum:
                return await GenerateReferendumSignatureSheet(collectionId);
            default:
                throw new InvalidOperationException($"Unexpected collection type: {collectionType}");
        }
    }

    protected abstract IQueryable<InitiativeEntity> GetInitiativeQueryable();

    protected abstract IQueryable<ReferendumEntity> GetReferendumQueryable();

    private async Task<FileEntity> GenerateInitiativeSignatureSheet(Guid collectionId)
    {
        var queryable = GetInitiativeQueryable();

        var initiative = await queryable
                   .Include(x => x.CommitteeMembers)
                   .Include(x => x.Image!.Content)
                   .Include(x => x.Logo!.Content)
                   .FirstOrDefaultAsync(x => x.Id == collectionId)
               ?? throw new EntityNotFoundException(nameof(InitiativeEntity), collectionId);

        var domainOfInfluencesByBfs = await accessControlListDoiRepository.Query()
            .Where(x => !string.IsNullOrWhiteSpace(x.Bfs))
            .GroupBy(x => x.Bfs)
            .ToDictionaryAsync(x => x.Key!, x => x.First());

        var committeeMembers = initiative.CommitteeMembers
            .Select(x => initiativeCommitteeService.EnrichCommitteeMember(x, domainOfInfluencesByBfs))
            .Where(x => x.ApprovalState == InitiativeCommitteeMemberApprovalState.Approved)
            .OrderBy(x => x.SortIndex);

        return await initiativeSignatureSheetTemplateGenerator.GenerateFile(new InitiativeTemplateData(initiative, committeeMembers));
    }

    private async Task<FileEntity> GenerateReferendumSignatureSheet(Guid collectionId)
    {
        var queryable = GetReferendumQueryable();

        var referendum = await queryable
                   .Include(x => x.Decree)
                   .Include(x => x.Image!.Content)
                   .Include(x => x.Logo!.Content)
                   .FirstOrDefaultAsync(x => x.Id == collectionId)
               ?? throw new EntityNotFoundException(nameof(ReferendumEntity), collectionId);

        return await referendumSignatureSheetTemplateGenerator.GenerateFile(referendum);
    }
}
