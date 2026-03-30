// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Models;

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
        UserNotificationContext context,
        CancellationToken cancellationToken = default);

    Task SendUserNotifications(
        IReadOnlyCollection<string> emails,
        bool recipientsAreCitizen,
        UserNotificationType type,
        UserNotificationContext context,
        CancellationToken cancellationToken = default);
}
