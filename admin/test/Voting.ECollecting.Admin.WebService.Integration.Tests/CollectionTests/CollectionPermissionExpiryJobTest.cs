// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Core.Services;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionPermissionExpiryJobTest : BaseDbTest
{
    public CollectionPermissionExpiryJobTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCh.GuidInPreparation));
    }

    [Fact]
    public async Task ShouldExpirePermissions()
    {
        var expectedStates =
            new List<(Guid Id, CollectionPermissionState StateBeforeJob, CollectionPermissionState StateAfterJob)>
            {
                (CollectionPermissions.BuildGuid(
                    InitiativesCh.GuidInPreparation,
                    CollectionPermissionRole.Reader,
                    "expired"), CollectionPermissionState.Expired, CollectionPermissionState.Expired),
                (CollectionPermissions.BuildGuid(
                    InitiativesCh.GuidInPreparation,
                    CollectionPermissionRole.Reader,
                    "expired-not-updated"), CollectionPermissionState.Pending, CollectionPermissionState.Expired),
                (CollectionPermissions.BuildGuid(
                    InitiativesCh.GuidInPreparation,
                    true,
                    CollectionPermissionRole.Reader), CollectionPermissionState.Accepted, CollectionPermissionState.Accepted),
                (CollectionPermissions.BuildGuid(
                    InitiativesCh.GuidInPreparation,
                    false,
                    CollectionPermissionRole.Reader), CollectionPermissionState.Rejected, CollectionPermissionState.Rejected),
            };

        foreach (var (id, stateBeforeJob, _) in expectedStates)
        {
            var item = await RunOnDb(db => db.CollectionPermissions.SingleAsync(x => x.Id == id));
            item.State.Should().Be(stateBeforeJob);
        }

        await GetService<CollectionPermissionExpiryJob>().Run(CancellationToken.None);

        foreach (var (id, _, stateAfterJob) in expectedStates)
        {
            var item = await RunOnDb(db => db.CollectionPermissions.SingleAsync(x => x.Id == id));
            item.State.Should().Be(stateAfterJob);
        }
    }
}
