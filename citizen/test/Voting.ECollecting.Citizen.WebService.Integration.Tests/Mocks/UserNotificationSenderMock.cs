// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.UserNotifications;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.Mocks;

public class UserNotificationSenderMock : IUserNotificationSender
{
    public List<UserNotification> Sent { get; } = [];

    public bool FailSendAttempts { get; set; }

    public Task Send(UserNotification notification, CancellationToken cancellationToken)
    {
        if (FailSendAttempts)
        {
            throw new InvalidOperationException("Failed to send notification.");
        }

        Sent.Add(notification);
        return Task.CompletedTask;
    }
}
