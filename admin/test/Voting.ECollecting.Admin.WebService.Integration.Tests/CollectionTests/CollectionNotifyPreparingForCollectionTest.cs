// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionNotifyPreparingForCollectionTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionNotifyPreparingForCollectionTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Default
            .WithReferendums(ReferendumsCtStGallen.GuidInPreparingForCollection)
            .WithInitiatives(InitiativesCh.GuidPreparingForCollection));
    }

    [Fact]
    public async Task ShouldWork()
    {
        await ApiNotifyClient.NotifyPreparingForCollectionAsync(new());

        var collections = await RunOnDb(async db => await db.Collections
            .Where(c => c.Id == InitiativesCh.GuidPreparingForCollection || c.Id == ReferendumsCtStGallen.GuidInPreparingForCollection)
            .Include(x => x.Municipalities!.OrderBy(y => y.Bfs))
            .OrderBy(c => c.Description)
            .ToListAsync());

        var now = GetService<TimeProvider>().GetUtcTodayDateOnly();
        foreach (var collection in collections)
        {
            collection.SetPeriodState(now);
        }

        await Verify(collections);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await ApiNotifyClient.NotifyPreparingForCollectionAsync(new());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldWorkIfKeysAlreadyPartiallyCreated()
    {
        var id = InitiativesCh.GuidPreparingForCollection;

        await ModifyDbEntities<CollectionBaseEntity>(
            x => x.Id == id,
            x => x.MacKeyId = "ALREADY_CREATED");

        await ApiNotifyClient.NotifyPreparingForCollectionAsync(new());

        var collection = await RunOnDb(async db => await db.Collections.Include(x => x.Municipalities).SingleAsync(x => x.Id == id));
        collection.EncryptionKeyId.Should().NotBeEmpty();
        collection.MacKeyId.Should().NotBeEmpty();
        collection.State.Should().Be(CollectionState.EnabledForCollection);
        collection.Municipalities.Should().HaveCount(4);
    }

    [Fact]
    public async Task ShouldWorkIfCollectionMunicipalitiesAlreadyCreated()
    {
        var id = ReferendumsCtStGallen.GuidInPreparingForCollection;

        await ApiNotifyClient.NotifyPreparingForCollectionAsync(new());

        var collection = await RunOnDb(async db =>
            await db.Collections
                .Include(x => x.Municipalities)
                .SingleAsync(x => x.Id == id));
        collection.EncryptionKeyId.Should().NotBeEmpty();
        collection.MacKeyId.Should().NotBeEmpty();
        collection.State.Should().Be(CollectionState.EnabledForCollection);
        collection.Municipalities.Should().HaveCount(4);
    }

    [Fact]
    public async Task ShouldSkipUpdateIfStateNotPreparingForCollection()
    {
        var id = InitiativesCh.GuidPreparingForCollection;

        await ModifyDbEntities<CollectionBaseEntity>(
            x => x.Id == id,
            x =>
            {
                x.State = CollectionState.InPreparation;
                x.EncryptionKeyId = string.Empty;
                x.MacKeyId = string.Empty;
            });

        await ApiNotifyClient.NotifyPreparingForCollectionAsync(new());

        var collection = await RunOnDb(async db => await db.Collections.Include(x => x.Municipalities).SingleAsync(x => x.Id == id));
        collection.EncryptionKeyId.Should().BeEmpty();
        collection.MacKeyId.Should().BeEmpty();
        collection.State.Should().Be(CollectionState.InPreparation);
        collection.Municipalities.Should().BeEmpty();
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionService.CollectionServiceClient(channel)
            .NotifyPreparingForCollectionAsync(new());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.ApiNotify;
    }
}
