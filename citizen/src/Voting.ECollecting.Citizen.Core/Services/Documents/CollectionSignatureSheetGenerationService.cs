// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Citizen.Core.Services.Documents;

public class CollectionSignatureSheetGenerationService : Voting.ECollecting.Shared.Core.Services.Documents.CollectionSignatureSheetGenerationService
{
    private readonly IInitiativeRepository _initiativeRepository;
    private readonly IReferendumRepository _referendumRepository;

    public CollectionSignatureSheetGenerationService(
        IDomainOfInfluenceRepository domainOfInfluenceRepository,
        IInitiativeCommitteeMemberService initiativeCommitteeService,
        IInitiativeSignatureSheetTemplateGenerator initiativeSignatureSheetTemplateGenerator,
        IReferendumSignatureSheetTemplateGenerator referendumSignatureSheetTemplateGenerator,
        IInitiativeRepository initiativeRepository,
        IReferendumRepository referendumRepository)
        : base(
            domainOfInfluenceRepository,
            initiativeCommitteeService,
            initiativeSignatureSheetTemplateGenerator,
            referendumSignatureSheetTemplateGenerator)
    {
        _initiativeRepository = initiativeRepository;
        _referendumRepository = referendumRepository;
    }

    protected override IQueryable<InitiativeEntity> GetInitiativeQueryable() => _initiativeRepository.Query();

    protected override IQueryable<ReferendumEntity> GetReferendumQueryable() => _referendumRepository.Query();
}
