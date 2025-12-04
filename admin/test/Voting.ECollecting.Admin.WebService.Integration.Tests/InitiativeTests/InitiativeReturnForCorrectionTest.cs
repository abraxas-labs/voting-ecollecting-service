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
using Voting.ECollecting.Shared.Test.Utils;
using CollectionState = Voting.ECollecting.Shared.Domain.Enums.CollectionState;
using InitiativeLockedFields = Voting.ECollecting.Proto.Admin.Services.V1.Models.InitiativeLockedFields;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class InitiativeReturnForCorrectionTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeReturnForCorrectionTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeUnderReview, InitiativesMuStGallen.GuidUnderReview));
    }

    [Fact]
    public async Task ShouldReturnInitiativeForCorrection()
    {
        await CtSgStammdatenverwalterClient.ReturnForCorrectionAsync(NewValidRequest());

        var initiative = await RunOnDb(db => db.Initiatives
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeUnderReview));
        initiative.State.Should().Be(CollectionState.ReturnedForCorrection);
        initiative.SetPeriodState(GetService<TimeProvider>().GetUtcNowDateTime());

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCtStGallen.GuidLegislativeUnderReview)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesCtStGallen.GuidLegislativeUnderReview));
        await Verify(new { userNotifications, collectionMessage, initiative });
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStammdatenverwalterClient.ReturnForCorrectionAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task AsMuOnOwnCollectionShouldWork()
    {
        await MuSgStammdatenverwalterClient.ReturnForCorrectionAsync(NewValidRequest(x => x.Id = InitiativesMuStGallen.IdUnderReview));

        var initiative = await RunOnDb(db => db.Initiatives
            .FirstAsync(x => x.Id == InitiativesMuStGallen.GuidUnderReview));
        initiative.State.Should().Be(CollectionState.ReturnedForCorrection);
        initiative.SetPeriodState(GetService<TimeProvider>().GetUtcNowDateTime());

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesMuStGallen.GuidUnderReview)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesMuStGallen.GuidUnderReview));
        await Verify(new { userNotifications, collectionMessage, initiative });
    }

    [Fact]
    public async Task AsMuOnCtCollectionShouldFail()
    {
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.ReturnForCorrectionAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsMuOnOtherMuCollectionShouldFail()
    {
        var req = NewValidRequest(x => x.Id = InitiativesMuStGallen.IdUnderReview);
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.ReturnForCorrectionAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsCtOnMuCollectionShouldFail()
    {
        var req = NewValidRequest(x => x.Id = InitiativesMuStGallen.IdUnderReview);
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.ReturnForCorrectionAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.ReturnForCorrectionAsync(NewValidRequest(x => x.Id = "90c0535d-81f8-44fa-b24f-fa6b46f53cd8")),
            StatusCode.NotFound);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeUnderReview)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        if (state is CollectionState.Submitted or CollectionState.UnderReview)
        {
            await CtSgStammdatenverwalterClient.ReturnForCorrectionAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await CtSgStammdatenverwalterClient.ReturnForCorrectionAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new InitiativeService.InitiativeServiceClient(channel)
            .ReturnForCorrectionAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private ReturnInitiativeForCorrectionRequest NewValidRequest(Action<ReturnInitiativeForCorrectionRequest>? customizer = null)
    {
        var request = new ReturnInitiativeForCorrectionRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeUnderReview,
            LockedFields = new InitiativeLockedFields
            {
                Description = true,
                Wording = false,
                CommitteeMembers = true,
            },
        };
        customizer?.Invoke(request);
        return request;
    }
}
