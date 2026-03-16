// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Admin.Core.Services;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.Lib.Scheduler;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class InitiativeCleanupWarningNotificationJobTest : BaseDbTest
{
    public InitiativeCleanupWarningNotificationJobTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        ResetUserNotificationSender();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCh.GuidInPreparation));
    }

    [Fact]
    public async Task ShouldSendNotification()
    {
        var config = GetService<CollectionCleanupJobConfig>();
        var initiativeId = InitiativesCh.GuidInPreparation;

        await ModifyDbEntities<CollectionBaseEntity>(
            x => x.Id == initiativeId,
            x =>
            {
                x.AuditInfo.CreatedAt = MockedClock.UtcNowDate.Subtract(config.RetentionPeriod).Add(config.NotificationPeriod).AddDays(-2);
                x.CleanupWarningSentAt = null;
                x.CollectionStartDate = null;
            });

        await GetService<JobRunner>().RunJob<InitiativeCleanupWarningNotificationJob>(CancellationToken.None);

        var collection = await RunOnDb(db => db.Collections.FirstOrDefaultAsync(x => x.Id == initiativeId));
        collection.Should().NotBeNull();
        collection!.CleanupWarningSentAt.Should().Be(MockedClock.UtcNowDate);

        var notifications = await RunOnDb(db => db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == initiativeId && x.TemplateBag.NotificationType == UserNotificationType.CollectionCleanupWarning)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        notifications.Should().HaveCount(2);
        await Verify(SentUserNotifications);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        var config = GetService<CollectionCleanupJobConfig>();
        var initiativeId = InitiativesCh.GuidInPreparation;

        await ModifyDbEntities<CollectionBaseEntity>(
            x => x.Id == initiativeId,
            x =>
            {
                x.AuditInfo.CreatedAt = MockedClock.UtcNowDate.Subtract(config.RetentionPeriod).Add(config.NotificationPeriod).AddDays(-3);
                x.CleanupWarningSentAt = null;
                x.CollectionStartDate = null;
            });

        await RunInAuditTrailTestScope(async () =>
        {
            await GetService<JobRunner>().RunJob<InitiativeCleanupWarningNotificationJob>(CancellationToken.None);

            var collection = await RunOnDb(db => db.Collections.FirstOrDefaultAsync(x => x.Id == initiativeId));
            collection.Should().NotBeNull();
            collection!.CleanupWarningSentAt.Should().Be(MockedClock.UtcNowDate);

            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldNotSendNotificationIfAlreadySent()
    {
        var config = GetService<CollectionCleanupJobConfig>();
        var initiativeId = InitiativesCh.GuidInPreparation;

        var sentAt = MockedClock.UtcNowDate.AddDays(-2);
        await ModifyDbEntities<CollectionBaseEntity>(
            x => x.Id == initiativeId,
            x =>
            {
                x.AuditInfo.CreatedAt = MockedClock.UtcNowDate.Subtract(config.RetentionPeriod).Add(config.NotificationPeriod).Subtract(TimeSpan.FromDays(1));
                x.CleanupWarningSentAt = sentAt;
            });

        await GetService<JobRunner>().RunJob<InitiativeCleanupWarningNotificationJob>(CancellationToken.None);

        var collection = await RunOnDb(db => db.Collections.FirstOrDefaultAsync(x => x.Id == initiativeId));
        collection!.CleanupWarningSentAt.Should().Be(sentAt);
        SentUserNotifications.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldNotSendNotificationIfTooYoung()
    {
        var config = GetService<CollectionCleanupJobConfig>();
        var initiativeId = InitiativesCh.GuidInPreparation;

        await ModifyDbEntities<CollectionBaseEntity>(
            x => x.Id == initiativeId,
            x =>
            {
                x.AuditInfo.CreatedAt = MockedClock.UtcNowDate.Subtract(config.RetentionPeriod).Add(config.NotificationPeriod).Add(TimeSpan.FromDays(1));
                x.CleanupWarningSentAt = null;
                x.CollectionStartDate = null;
            });

        await GetService<JobRunner>().RunJob<InitiativeCleanupWarningNotificationJob>(CancellationToken.None);

        var collection = await RunOnDb(db => db.Collections.FirstOrDefaultAsync(x => x.Id == initiativeId));
        collection!.CleanupWarningSentAt.Should().BeNull();
        SentUserNotifications.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldNotSendNotificationIfStarted()
    {
        var config = GetService<CollectionCleanupJobConfig>();
        var initiativeId = InitiativesCh.GuidInPreparation;

        await ModifyDbEntities<CollectionBaseEntity>(
            x => x.Id == initiativeId,
            x =>
            {
                x.AuditInfo.CreatedAt = MockedClock.UtcNowDate.Subtract(config.RetentionPeriod).Add(config.NotificationPeriod).Subtract(TimeSpan.FromDays(1));
                x.CollectionStartDate = MockedClock.NowDateOnly;
            });

        await GetService<JobRunner>().RunJob<InitiativeCleanupWarningNotificationJob>(CancellationToken.None);

        var collection = await RunOnDb(db => db.Collections.FirstOrDefaultAsync(x => x.Id == initiativeId));
        collection!.CleanupWarningSentAt.Should().BeNull();
        SentUserNotifications.Should().BeEmpty();
    }
}
