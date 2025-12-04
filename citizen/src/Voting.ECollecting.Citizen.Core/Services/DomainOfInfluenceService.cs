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
    private readonly IAccessControlListDoiRepository _accessControlListDoiRepository;
    private readonly CoreAppConfig _config;

    public DomainOfInfluenceService(IAccessControlListDoiRepository accessControlListDoiRepository, CoreAppConfig config)
    {
        _accessControlListDoiRepository = accessControlListDoiRepository;
        _config = config;
    }

    public async Task<List<DomainOfInfluence>> List(bool? eCollectingEnabled, IReadOnlySet<DomainOfInfluenceType>? doiTypes)
    {
        IQueryable<AccessControlListDoiEntity> query = _accessControlListDoiRepository
            .Query()
            .OrderBy(x => x.Name);

        if (eCollectingEnabled.HasValue)
        {
            query = query.Where(x => x.ECollectingEnabled == eCollectingEnabled.Value);
        }

        doiTypes ??= Enum.GetValues<DomainOfInfluenceType>().ToHashSet();
        var aclDoiTypes = Mapper.MapToAclDoiTypes(doiTypes).ToHashSet();
        query = query.Where(x => aclDoiTypes.Contains(x.Type));
        return Mapper.MapToDomainOfInfluences(await query.ToListAsync()).ToList();
    }

    public IEnumerable<DomainOfInfluenceType> ListDomainOfInfluenceTypes() => _config.EnabledDomainOfInfluenceTypes.OrderBy(x => x);
}
