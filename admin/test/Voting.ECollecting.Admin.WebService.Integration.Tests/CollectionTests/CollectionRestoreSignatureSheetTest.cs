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

public class CollectionRestoreSignatureSheetTest : BaseGrpcTest<CollectionSignatureSheetService.CollectionSignatureSheetServiceClient>
{
    private static readonly Guid _municipalityCtSgId = CollectionMunicipalities.BuildGuid(
        ReferendumsCtStGallen.GuidSignatureSheetsSubmitted,
        Bfs.MunicipalityStGallen);

    private static readonly Guid _municipalityMuSgId = CollectionMunicipalities.BuildGuid(
        ReferendumsMuStGallen.GuidSignatureSheetsSubmitted,
        Bfs.MunicipalityStGallen);

    private static readonly Guid _sheetCtSgId = CollectionSignatureSheets.BuildGuid(_municipalityCtSgId, 8);
    private static readonly Guid _sheetMuSgId = CollectionSignatureSheets.BuildGuid(_municipalityMuSgId, 8);

    public CollectionRestoreSignatureSheetTest(TestApplicationFactory factory)
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
    }

    [Fact]
    public async Task ShouldWork()
    {
        var response = await CtSgStichprobenverwalterClient.RestoreAsync(NewValidRequest());
        await Verify(response);

        var sheet = await RunOnDb(db => db.CollectionSignatureSheets
            .SingleAsync(x => x.Id == _sheetCtSgId));

        sheet.State.Should().Be(CollectionSignatureSheetState.Attested);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStichprobenverwalterClient.RestoreAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldWorkAsMu()
    {
        var req = NewValidRequest();
        req.CollectionId = ReferendumsMuStGallen.IdSignatureSheetsSubmitted;
        req.SignatureSheetId = _sheetMuSgId.ToString();
        var response = await MuSgStichprobenverwalterClient.RestoreAsync(req);
        await Verify(response);

        var sheet = await RunOnDb(db => db.CollectionSignatureSheets
            .SingleAsync(x => x.Id == _sheetMuSgId));

        sheet.State.Should().Be(CollectionSignatureSheetState.Attested);
    }

    [Fact]
    public async Task ShouldThrowAsCtOnMu()
    {
        var req = NewValidRequest();
        req.CollectionId = ReferendumsMuStGallen.IdSignatureSheetsSubmitted;
        req.SignatureSheetId = _sheetMuSgId.ToString();
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.RestoreAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsMuOnCt()
    {
        await AssertStatus(
            async () => await MuSgStichprobenverwalterClient.RestoreAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowNotNotSubmitted()
    {
        await ModifyDbEntities<CollectionSignatureSheetEntity>(
            e => e.Id == _sheetCtSgId,
            e => e.State = CollectionSignatureSheetState.Submitted);
        await AssertStatus(
            async () => await MuSgStichprobenverwalterClient.RestoreAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowSheetNotFound()
    {
        var req = NewValidRequest();
        req.SignatureSheetId = "671695d0-7761-4550-9473-f676ae44332f";
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.RestoreAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowCollectionNotFound()
    {
        var req = NewValidRequest();
        req.CollectionId = "326ee699-1595-4fe4-a706-f656cb9c68ef";
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.RestoreAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowOtherCollection()
    {
        var req = NewValidRequest();
        req.CollectionId = ReferendumsCtStGallen.IdPastEndedCameAbout;
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.RestoreAsync(req),
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
            8).ToString();
        await AssertStatus(
            async () => await MuSgStichprobenverwalterClient.RestoreAsync(req),
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
                async () => await CtSgStichprobenverwalterClient.RestoreAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
        else
        {
            await CtSgStichprobenverwalterClient.RestoreAsync(NewValidRequest());
        }
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionSignatureSheetService.CollectionSignatureSheetServiceClient(channel)
            .RestoreAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stichprobenverwalter;
    }

    private static RestoreSignatureSheetRequest NewValidRequest()
    {
        return new RestoreSignatureSheetRequest
        {
            CollectionId = ReferendumsCtStGallen.IdSignatureSheetsSubmitted,
            SignatureSheetId = _sheetCtSgId.ToString(),
        };
    }
}
