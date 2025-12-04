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
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.ReferendumTests;

public class ReferendumGetTest : BaseGrpcTest<ReferendumService.ReferendumServiceClient>
{
    public ReferendumGetTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Referendums.WithReferendums(ReferendumsCtStGallen.GuidInPreparation, ReferendumsMuStGallen.GuidInCollectionActive));
    }

    [Fact]
    public async Task ShouldGetReferendum()
    {
        var messages = await CtSgStammdatenverwalterClient.GetAsync(NewValidRequest());
        await Verify(messages);
    }

    [Fact]
    public async Task AsMuOnOwnCollectionShouldWork()
    {
        var messages = await MuSgStammdatenverwalterClient.GetAsync(NewValidRequest(x => x.Id = ReferendumsMuStGallen.IdInCollectionActive));
        await Verify(messages);
    }

    [Fact]
    public async Task AsMuOnCtCollectionShouldWork()
    {
        var messages = await MuSgStammdatenverwalterClient.GetAsync(NewValidRequest());
        await Verify(messages);
    }

    [Fact]
    public async Task AsMuOnOtherMuCollectionShouldFail()
    {
        var req = NewValidRequest(x => x.Id = ReferendumsMuStGallen.IdInCollectionActive);
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.GetAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsCtOnMuCollectionShouldWork()
    {
        var req = NewValidRequest(x => x.Id = ReferendumsMuStGallen.IdInCollectionActive);
        var resp = await CtSgStammdatenverwalterClient.GetAsync(req);
        resp.Id.Should().Be(ReferendumsMuStGallen.IdInCollectionActive);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.GetAsync(NewValidRequest(x => x.Id = "d6a86ea5-7a47-4ebb-8dfe-f2b71f87206e")),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new ReferendumService.ReferendumServiceClient(channel).GetAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        return Roles.AllHumanUserRoles();
    }

    private GetReferendumRequest NewValidRequest(Action<GetReferendumRequest>? customizer = null)
    {
        var request = new GetReferendumRequest
        {
            Id = ReferendumsCtStGallen.IdInPreparation,
        };

        customizer?.Invoke(request);
        return request;
    }
}
