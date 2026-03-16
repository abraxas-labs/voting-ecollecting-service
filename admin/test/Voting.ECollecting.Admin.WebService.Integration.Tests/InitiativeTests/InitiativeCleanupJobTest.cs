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

public class InitiativeCleanupJobTest : BaseDbTest
{
    public InitiativeCleanupJobTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCh.GuidInPreparation));
    }

    [Fact]
    public async Task ShouldDeleteCollection()
    {
        var config = GetService<CollectionCleanupJobConfig>();
        var initiativeId = InitiativesCh.GuidInPreparation;

        await ModifyDbEntities<CollectionBaseEntity>(
            x => x.Id == initiativeId,
            x =>
            {
                x.CleanupWarningSentAt = MockedClock.UtcNowDate.Subtract(config.NotificationPeriod).AddDays(-2);
                x.CollectionStartDate = null;
            });

        await GetService<JobRunner>().RunJob<InitiativeCleanupJob>(CancellationToken.None);

        var exists = await RunOnDb(db => db.Collections.AnyAsync(x => x.Id == initiativeId));
        exists.Should().BeFalse();
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
                x.CleanupWarningSentAt = MockedClock.UtcNowDate.Subtract(config.NotificationPeriod).AddDays(-2);
                x.CollectionStartDate = null;
            });

        await RunInAuditTrailTestScope(async () =>
        {
            await GetService<JobRunner>().RunJob<InitiativeCleanupJob>(CancellationToken.None);

            var exists = await RunOnDb(db => db.Collections.AnyAsync(x => x.Id == initiativeId));
            exists.Should().BeFalse();

            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldNotDeleteIfWarningNotSent()
    {
        var config = GetService<CollectionCleanupJobConfig>();
        var initiativeId = InitiativesCh.GuidInPreparation;

        await ModifyDbEntities<CollectionBaseEntity>(
            x => x.Id == initiativeId,
            x =>
            {
                x.AuditInfo.CreatedAt = MockedClock.UtcNowDate.Subtract(config.RetentionPeriod).AddDays(-2);
                x.CleanupWarningSentAt = null;
                x.CollectionStartDate = null;
            });

        await GetService<JobRunner>().RunJob<InitiativeCleanupJob>(CancellationToken.None);

        var exists = await RunOnDb(db => db.Collections.AnyAsync(x => x.Id == initiativeId));
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldNotDeleteIfWarningSentRecently()
    {
        var config = GetService<CollectionCleanupJobConfig>();
        var initiativeId = InitiativesCh.GuidInPreparation;

        await ModifyDbEntities<CollectionBaseEntity>(
            x => x.Id == initiativeId,
            x =>
            {
                x.CleanupWarningSentAt = MockedClock.UtcNowDate.Subtract(config.NotificationPeriod).AddDays(1);
                x.CollectionStartDate = null;
            });

        await GetService<JobRunner>().RunJob<InitiativeCleanupJob>(CancellationToken.None);

        var exists = await RunOnDb(db => db.Collections.AnyAsync(x => x.Id == initiativeId));
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldNotDeleteIfStarted()
    {
        var config = GetService<CollectionCleanupJobConfig>();
        var initiativeId = InitiativesCh.GuidInPreparation;

        await ModifyDbEntities<CollectionBaseEntity>(
            x => x.Id == initiativeId,
            x =>
            {
                x.AuditInfo.CreatedAt = MockedClock.UtcNowDate.Subtract(config.RetentionPeriod).AddDays(-2);
                x.CleanupWarningSentAt = MockedClock.UtcNowDate.Subtract(config.RetentionPeriod).AddDays(-2);
                x.CollectionStartDate = MockedClock.NowDateOnly;
            });

        await GetService<JobRunner>().RunJob<InitiativeCleanupJob>(CancellationToken.None);

        var exists = await RunOnDb(db => db.Collections.AnyAsync(x => x.Id == initiativeId));
        exists.Should().BeTrue();
    }
}
