// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.Lib.Common;

namespace Voting.ECollecting.Shared.Domain.Entities;

public class UserNotificationTemplateBag
{
    public Guid? DecreeId { get; set; }

    public string? DecreeName { get; set; }

    public string CollectionName { get; set; } = string.Empty;

    public bool RecipientIsCitizen { get; set; }

    public UserNotificationType NotificationType { get; set; }

    public Guid? CollectionId { get; set; }

    public CollectionType? CollectionType { get; set; }

    public UrlToken? PermissionToken { get; set; }

    public UrlToken? InitiativeCommitteeMembershipToken { get; set; }

    public AccessibilityMessage? AccessibilityMessage { get; set; }

    public DateOnly? CollectionCleanupDate { get; set; }

    public DateTime? CertificateExpirationDate { get; set; }

    public bool IsCaCertificate { get; set; }
}
