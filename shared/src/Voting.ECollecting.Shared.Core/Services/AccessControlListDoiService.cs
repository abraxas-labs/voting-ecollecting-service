// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Shared.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Core.Services;

public class AccessControlListDoiService : IAccessControlListDoiService
{
    private readonly IAccessControlListDoiRepository _accessControlListDoiRepository;

    public AccessControlListDoiService(IAccessControlListDoiRepository accessControlListDoiRepository)
    {
        _accessControlListDoiRepository = accessControlListDoiRepository;
    }

    public async Task<AccessControlListDoiEntity> GetAccessControlListDoiWithChildren(string bfs)
    {
        var aclTree = await GetTree();
        return aclTree.Single(x => x.Bfs == bfs);
    }

    public async Task<List<AccessControlListDoiEntity>> GetTree()
    {
        var allAcls = await _accessControlListDoiRepository.Query().ToListAsync();
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
