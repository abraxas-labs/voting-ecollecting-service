// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

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

public class CollectionFinishInformalReviewTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionFinishInformalReviewTest(TestApplicationFactory factory)
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
        await RunOnDb(db => db.Referendums
            .Where(x => x.Id == ReferendumsCtStGallen.GuidInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.InformalReviewRequested, true)));
        var message = await CtSgStammdatenverwalterClient.FinishInformalReviewAsync(NewValidRequest());
        await Verify(message);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await RunOnDb(db => db.Referendums
            .Where(x => x.Id == ReferendumsCtStGallen.GuidInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.InformalReviewRequested, true)));
            await CtSgStammdatenverwalterClient.FinishInformalReviewAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldWorkAsMuAdmin()
    {
        await RunOnDb(db => db.Referendums
            .Where(x => x.Id == ReferendumsMuStGallen.GuidInCollectionActive)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.InformalReviewRequested, true)));
        var message = await MuSgStammdatenverwalterClient.FinishInformalReviewAsync(new FinishInformalReviewRequest
        {
            CollectionId = ReferendumsMuStGallen.IdInCollectionActive,
        });
        await Verify(message);
    }

    [Fact]
    public async Task ShouldThrowAlreadyFinished()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.FinishInformalReviewAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task ShouldReturnNotFoundForMuAdminOnCtInitiative()
    {
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.FinishInformalReviewAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturnNotFoundForMuAdminOnOtherMuInitiative()
    {
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.FinishInformalReviewAsync(new FinishInformalReviewRequest
            {
                CollectionId = ReferendumsMuStGallen.IdInCollectionActive,
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowNotFoundForUnknownCollection()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.FinishInformalReviewAsync(new FinishInformalReviewRequest
            {
                CollectionId = "e239e756-e823-4193-b04c-1cf371ff9d2e",
            }),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionService.CollectionServiceClient(channel)
            .FinishInformalReviewAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private FinishInformalReviewRequest NewValidRequest()
    {
        return new FinishInformalReviewRequest
        {
            CollectionId = ReferendumsCtStGallen.IdInPreparation,
        };
    }
}
