// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.Extensions.DependencyInjection;
using Voting.ECollecting.Admin.WebService.Integration.Tests.Mocks;
using Voting.ECollecting.Shared.Migrations;
using Voting.ECollecting.Shared.Test.Utils;
using Voting.Lib.Testing;
using Voting.Lib.UserNotifications;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests;

public abstract class BaseDbTest : BaseTest<TestApplicationFactory, TestStartup>
{
    protected BaseDbTest(TestApplicationFactory factory)
        : base(factory)
    {
        ResetDb();
    }

    protected List<UserNotification> SentUserNotifications => GetService<UserNotificationSenderMock>().Sent;

    protected void ResetDb()
    {
        using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MigrationDataContext>();
        DatabaseUtil.Truncate(db);
    }

    protected void ResetUserNotificationSender(bool failAttempts = false)
    {
        var senderMock = GetService<UserNotificationSenderMock>();
        senderMock.Sent.Clear();
        senderMock.FailSendAttempts = failAttempts;
    }
}
