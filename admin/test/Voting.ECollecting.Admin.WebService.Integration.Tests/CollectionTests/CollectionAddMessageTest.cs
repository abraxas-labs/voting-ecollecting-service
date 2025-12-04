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
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionAddMessageTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionAddMessageTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Default);
    }

    [Fact]
    public async Task ShouldWorkAsCtAdmin()
    {
        var id = await CtSgStammdatenverwalterClient.AddMessageAsync(NewValidRequest());
        id.Id.Should().NotBeEmpty();

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.SingleAsync(x => x.Id == Guid.Parse(id.Id)));
        await Verify(new { userNotifications, collectionMessage });
    }

    [Fact]
    public async Task ShouldWorkAsMuAdmin()
    {
        var id = await MuSgStammdatenverwalterClient.AddMessageAsync(new AddCollectionMessageRequest
        {
            CollectionId = ReferendumsMuStGallen.IdInCollectionActive,
            Content = "Hey there!",
        });
        id.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ShouldReturnNotFoundForMuAdminOnCtInitiative()
    {
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.AddMessageAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturnNotFoundForMuAdminOnOtherMuInitiative()
    {
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.AddMessageAsync(new AddCollectionMessageRequest
            {
                CollectionId = ReferendumsMuStGallen.IdInCollectionActive,
                Content = "Hey there!",
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowNotFoundForUnknownCollection()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.AddMessageAsync(new AddCollectionMessageRequest
            {
                CollectionId = "e239e756-e823-4193-b04c-1cf371ff9d2e",
                Content = "Hey there!",
            }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionService.CollectionServiceClient(channel)
            .AddMessageAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private AddCollectionMessageRequest NewValidRequest()
    {
        return new AddCollectionMessageRequest
        {
            CollectionId = InitiativesCtStGallen.IdLegislativeInPreparation,
            Content = "Hey there, nice to meet you!",
        };
    }
}
