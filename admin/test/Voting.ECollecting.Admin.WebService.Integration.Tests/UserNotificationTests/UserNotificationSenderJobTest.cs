// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Core.Services.UserNotifications;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Migrations;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.UserNotificationTests;

public class UserNotificationSenderJobTest : BaseDbTest
{
    public UserNotificationSenderJobTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives with { SeedInitiativeUserNotifications = true });
    }

    [Fact]
    public async Task ShouldSendEmails()
    {
        ResetUserNotificationSender();

        await RunScoped((MigrationDataContext db) => db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId != InitiativesCh.GuidInPreparation)
            .ExecuteDeleteAsync());

        var job = GetService<UserNotificationSenderJob>();
        await job.Run(CancellationToken.None);

        var notifications = await RunScoped((MigrationDataContext db) => db
            .UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCh.GuidInPreparation)
            .OrderBy(x => x.Id)
            .ToListAsync());

        var sent = SentUserNotifications;
        await Verify(new { sent, notifications })
            .ScrubInlineGuids();
    }

    [Fact]
    public async Task ShouldIncreaseFailureCounterWhenFailed()
    {
        ResetUserNotificationSender(true);

        var job = GetService<UserNotificationSenderJob>();
        await job.Run(CancellationToken.None);

        var notifications = await RunScoped((MigrationDataContext db) => db
            .UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCh.GuidInPreparation)
            .OrderBy(x => x.Id)
            .ToListAsync());

        SentUserNotifications.Should().BeEmpty();
        notifications.Count.Should().Be(5);
        notifications
            .Single(x => x.Id == UserNotifications.BuildGuidPending(InitiativesCh.GuidInPreparation))
            .FailureCounter
            .Should()
            .Be(1);
        notifications
            .Single(x => x.Id == UserNotifications.BuildGuidPendingAfterFailure(InitiativesCh.GuidInPreparation))
            .FailureCounter
            .Should()
            .Be(2);
    }
}
