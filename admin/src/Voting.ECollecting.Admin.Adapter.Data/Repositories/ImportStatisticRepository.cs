// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Admin.Domain.Diagnostics;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.Lib.Database.Repositories;

namespace Voting.ECollecting.Admin.Adapter.Data.Repositories;

/// <inheritdoc cref="IImportStatisticRepository"/>
public class ImportStatisticRepository : DbRepository<DataContext, ImportStatisticEntity>, IImportStatisticRepository
{
    private readonly TimeProvider _timeProvider;

    public ImportStatisticRepository(
        DataContext context,
        TimeProvider timeProvider)
        : base(context)
    {
        _timeProvider = timeProvider;
    }

    public async Task CreateAndUpdateIsLatest(ImportStatisticEntity import)
    {
        var currentLatest = await Set
            .Where(x => x.IsLatest && x.ImportType == import.ImportType && x.SourceSystem == import.SourceSystem)
            .AsTracking()
            .SingleOrDefaultAsync();

        if (currentLatest != null)
        {
            currentLatest.IsLatest = false;
        }

        import.IsLatest = true;
        Set.Add(import);
        await Context.SaveChangesAsync();
    }

    public async Task UpdateFinishedWithProcessingErrors(
        Guid importId,
        string processingErrors,
        List<RecordValidationErrorModel> recordValidationErrors,
        List<Guid> entityIdsWithValidationErrors,
        int? bfs,
        DateTime errorTimestamp)
    {
        var importEntity = await GetByKey(importId)
            ?? throw new EntityNotFoundException(typeof(ImportStatisticEntity), importId);

        importEntity.ImportStatus = ImportStatus.Failed;
        importEntity.AuditInfo.ModifiedAt = _timeProvider.GetUtcNowDateTime();
        importEntity.HasValidationErrors = entityIdsWithValidationErrors.Count > 0;
        importEntity.EntitiesWithValidationErrors = entityIdsWithValidationErrors;
        importEntity.FinishedDate = errorTimestamp;

        await Update(importEntity);

        DiagnosticsConfig.IncreaseProcessedImportJobs(
            importEntity.ImportType.ToString(),
            importEntity.ImportStatus.ToString());
    }
}
