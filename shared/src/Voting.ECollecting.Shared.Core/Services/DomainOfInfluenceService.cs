// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Shared.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Core.Services;

public class DomainOfInfluenceService : IDomainOfInfluenceService
{
    private readonly IDomainOfInfluenceRepository _domainOfInfluenceRepository;

    public DomainOfInfluenceService(IDomainOfInfluenceRepository domainOfInfluenceRepository)
    {
        _domainOfInfluenceRepository = domainOfInfluenceRepository;
    }

    public async Task<DomainOfInfluenceEntity> GetWithChildren(string bfs)
    {
        var aclTree = await GetTree();
        return aclTree.Single(x => x.Bfs == bfs);
    }

    public async Task<List<DomainOfInfluenceEntity>> GetTree()
    {
        var allAcls = await _domainOfInfluenceRepository.Query().ToListAsync();
        var byId = allAcls.ToDictionary(x => x.Id);

        foreach (var acl in allAcls)
        {
            var parent = acl.ParentId.HasValue
                ? byId.GetValueOrDefault(acl.ParentId.Value)
                : null;
            acl.Parent = parent;
            parent?.Children.Add(acl);
        }

        return allAcls;
    }
}
