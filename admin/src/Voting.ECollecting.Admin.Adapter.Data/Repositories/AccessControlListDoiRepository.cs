// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Database.Repositories;

namespace Voting.ECollecting.Admin.Adapter.Data.Repositories;

/// <inheritdoc cref="IAccessControlListDoiRepository"/>
public class AccessControlListDoiRepository(DataContext context, ILogger<AccessControlListDoiRepository> logger)
    : DbRepository<DataContext, AccessControlListDoiEntity>(context), IAccessControlListDoiRepository
{
    public async Task<string> GetMunicipalityNameByBfs(AclDomainOfInfluenceType doiType, string bfs)
    {
        return await Query()
            .Where(x => x.Type == doiType && x.Bfs == bfs)
            .Select(x => x.Name)
            .FirstAsync();
    }

    public async Task<string> GetSingleBfsForDoiType(AclBfsLists aclBfsLists, AclDomainOfInfluenceType doiType)
    {
        if (doiType == AclDomainOfInfluenceType.Mu)
        {
            return GetSingle(aclBfsLists.BfsMunicipalities, doiType);
        }

        var bfs = await Query()
            .Where(x => x.Type == doiType
                        && !string.IsNullOrEmpty(x.Bfs)
                        && aclBfsLists.Bfs.Contains(x.Bfs))
            .Select(x => x.Bfs!)
            .Distinct()
            .Take(2)
            .ToListAsync();
        return GetSingle(bfs, doiType);
    }

    public async Task<AccessControlListDoiEntity> GetSingleForDoiType(AclBfsLists aclBfsLists, AclDomainOfInfluenceType doiType)
    {
        var aclDois = await Query()
            .Where(x => x.Type == doiType
                        && !string.IsNullOrEmpty(x.Bfs)
                        && aclBfsLists.Bfs.Contains(x.Bfs))
            .Take(2)
            .ToListAsync();
        return GetSingle(aclDois, doiType);
    }

    private T GetSingle<T>(IReadOnlyCollection<T> items, AclDomainOfInfluenceType doiType)
    {
        if (items.Count == 1)
        {
            return items.Single();
        }

        logger.LogWarning(
            "Tried to load single item for doi type {DoiType} but found none or more than one. This may indicate an invalid tenant/roles configuration.",
            doiType);
        throw new ValidationException(
            $"Expected exactly one item for doi type {doiType} but found none or more than one.");
    }
}
