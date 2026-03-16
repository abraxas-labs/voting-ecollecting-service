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

public class DomainOfInfluenceRepository(DataContext context, ILogger<DomainOfInfluenceRepository> logger)
    : DbRepository<DataContext, DomainOfInfluenceEntity>(context), IDomainOfInfluenceRepository
{
    public async Task<string> GetNameByBfs(DomainOfInfluenceType type, string bfs)
    {
        var result = await Query()
            .Where(x => x.Type == type && x.Bfs == bfs)
            .Select(x => x.Name)
            .Distinct()
            .Take(2)
            .ToListAsync();
        return GetSingle(result, type);
    }

    public async Task<string> GetSingleBfsByType(AclBfsLists aclBfsLists, DomainOfInfluenceType type)
    {
        if (type == DomainOfInfluenceType.Mu)
        {
            return GetSingle(aclBfsLists.BfsMunicipalities, type);
        }

        var result = await Query()
            .Where(x => x.Type == type
                        && !string.IsNullOrEmpty(x.Bfs)
                        && aclBfsLists.Bfs.Contains(x.Bfs))
            .Select(x => x.Bfs!)
            .Distinct()
            .Take(2)
            .ToListAsync();
        return GetSingle(result, type);
    }

    public async Task<DomainOfInfluenceEntity> GetSingleByType(AclBfsLists aclBfsLists, DomainOfInfluenceType type)
    {
        var dois = await Query()
            .Where(x => x.Type == type
                        && !string.IsNullOrEmpty(x.Bfs)
                        && aclBfsLists.Bfs.Contains(x.Bfs))
            .Take(2)
            .ToListAsync();
        return GetSingle(dois, type);
    }

    public async Task<DomainOfInfluenceEntity> GetSingleWithLogoContentsByType(AclBfsLists aclBfsLists, DomainOfInfluenceType type)
    {
        var dois = await Query()
            .Where(x => x.Type == type
                        && !string.IsNullOrEmpty(x.Bfs)
                        && aclBfsLists.Bfs.Contains(x.Bfs))
            .Include(x => x.Logo!.Content)
            .Take(2)
            .ToListAsync();
        return GetSingle(dois, type);
    }

    public async Task<DomainOfInfluenceEntity> GetCanton()
    {
        var dois = await Query()
            .Where(x => x.Type == DomainOfInfluenceType.Ct)
            .Distinct()
            .Take(2)
            .ToListAsync();
        return GetSingle(dois, DomainOfInfluenceType.Ct);
    }

    private T GetSingle<T>(IReadOnlyCollection<T> items, DomainOfInfluenceType doiType)
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
