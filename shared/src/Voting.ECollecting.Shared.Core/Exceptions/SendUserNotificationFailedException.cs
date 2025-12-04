// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Shared.Core.Exceptions;

public class SendUserNotificationFailedException : Exception
{
    public SendUserNotificationFailedException(Guid id, Exception ex)
        : base($"Sending user notification with id {id} failed", ex)
    {
    }
}
