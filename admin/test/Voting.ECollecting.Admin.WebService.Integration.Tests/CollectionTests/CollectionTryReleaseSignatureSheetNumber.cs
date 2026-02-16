// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionTryReleaseSignatureSheetNumber : BaseGrpcTest<CollectionSignatureSheetService.CollectionSignatureSheetServiceClient>
{
    public CollectionTryReleaseSignatureSheetNumber(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Referendums.WithReferendums(ReferendumsCtStGallen.GuidInCollectionEnabledForCollection) with
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
        await MuSgKontrollzeichenerfasserClient.TryReleaseNumberAsync(NewValidRequest());
        var ok = await RunOnDb(db => db.CollectionMunicipalities.AnyAsync(x =>
            x.Id == CollectionMunicipalities.BuildGuid(ReferendumsCtStGallen.GuidInCollectionEnabledForCollection, Bfs.MunicipalityStGallen)
            && x.NextSheetNumber == 9));
        ok.Should().BeTrue();
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await MuSgKontrollzeichenerfasserClient.TryReleaseNumberAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldThrowAsCt()
    {
        await AssertStatus(
            async () => await CtSgKontrollzeichenerfasserClient.TryReleaseNumberAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ReleaseAlreadyUsedShouldFail()
    {
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.TryReleaseNumberAsync(NewValidRequest(8)),
            StatusCode.InvalidArgument,
            "ValidationException: The number is already in use");
    }

    [Fact]
    public async Task ReleaseNotLatestNumberShouldSilentlyIgnore()
    {
        await MuSgKontrollzeichenerfasserClient.ReserveNumberAsync(new ReserveSignatureSheetNumberRequest
        {
            CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection,
        });

        // release unused but not latest
        await MuSgKontrollzeichenerfasserClient.TryReleaseNumberAsync(NewValidRequest());
    }

    [Fact]
    public async Task NotPublishedShouldThrow()
    {
        await ModifyDbEntities(
            (ReferendumEntity e) => e.Id == ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
            e => e.CollectionStartDate = MockedClock.NowDateOnly.AddDays(1));
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.TryReleaseNumberAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task MunicipalityLockedShouldThrow()
    {
        await ModifyDbEntities<CollectionMunicipalityEntity>(
            x => x.CollectionId == ReferendumsCtStGallen.GuidInCollectionEnabledForCollection && x.Bfs == Bfs.MunicipalityStGallen,
            x => x.IsLocked = true);
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.TryReleaseNumberAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionSignatureSheetService.CollectionSignatureSheetServiceClient(channel)
            .TryReleaseNumberAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Kontrollzeichenerfasser;
    }

    private static TryReleaseSignatureSheetNumberRequest NewValidRequest(int number = 9)
    {
        return new TryReleaseSignatureSheetNumberRequest
        {
            CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection,
            Number = number,
        };
    }
}
