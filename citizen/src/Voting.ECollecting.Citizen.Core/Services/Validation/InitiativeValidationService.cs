// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Citizen.Core.Configuration;
using Voting.ECollecting.Citizen.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using DomainEnums = Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Core.Services.Validation;

public class InitiativeValidationService : CollectionValidationService
{
    private readonly IAccessControlListDoiRepository _accessControlListDoiRepository;
    private readonly IInitiativeCommitteeMemberRepository _committeeMemberRepository;
    private readonly IFileRepository _fileRepository;
    private readonly CoreAppConfig _config;

    public InitiativeValidationService(
        IAccessControlListDoiRepository accessControlListDoiRepository,
        IInitiativeCommitteeMemberRepository committeeMemberRepository,
        ICollectionPermissionRepository collectionPermissionRepository,
        IFileRepository fileRepository,
        CoreAppConfig config)
        : base(collectionPermissionRepository)
    {
        _accessControlListDoiRepository = accessControlListDoiRepository;
        _committeeMemberRepository = committeeMemberRepository;
        _fileRepository = fileRepository;
        _config = config;
    }

    public override ValidationResult ValidateGeneralInformation(CollectionBaseEntity collection)
    {
        var validationResult = base.ValidateGeneralInformation(collection);
        return validationResult with
        {
            IsValid = validationResult.IsValid && !string.IsNullOrEmpty(((InitiativeEntity)collection).Wording),
        };
    }

    public override async Task<ValidationSummary> ValidateForSubmission(CollectionBaseEntity collection)
    {
        var validationResults = (await base.ValidateForSubmission(collection)).ValidationResults.ToList();

        var initiative = (InitiativeEntity)collection;
        var requiredApprovedMembersCount = await _accessControlListDoiRepository.Query()
                                               .Where(x => x.Bfs == initiative.Bfs)
                                               .Select(x => x.ECollectingInitiativeNumberOfMembersCommittee)
                                               .FirstOrDefaultAsync()
                                           ?? _config.InitiativeCommitteeMinApprovedMembersCount;

        var approvedMembersCount = await _committeeMemberRepository.Query()
            .CountAsync(x =>
                x.InitiativeId == initiative.Id &&
                (x.ApprovalState == InitiativeCommitteeMemberApprovalState.Approved ||
                 x.ApprovalState == InitiativeCommitteeMemberApprovalState.Signed));

        var hasAnyCommitteeMemberUploadedSignature = await _committeeMemberRepository.Query()
            .AnyAsync(x =>
                x.InitiativeId == initiative.Id &&
                x.SignatureType == InitiativeCommitteeMemberSignatureType.UploadedSignature);
        if (hasAnyCommitteeMemberUploadedSignature)
        {
            var hasCommitteeListsUploaded = await _fileRepository.Query()
                .AnyAsync(x => x.CommitteeListOfInitiativeId == initiative.Id);

            validationResults.Add(new ValidationResult(DomainEnums.Validation.CommitteeListUploaded, hasCommitteeListsUploaded));
        }

        var approvedCommitteeMembersMinValid = approvedMembersCount >= requiredApprovedMembersCount;
        if (initiative.DomainOfInfluenceType == DomainOfInfluenceType.Ch)
        {
            var approvedCommitteeMembersMaxValid = approvedMembersCount <= _config.InitiativeCommitteeMaxApprovedMembersCount;
            validationResults.Add(new ValidationResult(DomainEnums.Validation.ApprovedCommitteeMembersMaxValid, approvedCommitteeMembersMaxValid));
        }

        return new ValidationSummary([
            .. validationResults,
            new ValidationResult(DomainEnums.Validation.ApprovedCommitteeMembersMinValid, approvedCommitteeMembersMinValid),
        ]);
    }
}
