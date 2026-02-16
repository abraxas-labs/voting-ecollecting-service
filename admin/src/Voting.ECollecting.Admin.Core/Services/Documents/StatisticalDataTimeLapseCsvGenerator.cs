// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Core.Models;
using Voting.ECollecting.Admin.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.Lib.Common.Files;

namespace Voting.ECollecting.Admin.Core.Services.Documents;

public class StatisticalDataTimeLapseCsvGenerator :
    CsvGenerator<StatisticalDataTimeLapseCsvEntry, StatisticalDataTimeLapseTemplateData>,
    IStatisticalDataTimeLapseCsvGenerator
{
    private readonly CsvConfig _config;
    private readonly TimeProvider _timeProvider;
    private readonly ICollectionCitizenRepository _collectionCitizenRepository;
    private readonly ICollectionSignatureSheetRepository _collectionSignatureSheetRepository;
    private readonly ICollectionRepository _collectionRepository;

    public StatisticalDataTimeLapseCsvGenerator(
        CsvService csvService,
        CsvConfig config,
        TimeProvider timeProvider,
        ICollectionCitizenRepository collectionCitizenRepository,
        ICollectionSignatureSheetRepository collectionSignatureSheetRepository,
        ICollectionRepository collectionRepository)
        : base(csvService)
    {
        _config = config;
        _timeProvider = timeProvider;
        _collectionCitizenRepository = collectionCitizenRepository;
        _collectionSignatureSheetRepository = collectionSignatureSheetRepository;
        _collectionRepository = collectionRepository;
    }

    public async Task<IFile> GenerateFile(StatisticalDataTimeLapseTemplateData data)
    {
        // load electronic citizens first
        var electronicCitizens = await _collectionCitizenRepository.Query()
            .Where(x => data.CollectionIds.Contains(x.CollectionMunicipality!.CollectionId)
                        && !x.SignatureSheetId.HasValue)
            .GroupBy(x => new { Date = DateOnly.FromDateTime(x.CollectionDateTime), x.CollectionMunicipality!.MunicipalityName })
            .Select(g => new
            {
                g.Key.Date,
                g.Key.MunicipalityName,
                ElectronicCitizenCount = g.Count(),
                PhysicalValidCount = 0,
                PhysicalInvalidCount = 0,
            })
            .ToListAsync();

        // load the sheets separately, since SQL cannot perform distinct queries with a group by clause efficiently
        var sheets = await _collectionSignatureSheetRepository.Query()
            .Where(s => data.CollectionIds.Contains(s.CollectionMunicipality!.CollectionId) &&
                        s.State >= CollectionSignatureSheetState.Submitted)
            .GroupBy(s => new { Date = s.ReceivedAt, s.CollectionMunicipality!.MunicipalityName })
            .Select(g => new
            {
                g.Key.Date,
                g.Key.MunicipalityName,
                ElectronicCitizenCount = 0,
                PhysicalValidCount = g.Sum(s => s.Count.Valid),
                PhysicalInvalidCount = g.Sum(s => s.Count.Invalid),
            })
            .ToListAsync();

        // concatenate both
        var rows = electronicCitizens.Concat(sheets)
            .GroupBy(x => new { x.Date, x.MunicipalityName })
            .Select(g => new StatisticalDataTimeLapseAggregateData(
                g.Key.Date,
                g.Key.MunicipalityName,
                g.Sum(x => x.ElectronicCitizenCount),
                g.Sum(x => x.PhysicalValidCount),
                g.Sum(x => x.PhysicalInvalidCount)))
            .OrderBy(x => x.Date)
            .ThenBy(x => x.MunicipalityName)
            .ToList();

        var maxElectronicSignatureCountReachedDateTime = await GetMaxElectronicSignatureCountReachedDateTime(data.CollectionIds);

        // calculate the cumulative sum in memory since the required SQL window function cannot be translated
        return GenerateFile(data, CalculateCumulativeSum(rows, maxElectronicSignatureCountReachedDateTime));
    }

    protected override string BuildFileName(StatisticalDataTimeLapseTemplateData data) => string.Format(
        _config.StatisticalDataTimeLapseCsvFileName,
        data.Description,
        _timeProvider.GetSwissDateTime().ToString("yyyy-MM-dd-HHmmss"));

    private IEnumerable<StatisticalDataTimeLapseCsvEntry> CalculateCumulativeSum(
        IEnumerable<StatisticalDataTimeLapseAggregateData> rows,
        DateTime? maxElectronicSignatureCountReachedDateTime)
    {
        var cumulativeTotals = new Dictionary<string, (int ElectronicCitizenCount, int PhysicalValidCount, int PhysicalInvalidCount)>();

        foreach (var row in rows)
        {
            if (!cumulativeTotals.TryGetValue(row.MunicipalityName, out var totals))
            {
                totals = (0, 0, 0);
            }

            totals = (
                ElectronicCitizenCount: totals.ElectronicCitizenCount + row.ElectronicCitizenCount,
                PhysicalValidCount: totals.PhysicalValidCount + row.PhysicalValidCount,
                PhysicalInvalidCount: totals.PhysicalInvalidCount + row.PhysicalInvalidCount);

            cumulativeTotals[row.MunicipalityName] = totals;

            yield return new StatisticalDataTimeLapseCsvEntry
            {
                Date = row.Date,
                MunicipalityName = row.MunicipalityName,
                ElectronicCitizenCount = totals.ElectronicCitizenCount,
                ValidPhysicalSignatureCount = totals.PhysicalValidCount,
                InvalidPhysicalSignatureCount = totals.PhysicalInvalidCount,
                DateMaxElectronicSignatureCountReached =
                    maxElectronicSignatureCountReachedDateTime.HasValue &&
                    row.Date == DateOnly.FromDateTime(maxElectronicSignatureCountReachedDateTime.Value)
                        ? maxElectronicSignatureCountReachedDateTime.Value
                        : null,
            };
        }
    }

    private async Task<DateTime?> GetMaxElectronicSignatureCountReachedDateTime(HashSet<Guid> collectionIds)
    {
        var collectionId = collectionIds.First();
        var maxElectronicSignatureCount = await _collectionRepository.Query()
                                              .Where(x => x.Id == collectionId)
                                              .Select(x => x.MaxElectronicSignatureCount)
                                              .FirstOrDefaultAsync()
                                          ?? throw new EntityNotFoundException(typeof(CollectionBaseEntity), collectionId);

        var electronicCitizenCount = await _collectionRepository.Query()
            .Where(x => collectionIds.Contains(x.Id))
            .SumAsync(x => x.CollectionCount!.ElectronicCitizenCount);

        if (electronicCitizenCount < maxElectronicSignatureCount)
        {
            return null;
        }

        return await _collectionCitizenRepository.Query()
            .Where(x => collectionIds.Contains(x.CollectionMunicipality!.CollectionId) && !x.SignatureSheetId.HasValue)
            .MaxAsync(x => x.CollectionDateTime);
    }
}
