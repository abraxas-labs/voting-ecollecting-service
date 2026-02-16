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

public class CollectionDeleteSignatureSheetTest : BaseGrpcTest<CollectionSignatureSheetService.CollectionSignatureSheetServiceClient>
{
    private static readonly Guid _sheetId = CollectionSignatureSheets.BuildGuid(
        CollectionMunicipalities.BuildGuid(
            ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
            Bfs.MunicipalityStGallen),
        1);

    public CollectionDeleteSignatureSheetTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Referendums.WithReferendums(
                    ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
                    ReferendumsCtStGallen.GuidInCollectionEnabledForCollection2,
                    ReferendumsMuStGallen.GuidInCollectionActive) with
            {
                SeedReferendumSignatureSheets = true,
            });
    }

    [Fact]
    public async Task ShouldWork()
    {
        await MuSgKontrollzeichenerfasserClient.DeleteAsync(NewValidRequest());
        var exists = await RunOnDb(db => db.CollectionSignatureSheets.AnyAsync(x => x.Id == _sheetId));
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await MuSgKontrollzeichenerfasserClient.DeleteAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task AsMuOnMuCollectionShouldWork()
    {
        var sheetId = CollectionSignatureSheets.BuildGuid(
            CollectionMunicipalities.BuildGuid(
                ReferendumsMuStGallen.GuidInCollectionActive,
                Bfs.MunicipalityStGallen),
            1);
        await MuSgKontrollzeichenerfasserClient.DeleteAsync(new DeleteSignatureSheetRequest
        {
            CollectionId = ReferendumsMuStGallen.IdInCollectionActive,
            SignatureSheetId = sheetId.ToString(),
        });
        var exists = await RunOnDb(db => db.CollectionSignatureSheets.AnyAsync(x => x.Id == sheetId));
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldThrowSheetNotFound()
    {
        var req = NewValidRequest();
        req.SignatureSheetId = "fef93b19-2f28-4d33-8486-d847148607e2";
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.DeleteAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowCollectionNotFound()
    {
        var req = NewValidRequest();
        req.CollectionId = "f48d4ff0-bdf5-45f8-9ec3-40a3f08da46b";
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.DeleteAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowOtherCollection()
    {
        var req = NewValidRequest();
        req.CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection2;
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.DeleteAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowOtherTenant()
    {
        var req = NewValidRequest();
        req.SignatureSheetId = CollectionSignatureSheets.BuildGuid(
            CollectionMunicipalities.BuildGuid(
                ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
                Bfs.MunicipalityBergSG),
            1).ToString();
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.DeleteAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAttestedState()
    {
        var req = NewValidRequest();
        req.SignatureSheetId = CollectionSignatureSheets.BuildGuid(
            CollectionMunicipalities.BuildGuid(
                ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
                Bfs.MunicipalityStGallen),
            4).ToString();
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.DeleteAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task NotPublishedShouldThrow()
    {
        await ModifyDbEntities(
            (ReferendumEntity e) => e.Id == ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
            e => e.CollectionStartDate = MockedClock.NowDateOnly.AddDays(1));
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.DeleteAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task MunicipalityLockedShouldThrow()
    {
        await ModifyDbEntities<CollectionMunicipalityEntity>(
            x => x.CollectionId == ReferendumsCtStGallen.GuidInCollectionEnabledForCollection && x.Bfs == Bfs.MunicipalityStGallen,
            x => x.IsLocked = true);
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.DeleteAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionSignatureSheetService.CollectionSignatureSheetServiceClient(channel)
            .DeleteAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Kontrollzeichenerfasser;
    }

    private static DeleteSignatureSheetRequest NewValidRequest()
    {
        return new DeleteSignatureSheetRequest
        {
            CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection,
            SignatureSheetId = _sheetId.ToString(),
        };
    }
}
