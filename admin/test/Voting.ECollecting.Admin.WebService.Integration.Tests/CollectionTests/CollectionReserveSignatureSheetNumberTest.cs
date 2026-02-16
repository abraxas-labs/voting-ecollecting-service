// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionReserveSignatureSheetNumberTest : BaseGrpcTest<CollectionSignatureSheetService.CollectionSignatureSheetServiceClient>
{
    public CollectionReserveSignatureSheetNumberTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Referendums.WithReferendums(ReferendumsCtStGallen.GuidInCollectionEnabledForCollection, ReferendumsCtStGallen.GuidInCollectionEnabledForCollection2) with
            {
                SeedReferendumSignatureSheets = true,
            });
    }

    [Fact]
    public async Task ShouldWork()
    {
        var data = await MuSgKontrollzeichenerfasserClient.ReserveNumberAsync(NewValidRequest());
        await Verify(data);

        var data2 = await MuSgKontrollzeichenerfasserClient.ReserveNumberAsync(NewValidRequest());
        data2.Number.Should().Be(data.Number + 1);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await MuSgKontrollzeichenerfasserClient.ReserveNumberAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldThrowAsCt()
    {
        await AssertStatus(
            async () => await CtSgKontrollzeichenerfasserClient.ReserveNumberAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldWorkWithOtherMunicipality()
    {
        var data = await MuSgKontrollzeichenerfasserClient.ReserveNumberAsync(NewValidRequest());
        data.Number.Should().Be(9);

        var data2 = await MuGoldachKontrollzeichenerfasserClient.ReserveNumberAsync(NewValidRequest());
        data2.Number.Should().Be(9);
    }

    [Fact]
    public async Task ShouldWorkWithOtherCollection()
    {
        var data = await MuSgKontrollzeichenerfasserClient.ReserveNumberAsync(NewValidRequest());
        data.Number.Should().Be(9);

        var data2 = await MuSgKontrollzeichenerfasserClient.ReserveNumberAsync(new ReserveSignatureSheetNumberRequest
        {
            CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection2,
        });
        data2.Number.Should().Be(9);
    }

    [Fact]
    public async Task ParallelInvocationsShouldReturnUniqueNumbers()
    {
        const int count = 10;
        var tasks = Enumerable.Range(0, count)
            .Select(_ => MuSgKontrollzeichenerfasserClient.ReserveNumberAsync(NewValidRequest()).ResponseAsync)
            .ToList();
        await Task.WhenAll(tasks);

        var seenNumbers = new SortedSet<int>();
        foreach (var task in tasks)
        {
            var n = (await task).Number;
            seenNumbers.Add(n).Should().BeTrue();
        }

        seenNumbers
            .Should()
            .BeEquivalentTo(Enumerable.Range(seenNumbers.Min, count), x => x.WithStrictOrdering());
    }

    [Fact]
    public async Task NotPublishedShouldThrow()
    {
        await ModifyDbEntities(
            (ReferendumEntity e) => e.Id == ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
            e => e.CollectionStartDate = MockedClock.NowDateOnly.AddDays(1));
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.ReserveNumberAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task MunicipalityLockedShouldThrow()
    {
        await ModifyDbEntities<CollectionMunicipalityEntity>(
            x => x.CollectionId == ReferendumsCtStGallen.GuidInCollectionEnabledForCollection && x.Bfs == Bfs.MunicipalityStGallen,
            x => x.IsLocked = true);
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.ReserveNumberAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionSignatureSheetService.CollectionSignatureSheetServiceClient(channel)
            .ReserveNumberAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Kontrollzeichenerfasser;
    }

    private static ReserveSignatureSheetNumberRequest NewValidRequest()
    {
        return new ReserveSignatureSheetNumberRequest
        {
            CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection,
        };
    }
}
