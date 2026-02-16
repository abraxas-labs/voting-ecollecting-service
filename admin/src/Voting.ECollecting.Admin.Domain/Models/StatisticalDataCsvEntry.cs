// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using CsvHelper.Configuration.Attributes;

namespace Voting.ECollecting.Admin.Domain.Models;

public class StatisticalDataCsvEntry
{
    [Name("ID_Unterschriftensammlung")]
    public Guid CollectionId { get; set; }

    [Name("ID_Erlass")]
    public Guid? DecreeId { get; set; }

    [Name("Datum elektr. Unterzeichnung")]
    public string ElectronicSignatureDateFormatted => ElectronicSignatureDate?.ToString("dd.MM.yyyy") ?? string.Empty;

    [Name("Uhrzeit elektr. Unterzeichnung")]
    public string ElectronicSignatureTimeFormatted => ElectronicSignatureDate?.ToString("HH:mm") ?? string.Empty;

    [Name("Datum Eingang physische Unterschrift bei der Gemeinde")]
    public string PhysicalReceivedAtFormatted => PhysicalReceivedAt?.ToString("dd.MM.yyyy") ?? string.Empty;

    [Name("BFS Gemeinde")]
    public string Bfs { get; set; } = string.Empty;

    [Name("Name der Gemeinde")]
    public string MunicipalityName { get; set; } = string.Empty;

    [Name("Geschlecht")]
    public string SexFormatted => Sex == 1 ? "M" : "F";

    [Name("Alter")]
    public int Age { get; set; }

    [Ignore]
    public DateTime? ElectronicSignatureDate { get; set; }

    [Ignore]
    public DateOnly? PhysicalReceivedAt { get; set; }

    [Ignore]
    public int Sex { get; set; }
}
