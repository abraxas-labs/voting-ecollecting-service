// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.Lib.Common;
using Voting.Lib.Common.Files;

namespace Voting.ECollecting.Shared.Abstractions.Core.Services;

public interface IUserNotificationService
{
    Task ScheduleNotification(
        CollectionBaseEntity collection,
        UserNotificationType type);

    Task SendUserNotification(
        string email,
        bool recipientIsCitizen,
        UserNotificationType type,
        DecreeEntity? decree = null,
        CollectionBaseEntity? collection = null,
        IFile[]? attachments = null,
        UrlToken? permissionToken = null,
        UrlToken? initiativeCommitteeMembershipToken = null,
        AccessibilityMessage? accessibilityMessage = null,
        CancellationToken cancellationToken = default);
}
