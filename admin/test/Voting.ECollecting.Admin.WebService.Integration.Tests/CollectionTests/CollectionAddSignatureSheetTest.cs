// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.Admin.WebService.Integration.Tests.Mocks;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionAddSignatureSheetTest : BaseGrpcTest<CollectionSignatureSheetService.CollectionSignatureSheetServiceClient>
{
    public CollectionAddSignatureSheetTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Referendums.WithReferendums(ReferendumsCtStGallen.GuidInCollectionEnabledForCollection, ReferendumsMuStGallen.GuidInCollectionActive) with
            {
                SeedReferendumSignatureSheets = true,
            });
        await MuSgKontrollzeichenerfasserClient.ReserveNumberAsync(new ReserveSignatureSheetNumberRequest
        {
            CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection,
        });
    }

    [Fact]
    public async Task ShouldWork()
    {
        var id = await MuSgKontrollzeichenerfasserClient.AddAsync(NewValidRequest());
        var created = await RunOnDb(db => db.CollectionSignatureSheets.FirstAsync(x => x.Id == Guid.Parse(id.Id)));
        await Verify(created);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await MuSgKontrollzeichenerfasserClient.AddAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldWorkAsMuOnMuCollection()
    {
        await MuSgKontrollzeichenerfasserClient.ReserveNumberAsync(new ReserveSignatureSheetNumberRequest
        {
            CollectionId = ReferendumsMuStGallen.IdInCollectionActive,
        });

        var req = NewValidRequest();
        req.CollectionId = ReferendumsMuStGallen.IdInCollectionActive;
        var id = await MuSgKontrollzeichenerfasserClient.AddAsync(req);
        var created = await RunOnDb(db => db.CollectionSignatureSheets.FirstAsync(x => x.Id == Guid.Parse(id.Id)));
        await Verify(created);
    }

    [Fact]
    public async Task ShouldWorkAsOtherMu()
    {
        var number = await MuGoldachKontrollzeichenerfasserClient.ReserveNumberAsync(
            new ReserveSignatureSheetNumberRequest
            {
                CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection,
            });
        var id = await MuGoldachKontrollzeichenerfasserClient.AddAsync(NewValidRequest(x => x.Number = number.Number));
        var created = await RunOnDb(db => db.CollectionSignatureSheets.Include(x => x.CollectionMunicipality).FirstAsync(x => x.Id == Guid.Parse(id.Id)));
        created.CollectionMunicipality!.Bfs.Should().Be(Bfs.MunicipalityGoldach);
    }

    [Fact]
    public async Task ShouldThrowAsCt()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.AddAsync(NewValidRequest()),
            StatusCode.PermissionDenied);
    }

    [Fact]
    public async Task NotReservedNumberShouldThrow()
    {
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.AddAsync(NewValidRequest(x => x.Number = 20)),
            StatusCode.InvalidArgument,
            "ValidationException: Cannot use a number higher than the current counter");
    }

    [Fact]
    public async Task AlreadyUsedNumberShouldThrow()
    {
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.AddAsync(NewValidRequest(x => x.Number = 8)),
            StatusCode.InvalidArgument,
            "ValidationException: The number is already in use");
    }

    [Fact]
    public async Task NotPublishedShouldThrow()
    {
        await ModifyDbEntities(
            (ReferendumEntity e) => e.Id == ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
            e => e.CollectionStartDate = MockedClock.NowDateOnly.AddDays(1));
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.AddAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task MunicipalityLockedShouldThrow()
    {
        await ModifyDbEntities<CollectionMunicipalityEntity>(
            x => x.CollectionId == ReferendumsCtStGallen.GuidInCollectionEnabledForCollection && x.Bfs == Bfs.MunicipalityStGallen,
            x => x.IsLocked = true);
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.AddAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ReceivedAtInFutureShouldThrow()
    {
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.AddAsync(NewValidRequest(x => x.ReceivedAt = MockedClock.GetDate(1).ToProtoDate())),
            StatusCode.InvalidArgument,
            "Received at date can't be in the future.");
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionSignatureSheetService.CollectionSignatureSheetServiceClient(channel)
            .AddAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Kontrollzeichenerfasser;
    }

    private static AddSignatureSheetRequest NewValidRequest(Action<AddSignatureSheetRequest>? customizer = null)
    {
        var req = new AddSignatureSheetRequest
        {
            CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection,
            Number = 9,
            ReceivedAt = MockedClock.NowDateOnly.ToProtoDate(),
            SignatureCountTotal = 15,
        };

        customizer?.Invoke(req);
        return req;
    }
}
