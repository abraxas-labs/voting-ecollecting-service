// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.UserNotifications;

namespace Voting.ECollecting.Shared.Abstractions.Core.Services;

public interface IUserNotificationRenderer
{
    UserNotification Render(UserNotificationEntity notification);
}
