// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Database.Repositories;

namespace Voting.ECollecting.Citizen.Adapter.Data.Repositories;

public class DomainOfInfluenceRepository(
    DataContext db,
    ILogger<DomainOfInfluenceRepository> logger) :
    DbRepository<DataContext, DomainOfInfluenceEntity>(db),
    IDomainOfInfluenceRepository
{
    public async Task<DomainOfInfluenceEntity> GetSingleByType(DomainOfInfluenceType type)
    {
        var bfs = await Query()
            .Where(x => x.Type == type)
            .Take(2)
            .ToListAsync();

        return GetSingle(bfs, type);
    }

    public async Task<string> GetSingleBfsByType(DomainOfInfluenceType type)
    {
        var bfs = await Query()
                   .Where(x => x.Type == type && !string.IsNullOrEmpty(x.Bfs))
                   .Select(x => x.Bfs!)
                   .Distinct()
                   .Take(2)
                   .ToListAsync();

        return GetSingle(bfs, type);
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
