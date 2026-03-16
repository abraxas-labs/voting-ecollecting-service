// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities.IntegritySignature;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Shared.Domain.Entities;

public class CollectionBaseEntity : IntegritySignatureEntity, IHasBfs, IHasCollectionPeriod
{
    // The signature list submission end date is 3 days before the collection end date.
    private const int SignatureListSubmissionOffsetInDays = -3;

    private CollectionPeriodState? _periodState;

    public DateOnly? CollectionStartDate { get; set; }

    public DateOnly? CollectionEndDate { get; set; }

    public string Description { get; set; } = string.Empty;

    public CollectionType Type { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string Link { get; set; } = string.Empty;

    public Guid? LogoId { get; set; }

    public FileEntity? Logo { get; set; }

    public Guid? ImageId { get; set; }

    public FileEntity? Image { get; set; }

    public bool SignatureSheetTemplateGenerated { get; set; }

    public Guid? SignatureSheetTemplateId { get; set; }

    public FileEntity? SignatureSheetTemplate { get; set; }

    public CollectionState State { get; set; }

    public CollectionCountEntity? CollectionCount { get; set; }

    // cannot set owned field to null
    public CollectionAddress Address { get; set; } = new();

    public List<CollectionPermissionEntity>? Permissions { get; set; }

    public List<CollectionMessageEntity>? Messages { get; set; }

    public List<CollectionMunicipalityEntity>? Municipalities { get; set; }

    public string? EncryptionKeyId { get; set; }

    public string? MacKeyId { get; set; }

    public bool IsElectronicSubmission { get; set; }

    public bool InformalReviewRequested { get; set; }

    public List<CollectionCitizenLogEntity>? CitizenLogs { get; set; }

    // This can be nullable, since a referendum may not be assigned to a decree.
    public DomainOfInfluenceType? DomainOfInfluenceType { get; set; }

    // This can be nullable, since a referendum may not be assigned to a decree.
    public string? Bfs { get; set; }

    // This can be nullable, since a referendum may not be assigned to a decree.
    public int? MaxElectronicSignatureCount { get; set; }

    /// <summary>
    /// Gets or sets a public but secure identification number for this collection.
    /// Secure in the sense that it is not easily guessable and can be used for public identification.
    /// </summary>
    public string? SecureIdNumber { get; set; }

    public DateTime? CleanupWarningSentAt { get; set; }

    public CollectionPeriodState PeriodState => _periodState ?? throw new InvalidOperationException("State not initialized.");

    // The signature list submission end date is a defined amount of days before the collection end date.
    public DateTime? SignatureListSubmissionEndDate => CollectionEndDate?.AddDays(SignatureListSubmissionOffsetInDays).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

    public virtual void SetPeriodState(DateOnly today)
    {
        if (!CollectionStartDate.HasValue || !CollectionEndDate.HasValue)
        {
            _periodState = CollectionPeriodState.Unspecified;
            return;
        }

        if (CollectionStartDate > today)
        {
            _periodState = CollectionPeriodState.Published;
        }
        else if (CollectionStartDate <= today && CollectionEndDate >= today)
        {
            _periodState = CollectionPeriodState.InCollection;
        }
        else
        {
            _periodState = CollectionPeriodState.Expired;
        }
    }
}
