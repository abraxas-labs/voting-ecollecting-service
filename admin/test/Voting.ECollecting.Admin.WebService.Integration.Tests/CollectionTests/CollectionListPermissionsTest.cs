// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Grpc.Net.Client;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionListPermissionsTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionListPermissionsTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Referendums with { SeedReferendumPermissions = true });
    }

    [Fact]
    public async Task ShouldListPermissions()
    {
        var messages = await CtSgStammdatenverwalterClient.ListPermissionsAsync(NewValidRequest());
        await Verify(messages);
    }

    [Fact]
    public async Task ShouldListPermissionsAsCtOnMu()
    {
        var messages = await CtSgStammdatenverwalterClient.ListPermissionsAsync(NewValidRequest(x => x.CollectionId = ReferendumsMuStGallen.IdInCollectionActive));
        await Verify(messages);
    }

    [Fact]
    public async Task AsMuOnOwnCollectionShouldWork()
    {
        var messages = await MuSgStammdatenverwalterClient.ListPermissionsAsync(NewValidRequest(x => x.CollectionId = ReferendumsMuStGallen.IdInCollectionActive));
        await Verify(messages);
    }

    [Fact]
    public async Task AsMuOnCtCollectionShouldFail()
    {
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.ListPermissionsAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsMuOnOtherMuCollectionShouldFail()
    {
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.ListPermissionsAsync(NewValidRequest(x => x.CollectionId = ReferendumsMuStGallen.IdInCollectionActive)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.ListPermissionsAsync(NewValidRequest(x => x.CollectionId = "db89aa7a-611d-4442-a52a-db8c6851e31f")),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionService.CollectionServiceClient(channel).ListPermissionsAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private ListCollectionPermissionsRequest NewValidRequest(Action<ListCollectionPermissionsRequest>? customizer = null)
    {
        var request = new ListCollectionPermissionsRequest
        {
            CollectionId = ReferendumsCtStGallen.IdInPreparation,
        };

        customizer?.Invoke(request);
        return request;
    }
}
