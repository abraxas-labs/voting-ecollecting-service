// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Core.Services;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.Lib.Scheduler;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.Services;

public class SensitiveDataExpiryReminderJobTest : BaseDbTest
{
    public SensitiveDataExpiryReminderJobTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Default.WithInitiatives(InitiativesCh.GuidEndedCameNotAbout));
    }

    [Fact]
    public async Task ShouldSendNotificationsWhenExpiryDateIsToday()
    {
        await ModifyDbEntities<DecreeEntity>(
            x => x.Id == DecreesCh.GuidPastWithReferendumNotCameAbout,
            x => x.SensitiveDataExpiryDate = MockedClock.NowDateOnly);

        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesCh.GuidEndedCameNotAbout,
            x => x.SensitiveDataExpiryDate = MockedClock.NowDateOnly);

        SentUserNotifications.Clear();
        await GetService<JobRunner>().RunJob<SensitiveDataExpiryReminderJob>(CancellationToken.None);

        SentUserNotifications.Should().HaveCount(4);
        var dbNotifications = await RunOnDb(db => db.UserNotifications
            .Where(x => x.TemplateBag.NotificationType == UserNotificationType.SensitiveDataExpiryReminder)
            .ToListAsync());

        await Verify(new { SentUserNotifications, dbNotifications });
    }
}
