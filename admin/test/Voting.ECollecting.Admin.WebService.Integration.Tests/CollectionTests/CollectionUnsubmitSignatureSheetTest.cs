// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net.Http.Json;
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
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionUnsubmitSignatureSheetTest : BaseGrpcTest<CollectionSignatureSheetService.CollectionSignatureSheetServiceClient>
{
    private static readonly Guid _municipalityCtSgId = CollectionMunicipalities.BuildGuid(
        ReferendumsCtStGallen.GuidSignatureSheetsSubmitted,
        Bfs.MunicipalityStGallen);

    private static readonly Guid _municipalityMuSgId = CollectionMunicipalities.BuildGuid(
        ReferendumsMuStGallen.GuidSignatureSheetsSubmitted,
        Bfs.MunicipalityStGallen);

    private static readonly Guid _sheetCtSgId = CollectionSignatureSheets.BuildGuid(_municipalityCtSgId, 7);
    private static readonly Guid _sheetMuSgId = CollectionSignatureSheets.BuildGuid(_municipalityMuSgId, 7);

    public CollectionUnsubmitSignatureSheetTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Referendums.WithReferendums(ReferendumsCtStGallen.GuidSignatureSheetsSubmitted, ReferendumsCtStGallen.GuidPastEndedCameAbout, ReferendumsMuStGallen.GuidSignatureSheetsSubmitted, ReferendumsMuGoldach.GuidSignatureSheetsSubmitted) with
            {
                SeedReferendumSignatureSheets = true,
                SeedDomainOfInfluences = true,
            });
    }

    [Fact]
    public async Task ShouldWork()
    {
        var prevCount = await RunOnDb(db => db.CollectionCounts
            .SingleAsync(x => x.CollectionId == ReferendumsCtStGallen.GuidSignatureSheetsSubmitted));

        var prevCollectionMunicipality = await RunOnDb(db => db.CollectionMunicipalities
            .SingleAsync(x => x.Id == _municipalityCtSgId));

        var sheet = await RunOnDb(db => db.CollectionSignatureSheets
            .SingleAsync(x => x.Id == _sheetCtSgId));

        var response = await CtSgStichprobenverwalterClient.UnsubmitAsync(NewValidRequest());
        await Verify(response);

        var collectionMunicipality = await RunOnDb(db => db.CollectionMunicipalities
            .SingleAsync(x => x.Id == _municipalityCtSgId));

        collectionMunicipality.PhysicalCount.Valid.Should().Be(prevCollectionMunicipality.PhysicalCount.Valid - sheet.Count.Valid);
        collectionMunicipality.PhysicalCount.Invalid.Should().Be(prevCollectionMunicipality.PhysicalCount.Invalid - sheet.Count.Invalid);
        response.CollectionCount.ElectronicCitizenCount.Should().Be(prevCount.ElectronicCitizenCount);
        response.CollectionCount.TotalCitizenCount.Should().Be(prevCount.TotalCitizenCount - sheet.Count.Valid);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStichprobenverwalterClient.UnsubmitAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldWorkAsMu()
    {
        var prevCount = await RunOnDb(db => db.CollectionCounts
            .SingleAsync(x => x.CollectionId == ReferendumsMuStGallen.GuidSignatureSheetsSubmitted));

        var prevCollectionMunicipality = await RunOnDb(db => db.CollectionMunicipalities
            .SingleAsync(x => x.Id == _municipalityMuSgId));

        var sheet = await RunOnDb(db => db.CollectionSignatureSheets
            .SingleAsync(x => x.Id == _sheetMuSgId));

        var req = NewValidRequest();
        req.CollectionId = ReferendumsMuStGallen.IdSignatureSheetsSubmitted;
        req.SignatureSheetId = _sheetMuSgId.ToString();
        var response = await MuSgStichprobenverwalterClient.UnsubmitAsync(req);
        await Verify(response);

        var collectionMunicipality = await RunOnDb(db => db.CollectionMunicipalities
            .SingleAsync(x => x.Id == _municipalityMuSgId));

        collectionMunicipality.PhysicalCount.Valid.Should().Be(prevCollectionMunicipality.PhysicalCount.Valid - sheet.Count.Valid);
        collectionMunicipality.PhysicalCount.Invalid.Should().Be(prevCollectionMunicipality.PhysicalCount.Invalid - sheet.Count.Invalid);
        response.CollectionCount.ElectronicCitizenCount.Should().Be(prevCount.ElectronicCitizenCount);
        response.CollectionCount.TotalCitizenCount.Should().Be(prevCount.TotalCitizenCount - sheet.Count.Valid);
    }

    [Fact]
    public async Task ShouldWorkWithParallelAttest()
    {
        var prevCount = await RunOnDb(db => db.CollectionCounts
            .SingleAsync(x => x.CollectionId == ReferendumsCtStGallen.GuidSignatureSheetsSubmitted));

        var prevCollectionMunicipality = await RunOnDb(db => db.CollectionMunicipalities
            .SingleAsync(x => x.Id == _municipalityCtSgId));

        var sheet = await RunOnDb(db => db.CollectionSignatureSheets
            .SingleAsync(x => x.Id == _sheetCtSgId));

        var submitCall = CtSgStichprobenverwalterClient.UnsubmitAsync(NewValidRequest()).ResponseAsync;

        await ModifyDbEntities<CollectionMunicipalityEntity>(
            x => x.CollectionId == ReferendumsCtStGallen.GuidSignatureSheetsSubmitted && x.Bfs == Bfs.MunicipalityStGallen,
            x => x.IsLocked = false);

        HashSet<Guid> sheetIds =
        [
            CollectionSignatureSheets.BuildGuid(
                CollectionMunicipalities.BuildGuid(
                    ReferendumsCtStGallen.GuidSignatureSheetsSubmitted,
                    Bfs.MunicipalityStGallen),
                1),
            CollectionSignatureSheets.BuildGuid(
                CollectionMunicipalities.BuildGuid(
                    ReferendumsCtStGallen.GuidSignatureSheetsSubmitted,
                    Bfs.MunicipalityStGallen),
                2),
        ];
        var muSgKontrollzeichenerstellerClient = CreateHttpClient(tenant: MockedTenantIds.MUSG, roles: Roles.Kontrollzeichenerfasser);
        var resp = await muSgKontrollzeichenerstellerClient.PostAsJsonAsync($"v1/api/collections/{ReferendumsCtStGallen.IdSignatureSheetsSubmitted}/signature-sheets/attest", sheetIds);
        resp.EnsureSuccessStatusCode();

        var response = await submitCall;

        var collectionMunicipality = await RunOnDb(db => db.CollectionMunicipalities
            .SingleAsync(x => x.Id == _municipalityCtSgId));

        collectionMunicipality.PhysicalCount.Valid.Should().Be(prevCollectionMunicipality.PhysicalCount.Valid - sheet.Count.Valid);
        collectionMunicipality.PhysicalCount.Invalid.Should().Be(prevCollectionMunicipality.PhysicalCount.Invalid - sheet.Count.Invalid);
        response.CollectionCount.ElectronicCitizenCount.Should().Be(prevCount.ElectronicCitizenCount);
        response.CollectionCount.TotalCitizenCount.Should().Be(prevCount.TotalCitizenCount - sheet.Count.Valid);
    }

    [Fact]
    public async Task ShouldThrowAsCtOnMu()
    {
        var req = NewValidRequest();
        req.CollectionId = ReferendumsMuStGallen.IdSignatureSheetsSubmitted;
        req.SignatureSheetId = _sheetMuSgId.ToString();
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.UnsubmitAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsMuOnCt()
    {
        await AssertStatus(
            async () => await MuSgStichprobenverwalterClient.UnsubmitAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowNotSubmitted()
    {
        await ModifyDbEntities<CollectionSignatureSheetEntity>(
            e => e.Id == _sheetCtSgId,
            e => e.State = CollectionSignatureSheetState.Attested);
        await AssertStatus(
            async () => await MuSgStichprobenverwalterClient.UnsubmitAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowSheetNotFound()
    {
        var req = NewValidRequest();
        req.SignatureSheetId = "671695d0-7761-4550-9473-f676ae44332f";
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.UnsubmitAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowCollectionNotFound()
    {
        var req = NewValidRequest();
        req.CollectionId = "326ee699-1595-4fe4-a706-f656cb9c68ef";
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.UnsubmitAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowOtherCollection()
    {
        var req = NewValidRequest();
        req.CollectionId = ReferendumsCtStGallen.IdPastEndedCameAbout;
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.UnsubmitAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowOtherTenant()
    {
        var req = NewValidRequest();
        req.CollectionId = ReferendumsMuGoldach.IdSignatureSheetsSubmitted;
        req.SignatureSheetId = CollectionSignatureSheets.BuildGuid(
            CollectionMunicipalities.BuildGuid(
                ReferendumsMuGoldach.GuidSignatureSheetsSubmitted,
                Bfs.MunicipalityGoldach),
            7).ToString();
        await AssertStatus(
            async () => await MuSgStichprobenverwalterClient.UnsubmitAsync(req),
            StatusCode.NotFound);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInState(CollectionState state)
    {
        await ModifyDbEntities<ReferendumEntity>(
            e => e.Id == ReferendumsCtStGallen.GuidSignatureSheetsSubmitted,
            e => e.State = state);

        if (!state.IsEnded())
        {
            await AssertStatus(
                async () => await CtSgStichprobenverwalterClient.UnsubmitAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
        else
        {
            await CtSgStichprobenverwalterClient.UnsubmitAsync(NewValidRequest());
        }
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionSignatureSheetService.CollectionSignatureSheetServiceClient(channel)
            .UnsubmitAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stichprobenverwalter;
    }

    private static UnsubmitSignatureSheetRequest NewValidRequest()
    {
        return new UnsubmitSignatureSheetRequest
        {
            CollectionId = ReferendumsCtStGallen.IdSignatureSheetsSubmitted,
            SignatureSheetId = _sheetCtSgId.ToString(),
        };
    }
}
