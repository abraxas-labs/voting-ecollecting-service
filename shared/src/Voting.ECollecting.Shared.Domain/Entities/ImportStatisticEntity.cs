// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities.Audit;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Shared.Domain.Entities;

/// <summary>
/// Import statistics entity which will be created for each import request.
/// </summary>
public class ImportStatisticEntity : AuditedEntity
{
    /// <summary>
    /// Gets or sets the source system name.
    /// </summary>
    public ImportSourceSystem SourceSystem { get; set; }

    /// <summary>
    /// Gets or sets the count of delivered import records within the import.
    /// </summary>
    public int ImportRecordsCountTotal { get; set; }

    /// <summary>
    /// Gets or sets the count of created datasets within the import.
    /// </summary>
    public int DatasetsCountCreated { get; set; }

    /// <summary>
    /// Gets or sets the count of updated datasets within the import.
    /// </summary>
    public int DatasetsCountUpdated { get; set; }

    /// <summary>
    /// Gets or sets the count of deleted datasets within this import.
    /// </summary>
    public int DatasetsCountDeleted { get; set; }

    /// <summary>
    /// Gets or sets the finished date, which represents when the import has been completed.
    /// </summary>
    public DateTime? FinishedDate { get; set; }

    /// <summary>
    /// Gets or sets the started date, when the import job has been started.
    /// </summary>
    public DateTime? StartedDate { get; set; }

    /// <summary>
    /// Gets or sets the total elapsed miliseconds during the import.
    /// </summary>
    public double? TotalElapsedMilliseconds
    {
        get => (FinishedDate - StartedDate)?.TotalMilliseconds;
        set => _ = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the import has validation errors.
    /// </summary>
    public bool HasValidationErrors { get; set; }

    /// <summary>
    /// Gets or sets a list of entity ids which contain validation errors.
    /// </summary>
    public List<Guid> EntitiesWithValidationErrors { get; set; } = new();

    /// <summary>
    /// Gets or sets the import state.
    /// </summary>
    public ImportStatus ImportStatus { get; set; }

    /// <summary>
    /// Gets or sets the import type.
    /// </summary>
    public ImportType ImportType { get; set; }

    /// <summary>
    /// Gets or sets the name of the machine which worked on this import.
    /// </summary>
    public string WorkerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this entity is the latest of a given
    /// municipality id, import type and source system combination.
    /// </summary>
    public bool IsLatest { get; set; }

    /// <summary>
    /// Gets or sets the access control DOI references.
    /// </summary>
    public ICollection<DomainOfInfluenceEntity> AccessControlListDois { get; set; } = new HashSet<DomainOfInfluenceEntity>();
}
