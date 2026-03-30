// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.Common;
using Voting.Lib.Common.Files;

namespace Voting.ECollecting.Shared.Domain.Models;

public record UserNotificationContext(
    DecreeEntity? Decree = null,
    CollectionBaseEntity? Collection = null,
    IFile[]? Attachments = null,
    UrlToken? PermissionToken = null,
    UrlToken? InitiativeCommitteeMembershipToken = null,
    AccessibilityMessage? AccessibilityMessage = null,
    DateOnly? CollectionCleanupDate = null,
    DateTime? CertificateExpirationDate = null,
    bool IsCaCertificate = false);
