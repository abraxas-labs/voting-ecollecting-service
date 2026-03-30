// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Domain.Entities;

// if adjusted, the database may need to be migrated.
public enum UserNotificationType
{
    MessageAdded,
    StateChanged,
    PermissionAdded,
    CommitteeMembershipAdded,
    CommitteeMembershipAddedWithPermission,
    CollectionDeleted,
    AccessibilityMessage,
    DecreeDeleted,
    CollectionCleanupWarning,
    CertificateExpirationWarning,
}
