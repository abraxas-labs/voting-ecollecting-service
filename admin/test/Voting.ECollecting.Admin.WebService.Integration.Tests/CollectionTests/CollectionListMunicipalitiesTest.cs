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

public class CollectionListMunicipalitiesTest : BaseGrpcTest<CollectionMunicipalityService.CollectionMunicipalityServiceClient>
{
    public CollectionListMunicipalitiesTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Referendums.WithReferendums(ReferendumsCtStGallen.GuidSignatureSheetsSubmitted, ReferendumsMuStGallen.GuidSignatureSheetsSubmitted) with
        {
            SeedReferendumSignatureSheets = true,
        });
    }

    [Fact]
    public async Task ShouldWork()
    {
        var municipalities = await CtSgStichprobenverwalterClient.ListAsync(NewValidRequest());
        await Verify(municipalities);
    }

    [Fact]
    public async Task ShouldWorkAsMu()
    {
        var req = NewValidRequest(x => x.CollectionId = ReferendumsMuStGallen.IdSignatureSheetsSubmitted);
        var municipalities = await MuSgStichprobenverwalterClient.ListAsync(req);
        await Verify(municipalities);
    }

    [Fact]
    public async Task ShouldThrowAsMuOnCt()
    {
        await AssertStatus(
            async () => await MuSgStichprobenverwalterClient.ListAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsCtOnMu()
    {
        var req = NewValidRequest(x => x.CollectionId = ReferendumsMuStGallen.IdSignatureSheetsSubmitted);
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.ListAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowOtherMuTenant()
    {
        var req = NewValidRequest(x => x.CollectionId = ReferendumsMuStGallen.IdSignatureSheetsSubmitted);
        await AssertStatus(
            async () => await MuGoldachKontrollzeichenerfasserClient.ListAsync(req),
            StatusCode.PermissionDenied);
    }

    [Fact]
    public async Task ShouldThrowNotFound()
    {
        var req = NewValidRequest(x => x.CollectionId = "32998294-161a-4d8d-a62f-2d7b4342d43a");
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.ListAsync(req),
            StatusCode.NotFound);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInState(CollectionState state)
    {
        await ModifyDbEntities<ReferendumEntity>(
            e => e.Id == ReferendumsCtStGallen.GuidSignatureSheetsSubmitted,
            e => e.State = state);

        if (state.IsEnded())
        {
            await CtSgStichprobenverwalterClient.ListAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await CtSgStichprobenverwalterClient.ListAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionMunicipalityService.CollectionMunicipalityServiceClient(channel)
            .ListAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stichprobenverwalter;
    }

    private static ListCollectionMunicipalitiesRequest NewValidRequest(Action<ListCollectionMunicipalitiesRequest>? customizer = null)
    {
        var req = new ListCollectionMunicipalitiesRequest
        {
            CollectionId = ReferendumsCtStGallen.IdSignatureSheetsSubmitted,
        };
        customizer?.Invoke(req);
        return req;
    }
}
