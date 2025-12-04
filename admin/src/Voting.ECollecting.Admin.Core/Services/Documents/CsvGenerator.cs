// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Common.Files;

namespace Voting.ECollecting.Admin.Core.Services.Documents;

public abstract class CsvGenerator<TEntity, TRootEntity>
{
    private readonly CsvService _csvService;

    protected CsvGenerator(CsvService csvService)
    {
        _csvService = csvService;
    }

    protected IFile GenerateFile(TRootEntity rootEntity, IAsyncEnumerable<TEntity> records)
    {
        return new PipedFile((w, ct) => _csvService.Render(w, records, ct), BuildFileName(rootEntity), "text/csv");
    }

    protected abstract string BuildFileName(TRootEntity rootEntity);
}
