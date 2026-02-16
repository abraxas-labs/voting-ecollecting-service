// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.ECollecting.Admin.WebService.Integration.Tests.Mocks;
using Voting.ECollecting.Shared.Migrations;
using Voting.ECollecting.Shared.Test.Models;
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

    protected Task<TResult> RunOnDb<TResult>(Func<MigrationDataContext, Task<TResult>> action)
        => RunScoped(action);

    protected Task RunOnDb(Func<MigrationDataContext, Task> action)
        => RunScoped(action);

    protected void ResetUserNotificationSender(bool failAttempts = false)
    {
        var senderMock = GetService<UserNotificationSenderMock>();
        senderMock.Sent.Clear();
        senderMock.FailSendAttempts = failAttempts;
    }

    protected Task ModifyDbEntities<TEntity>(Expression<Func<TEntity, bool>> predicate, Action<TEntity> modifier)
        where TEntity : class
    {
        return RunOnDb(async db =>
        {
            var set = db.Set<TEntity>();
            var entities = await set.AsTracking().Where(predicate).ToListAsync();

            foreach (var entity in entities)
            {
                modifier(entity);
            }

            await db.SaveChangesAsync();
        });
    }

    protected async Task RunInAuditTrailTestScope(Func<Task> action)
    {
        PermissionServiceMock.HasTimestampIncrement = true;

        try
        {
            await action.Invoke();
        }
        finally
        {
            PermissionServiceMock.HasTimestampIncrement = false;
        }
    }

    protected async Task<AuditTrailEntriesResult> GetAuditTrailEntries()
    {
        var auditTrailEntries = await RunOnDb(db => db.AuditTrailEntries
            .OrderBy(e => e.AuditInfo.CreatedAt)
            .ToListAsync());

        var collectionCitizenLogAuditTrailEntries = await RunOnDb(db => db.CollectionCitizenLogAuditTrailEntries
            .OrderBy(e => e.AuditInfo.CreatedAt)
            .ToListAsync());

        return new(auditTrailEntries, collectionCitizenLogAuditTrailEntries);
    }
}
