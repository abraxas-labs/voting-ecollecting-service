// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Common.Files;

namespace Voting.ECollecting.Admin.Core.Services.Documents;

public class StatisticalDataCsvGenerator : CsvGenerator<StatisticalDataCsvEntry, CollectionBaseEntity>, IStatisticalDataCsvGenerator
{
    private readonly TimeProvider _timeProvider;
    private readonly CsvConfig _config;
    private readonly ICollectionCitizenRepository _collectionCitizenRepository;

    public StatisticalDataCsvGenerator(
        CsvService csvService,
        TimeProvider timeProvider,
        CsvConfig config,
        ICollectionCitizenRepository collectionCitizenRepository)
        : base(csvService)
    {
        _timeProvider = timeProvider;
        _config = config;
        _collectionCitizenRepository = collectionCitizenRepository;
    }

    public IFile GenerateFile(CollectionBaseEntity collection)
    {
        var citizens = _collectionCitizenRepository.Query()
            .Where(x => x.CollectionMunicipality!.CollectionId == collection.Id && (!x.SignatureSheetId.HasValue || x.SignatureSheet!.State >= CollectionSignatureSheetState.Submitted))
            .Include(x => x.CollectionMunicipality)
            .Include(x => x.SignatureSheet)
            .OrderBy(x => x.CollectionMunicipality!.MunicipalityName)
            .ThenBy(x => x.Id)
            .ThenBy(x => x.CollectionDateTime)
            .Select(x => new StatisticalDataCsvEntry
            {
                CollectionId = collection.Id,
                Age = x.Age,
                Sex = x.Sex,
                Bfs = x.CollectionMunicipality!.Bfs,
                MunicipalityName = x.CollectionMunicipality.MunicipalityName,
                ElectronicSignatureDate = x.Electronic ? x.CollectionDateTime : null,
                PhysicalReceivedAt = !x.Electronic ? x.SignatureSheet!.ReceivedAt : null,
            })
            .AsAsyncEnumerable();

        return GenerateFile(collection, citizens);
    }

    protected override string BuildFileName(CollectionBaseEntity rootEntity) => string.Format(_config.StatisticalDataCsvFileName, rootEntity.Description, _timeProvider.GetSwissDateTime().ToString("yyyy-MM-dd-HHmmss"));
}
