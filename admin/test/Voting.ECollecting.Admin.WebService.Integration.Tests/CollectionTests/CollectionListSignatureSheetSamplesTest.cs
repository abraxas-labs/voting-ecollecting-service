// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Grpc.Net.Client;
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

public class CollectionListSignatureSheetSamplesTest : BaseGrpcTest<CollectionSignatureSheetService.CollectionSignatureSheetServiceClient>
{
    public CollectionListSignatureSheetSamplesTest(TestApplicationFactory factory)
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
            e => (e.CollectionMunicipality!.CollectionId == ReferendumsCtStGallen.GuidSignatureSheetsSubmitted ||
                  e.CollectionMunicipality!.CollectionId == ReferendumsMuStGallen.GuidSignatureSheetsSubmitted) &&
                 e.State == CollectionSignatureSheetState.Submitted,
            e => e.IsSample = true);
    }

    [Fact]
    public async Task ShouldWork()
    {
        var req = NewValidRequest();
        var response = await CtSgStichprobenverwalterClient.ListSamplesAsync(req);
        await Verify(response);
    }

    [Fact]
    public async Task ShouldWorkAsMu()
    {
        var req = NewValidRequest();
        req.CollectionId = ReferendumsMuStGallen.IdSignatureSheetsSubmitted;
        var response = await MuSgStichprobenverwalterClient.ListSamplesAsync(req);
        await Verify(response);
    }

    [Fact]
    public async Task ShouldThrowAsCtOnMu()
    {
        var req = NewValidRequest();
        req.CollectionId = ReferendumsMuStGallen.IdSignatureSheetsSubmitted;
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.ListSamplesAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsMuOnCt()
    {
        await AssertStatus(
            async () => await MuSgStichprobenverwalterClient.ListSamplesAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowCollectionNotFound()
    {
        var req = NewValidRequest();
        req.CollectionId = "326ee699-1595-4fe4-a706-f656cb9c68ef";
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.ListSamplesAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowOtherTenant()
    {
        var req = NewValidRequest();
        req.CollectionId = ReferendumsMuGoldach.IdSignatureSheetsSubmitted;
        await AssertStatus(
            async () => await MuSgStichprobenverwalterClient.ListSamplesAsync(req),
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
                async () => await CtSgStichprobenverwalterClient.ListSamplesAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
        else
        {
            await CtSgStichprobenverwalterClient.ListSamplesAsync(NewValidRequest());
        }
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionSignatureSheetService.CollectionSignatureSheetServiceClient(channel)
            .ListSamplesAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stichprobenverwalter;
    }

    private static ListSignatureSheetSamplesRequest NewValidRequest()
    {
        return new ListSignatureSheetSamplesRequest
        {
            CollectionId = ReferendumsCtStGallen.IdSignatureSheetsSubmitted,
        };
    }
}
