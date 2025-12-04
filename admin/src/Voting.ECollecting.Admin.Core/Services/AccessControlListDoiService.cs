// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Extensions;

namespace Voting.ECollecting.Admin.Core.Services;

public class AccessControlListDoiService : IAccessControlListDoiService
{
    private readonly Shared.Abstractions.Core.Services.IAccessControlListDoiService _coreAccessControlListDoiService;

    public AccessControlListDoiService(
        Shared.Abstractions.Core.Services.IAccessControlListDoiService coreAccessControlListDoiService)
    {
        _coreAccessControlListDoiService = coreAccessControlListDoiService;
    }

    public async Task<AclBfsLists> GetBfsNumberAccessControlListsByTenantId(string tenantId)
    {
        var allAcls = await _coreAccessControlListDoiService.GetTree();

        var assignedAcls = allAcls.Where(a => a.TenantId == tenantId).ToList();
        var assignedBfs = assignedAcls
            .Select(x => x.Bfs)
            .WhereNotNull()
            .ToHashSet();
        var assignedBfsMunicipality = assignedAcls
            .Where(x => x.Type == AclDomainOfInfluenceType.Mu)
            .Select(x => x.Bfs)
            .WhereNotNull()
            .ToHashSet();

        var assignedBfsMunicipalityInclParents = assignedAcls
            .Where(x => x.Type == AclDomainOfInfluenceType.Mu)
            .SelectMany(x => x.GetFlattenParentsInclSelf())
            .Select(x => x.Bfs)
            .WhereNotNull()
            .ToHashSet();

        var assignedBfsInclChildren = assignedAcls
            .SelectMany(x => x.GetFlattenChildrenInclSelf())
            .Select(x => x.Bfs)
            .WhereNotNull()
            .ToHashSet();

        var assignedBfsInclChildrenAndParents = assignedAcls
            .SelectMany(x => x.GetFlattenParentsInclSelf())
            .Select(x => x.Bfs)
            .WhereNotNull()
            .Concat(assignedBfsInclChildren)
            .ToHashSet();

        return new AclBfsLists(
            assignedBfs,
            assignedBfsMunicipality,
            assignedBfsMunicipalityInclParents,
            assignedBfsInclChildren,
            assignedBfsInclChildrenAndParents);
    }

    public async Task<List<AccessControlListDoiEntity>> GetMunicipalities(string bfs)
    {
        var allAcls = await _coreAccessControlListDoiService.GetTree();
        return allAcls
            .Where(x => x.Bfs == bfs)
            .SelectMany(x => x.GetFlattenChildrenInclSelf())
            .Where(x => x.Type == AclDomainOfInfluenceType.Mu && !string.IsNullOrEmpty(x.Bfs))
            .OrderBy(x => x.Bfs)
            .ToList();
    }
}
