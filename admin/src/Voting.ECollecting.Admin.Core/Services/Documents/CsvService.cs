// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Globalization;
using System.IO.Pipelines;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace Voting.ECollecting.Admin.Core.Services.Documents;

public class CsvService
{
    private static readonly CsvConfiguration _csvConfiguration = NewCsvConfig();

    public async Task Render<TRow>(PipeWriter writer, IAsyncEnumerable<TRow> records, CancellationToken ct = default)
    {
        // use utf8 with bom (excel requires bom)
        await using var streamWriter = new StreamWriter(writer.AsStream(), Encoding.UTF8);
        await using var csvWriter = new CsvWriter(streamWriter, _csvConfiguration);
        await csvWriter.WriteRecordsAsync(records, ct);
    }

    private static CsvConfiguration NewCsvConfig() =>
        new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
        };
}
