// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Core.Permissions;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Domain.Entities;
using IAccessControlListDoiRepository = Voting.ECollecting.Shared.Abstractions.Adapter.Data.Repositories.IAccessControlListDoiRepository;
using IPermissionService = Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam.IPermissionService;

namespace Voting.ECollecting.Admin.Core.Services.Documents;

public class CollectionSignatureSheetGenerationService : Shared.Core.Services.Documents.CollectionSignatureSheetGenerationService
{
    private readonly IInitiativeRepository _initiativeRepository;
    private readonly IReferendumRepository _referendumRepository;
    private readonly IPermissionService _permissionService;

    public CollectionSignatureSheetGenerationService(
        IAccessControlListDoiRepository accessControlListDoiRepository,
        IInitiativeCommitteeMemberService initiativeCommitteeService,
        IInitiativeSignatureSheetTemplateGenerator initiativeSignatureSheetTemplateGenerator,
        IReferendumSignatureSheetTemplateGenerator referendumSignatureSheetTemplateGenerator,
        IInitiativeRepository initiativeRepository,
        IReferendumRepository referendumRepository,
        IPermissionService permissionService)
        : base(
            accessControlListDoiRepository,
            initiativeCommitteeService,
            initiativeSignatureSheetTemplateGenerator,
            referendumSignatureSheetTemplateGenerator)
    {
        _initiativeRepository = initiativeRepository;
        _referendumRepository = referendumRepository;
        _permissionService = permissionService;
    }

    protected override IQueryable<InitiativeEntity> GetInitiativeQueryable() => _initiativeRepository.Query().WhereCanEdit(_permissionService);

    protected override IQueryable<ReferendumEntity> GetReferendumQueryable() => _referendumRepository.Query().WhereCanEdit(_permissionService);
}
