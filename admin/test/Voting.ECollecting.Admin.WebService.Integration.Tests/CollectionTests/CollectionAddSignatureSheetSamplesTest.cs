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
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionAddSignatureSheetSamplesTest : BaseGrpcTest<CollectionSignatureSheetService.CollectionSignatureSheetServiceClient>
{
    private static readonly Guid _municipalityCtSgId = CollectionMunicipalities.BuildGuid(
        ReferendumsCtStGallen.GuidSignatureSheetsSubmitted,
        Bfs.MunicipalityStGallen);

    private static readonly Guid _sheetCtSgId = CollectionSignatureSheets.BuildGuid(_municipalityCtSgId, 1);

    public CollectionAddSignatureSheetSamplesTest(TestApplicationFactory factory)
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
            });

        await ModifyDbEntities<CollectionSignatureSheetEntity>(
            e => (e.CollectionMunicipality!.CollectionId == ReferendumsCtStGallen.GuidSignatureSheetsSubmitted || e.CollectionMunicipality.CollectionId == ReferendumsMuStGallen.GuidSignatureSheetsSubmitted) && e.State == CollectionSignatureSheetState.Attested,
            e => e.State = CollectionSignatureSheetState.Submitted);
    }

    [Fact]
    public async Task ShouldWork()
    {
        var req = NewValidRequest();
        var response = await CtSgStichprobenverwalterClient.AddSamplesAsync(req);

        // cannot snapshot test the signature sheets, since they are selected randomly
        response.SignatureSheets.Count.Should().Be(req.SignatureSheetsCount);
        var responseIds = response.SignatureSheets.Select(s => s.Id).ToList();
        var sheets = await RunOnDb(db =>
            db.CollectionSignatureSheets.Where(x => responseIds.Contains(x.Id.ToString())).ToListAsync());
        sheets.Should().AllSatisfy(x => x.IsSample.Should().BeTrue());
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStichprobenverwalterClient.AddSamplesAsync(NewValidRequest());

            var result = await GetAuditTrailEntries();
            result.AuditTrailEntries.Count(e => e.SourceEntityName == "CollectionSignatureSheets" && e.Action == "Modified")
                .Should().Be(2);
        });
    }

    [Fact]
    public async Task ShouldWorkAsMu()
    {
        var req = NewValidRequest();
        req.CollectionId = ReferendumsMuStGallen.IdSignatureSheetsSubmitted;
        var response = await MuSgStichprobenverwalterClient.AddSamplesAsync(req);

        // cannot snapshot test the signature sheets, since they are selected randomly
        response.SignatureSheets.Count.Should().Be(req.SignatureSheetsCount);
        var responseIds = response.SignatureSheets.Select(s => s.Id).ToList();
        var sheets = await RunOnDb(db =>
            db.CollectionSignatureSheets.Where(x => responseIds.Contains(x.Id.ToString())).ToListAsync());
        sheets.Should().AllSatisfy(x => x.IsSample.Should().BeTrue());
    }

    [Fact]
    public async Task ShouldWorkWithAllPossibleSignatureSheets()
    {
        var signatureSheetsCount = await RunOnDb(db =>
            db.CollectionSignatureSheets.Where(x =>
                x.CollectionMunicipality!.CollectionId == ReferendumsCtStGallen.GuidSignatureSheetsSubmitted &&
                !x.IsSample &&
                x.State == CollectionSignatureSheetState.Submitted).CountAsync());

        var req = NewValidRequest();
        req.SignatureSheetsCount = signatureSheetsCount;
        var response = await CtSgStichprobenverwalterClient.AddSamplesAsync(req);
        await Verify(response);

        var responseIds = response.SignatureSheets.Select(s => s.Id).ToList();
        var sheets = await RunOnDb(db =>
            db.CollectionSignatureSheets.Where(x => responseIds.Contains(x.Id.ToString())).ToListAsync());
        sheets.Should().AllSatisfy(x => x.IsSample.Should().BeTrue());
    }

    [Fact]
    public async Task ShouldThrowAsCtOnMu()
    {
        var req = NewValidRequest();
        req.CollectionId = ReferendumsMuStGallen.IdSignatureSheetsSubmitted;
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.AddSamplesAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsMuOnCt()
    {
        await AssertStatus(
            async () => await MuSgStichprobenverwalterClient.AddSamplesAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowCollectionNotFound()
    {
        var req = NewValidRequest();
        req.CollectionId = "326ee699-1595-4fe4-a706-f656cb9c68ef";
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.AddSamplesAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowTooManyCollectionSignatureSheetSamples()
    {
        var req = NewValidRequest();
        req.SignatureSheetsCount = 100;
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.AddSamplesAsync(req),
            StatusCode.InvalidArgument,
            "TooManyCollectionSignatureSheetSamples");
    }

    [Fact]
    public async Task ShouldThrowOtherTenant()
    {
        var req = NewValidRequest();
        req.CollectionId = ReferendumsMuGoldach.IdSignatureSheetsSubmitted;
        await AssertStatus(
            async () => await MuSgStichprobenverwalterClient.AddSamplesAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowNotAllSignatureSheetsPastAttested()
    {
        await ModifyDbEntities<CollectionSignatureSheetEntity>(
            e => e.Id == _sheetCtSgId,
            e => e.State = CollectionSignatureSheetState.Attested);

        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.AddSamplesAsync(NewValidRequest()),
            StatusCode.InvalidArgument,
            "All signature sheets must be past attested.");
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
                async () => await CtSgStichprobenverwalterClient.AddSamplesAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
        else
        {
            await CtSgStichprobenverwalterClient.AddSamplesAsync(NewValidRequest());
        }
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionSignatureSheetService.CollectionSignatureSheetServiceClient(channel)
            .AddSamplesAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stichprobenverwalter;
    }

    private static AddSignatureSheetSamplesRequest NewValidRequest()
    {
        return new AddSignatureSheetSamplesRequest
        {
            CollectionId = ReferendumsCtStGallen.IdSignatureSheetsSubmitted,
            SignatureSheetsCount = 2,
        };
    }
}
