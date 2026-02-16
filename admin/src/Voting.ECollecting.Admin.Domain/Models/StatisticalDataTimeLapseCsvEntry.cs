// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using CsvHelper.Configuration.Attributes;

namespace Voting.ECollecting.Admin.Domain.Models;

public class StatisticalDataTimeLapseCsvEntry
{
    [Name("Gemeinde")]
    public string MunicipalityName { get; set; } = string.Empty;

    [Name("Tag")]
    public string DateFormatted => Date.ToString("dd.MM.yyyy");

    [Name("Anzahl elektronisch gültige")]
    public int ElectronicCitizenCount { get; set; }

    [Name("Anzahl physisch gültige")]
    public int ValidPhysicalSignatureCount { get; set; }

    [Name("Anzahl physisch ungültige")]
    public int InvalidPhysicalSignatureCount { get; set; }

    [Name("Uhrzeit Max. elektr. Unterschriften erreicht")]
    public string DateMaxElectronicSignatureCountReachedFormatted => DateMaxElectronicSignatureCountReached?.ToString("HH:mm") ?? string.Empty;

    [Ignore]
    public DateTime? DateMaxElectronicSignatureCountReached { get; set; }

    [Ignore]
    public DateOnly Date { get; set; }
}
