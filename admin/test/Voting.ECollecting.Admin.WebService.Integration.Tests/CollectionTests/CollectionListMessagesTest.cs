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

public class CollectionListMessagesTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionListMessagesTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Referendums with { SeedReferendumMessages = true });
    }

    [Fact]
    public async Task ShouldWorkAsCtAdmin()
    {
        var messages = await CtSgStammdatenverwalterClient.ListMessagesAsync(NewValidRequest());
        await Verify(messages);
    }

    [Fact]
    public async Task ShouldWorkAsCtAdminOnMu()
    {
        var messages = await CtSgStammdatenverwalterClient.ListMessagesAsync(new ListCollectionMessagesRequest
        {
            CollectionId = ReferendumsMuStGallen.IdInCollectionActive,
        });
        await Verify(messages);
    }

    [Fact]
    public async Task ShouldWorkAsMuAdmin()
    {
        var messages = await MuSgStammdatenverwalterClient.ListMessagesAsync(new ListCollectionMessagesRequest
        {
            CollectionId = ReferendumsMuStGallen.IdInCollectionActive,
        });
        await Verify(messages);
    }

    [Fact]
    public async Task ShouldReturnNotFoundForMuAdminOnCtInitiative()
    {
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.ListMessagesAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturnNotFoundForMuAdminOnOtherMuInitiative()
    {
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.ListMessagesAsync(new ListCollectionMessagesRequest
            {
                CollectionId = ReferendumsMuStGallen.IdInCollectionActive,
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowNotFoundForUnknownCollection()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.ListMessagesAsync(new ListCollectionMessagesRequest
            {
                CollectionId = "e239e756-e823-4193-b04c-1cf371ff9d2e",
            }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionService.CollectionServiceClient(channel)
            .ListMessagesAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private ListCollectionMessagesRequest NewValidRequest()
    {
        return new ListCollectionMessagesRequest
        {
            CollectionId = ReferendumsCtStGallen.IdInPreparation,
        };
    }
}
