// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.Lib.Iam.Exceptions;

namespace Voting.ECollecting.Admin.Core.Services;

public class AccessControlListService : IAccessControlListService
{
    private readonly Shared.Abstractions.Core.Services.IDomainOfInfluenceService _coreDomainOfInfluenceService;
    private readonly IDomainOfInfluenceRepository _domainOfInfluenceRepository;
    private readonly IPermissionService _permissionService;

    public AccessControlListService(
        Shared.Abstractions.Core.Services.IDomainOfInfluenceService coreDomainOfInfluenceService,
        IDomainOfInfluenceRepository domainOfInfluenceRepository,
        IPermissionService permissionService)
    {
        _coreDomainOfInfluenceService = coreDomainOfInfluenceService;
        _domainOfInfluenceRepository = domainOfInfluenceRepository;
        _permissionService = permissionService;
    }

    public async Task EnsureIsCtOrChTenant()
    {
        var isChOrCtTenant = await _domainOfInfluenceRepository.Query()
            .AnyAsync(x =>
                x.TenantId == _permissionService.TenantId &&
                (x.Type == DomainOfInfluenceType.Ct || x.Type == DomainOfInfluenceType.Ch));
        if (!isChOrCtTenant)
        {
            throw new ForbiddenException("Only tenants of DOIs of type CT or CH are allowed to perform this operation.");
        }
    }

    public async Task<AclBfsLists> GetBfsNumberAccessControlListsByTenantId(string tenantId)
    {
        var allAcls = await _coreDomainOfInfluenceService.GetTree();

        var assignedDois = allAcls.Where(a => a.TenantId == tenantId).ToList();
        var assignedBfs = assignedDois
            .Select(x => x.Bfs)
            .WhereNotNull()
            .ToHashSet();
        var assignedBfsMunicipality = assignedDois
            .Where(x => x.Type == DomainOfInfluenceType.Mu)
            .Select(x => x.Bfs)
            .WhereNotNull()
            .ToHashSet();

        var assignedBfsMunicipalityInclParents = assignedDois
            .Where(x => x.Type == DomainOfInfluenceType.Mu)
            .SelectMany(x => x.GetFlattenParentsInclSelf())
            .Select(x => x.Bfs)
            .WhereNotNull()
            .ToHashSet();

        var assignedBfsInclChildren = assignedDois
            .SelectMany(x => x.GetFlattenChildrenInclSelf())
            .Select(x => x.Bfs)
            .WhereNotNull()
            .ToHashSet();

        var assignedBfsInclChildrenAndParents = assignedDois
            .SelectMany(x => x.GetFlattenParentsInclSelf())
            .Select(x => x.Bfs)
            .WhereNotNull()
            .Concat(assignedBfsInclChildren)
            .ToHashSet();

        var parentsBfs = assignedDois
            .SelectMany(x => x.GetFlattenParents())
            .Select(x => x.Bfs)
            .WhereNotNull()
            .ToHashSet();

        return new AclBfsLists(
            assignedBfs,
            assignedBfsMunicipality,
            assignedBfsMunicipalityInclParents,
            assignedBfsInclChildren,
            assignedBfsInclChildrenAndParents,
            parentsBfs);
    }

    internal async Task<List<DomainOfInfluenceEntity>> GetMunicipalities(string bfs)
    {
        var tree = await _coreDomainOfInfluenceService.GetTree();
        return tree
            .Where(x => x.Bfs == bfs)
            .SelectMany(x => x.GetFlattenChildrenInclSelf())
            .Where(x => x.Type == DomainOfInfluenceType.Mu && !string.IsNullOrEmpty(x.Bfs))
            .OrderBy(x => x.Bfs)
            .ToList();
    }
}
