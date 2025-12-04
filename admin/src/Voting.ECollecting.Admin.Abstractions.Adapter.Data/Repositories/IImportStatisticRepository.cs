// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.Database.Repositories;

namespace Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;

/// <summary>
/// Repository for import statistics.
/// </summary>
public interface IImportStatisticRepository : IDbRepository<DbContext, ImportStatisticEntity>
{
    /// <summary>
    /// Stores a new <see cref="ImportStatisticEntity"/>
    /// and sets <see cref="ImportStatisticEntity.IsLatest"/>
    /// of the old entry to false.
    /// </summary>
    /// <param name="import">The entity to create.</param>
    /// <returns>An <see cref="Task"/> representing the async operation.</returns>
    Task CreateAndUpdateIsLatest(ImportStatisticEntity import);

    /// <summary>
    /// Updates the import statistics for an <see cref="ImportStatisticEntity"/> matching the 'importId' in case of processing errors.
    /// </summary>
    /// <param name="importId">The import id for resolving the import entity.</param>
    /// <param name="processingErrors">Processing errors description.</param>
    /// <param name="recordValidationErrors">The record validations dictionary.</param>
    /// <param name="entityIdsWithValidationErrors">The ids of entities with validation errors.</param>
    /// <param name="bfs">The municipality id.</param>
    /// <param name="errorTimestamp">Timestamp when the error occured.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task UpdateFinishedWithProcessingErrors(
        Guid importId,
        string processingErrors,
        List<RecordValidationErrorModel> recordValidationErrors,
        List<Guid> entityIdsWithValidationErrors,
        int? bfs,
        DateTime errorTimestamp);
}
