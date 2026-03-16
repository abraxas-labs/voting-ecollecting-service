// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Citizen.Abstractions.Core.Services;
using Voting.ECollecting.Citizen.Core.Configuration;
using Voting.ECollecting.Citizen.Core.Mappings;
using Voting.ECollecting.Citizen.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Core.Services;

public class DomainOfInfluenceService : IDomainOfInfluenceService
{
    private readonly IDomainOfInfluenceRepository _domainOfInfluenceRepository;
    private readonly CoreAppConfig _config;

    public DomainOfInfluenceService(IDomainOfInfluenceRepository domainOfInfluenceRepository, CoreAppConfig config)
    {
        _domainOfInfluenceRepository = domainOfInfluenceRepository;
        _config = config;
    }

    public async Task<List<DomainOfInfluence>> List(bool? eCollectingEnabled, IReadOnlySet<DomainOfInfluenceType>? doiTypes)
    {
        IQueryable<DomainOfInfluenceEntity> query = _domainOfInfluenceRepository
            .Query()
            .OrderBy(x => x.Name);

        if (eCollectingEnabled.HasValue)
        {
            query = query.Where(x => x.ECollectingEnabled == eCollectingEnabled.Value);
        }

        query = doiTypes != null
            ? query.Where(x => doiTypes.Contains(x.Type))
            : query.Where(x => x.Type != DomainOfInfluenceType.Unspecified);

        var dois = await query.ToListAsync();

        // the MU's inherit the canton's max electronic signature percent
        if (dois.Any(x => x.Type == DomainOfInfluenceType.Mu))
        {
            var quorumDoi = await _domainOfInfluenceRepository.GetSingleByType(DomainOfInfluenceType.Ct);
            foreach (var doi in dois.Where(x => x.Type == DomainOfInfluenceType.Mu))
            {
                doi.InitiativeMaxElectronicSignaturePercent = quorumDoi.InitiativeMaxElectronicSignaturePercent;
                doi.ReferendumMaxElectronicSignaturePercent = quorumDoi.ReferendumMaxElectronicSignaturePercent;
            }
        }

        return Mapper.MapToDomainOfInfluences(dois).ToList();
    }

    public IEnumerable<DomainOfInfluenceType> ListDomainOfInfluenceTypes() => _config.EnabledDomainOfInfluenceTypes.OrderBy(x => x);
}
