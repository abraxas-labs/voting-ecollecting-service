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
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;
using CollectionState = Voting.ECollecting.Shared.Domain.Enums.CollectionState;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class InitiativeFinishCorrectionTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeFinishCorrectionTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeUnderReview, InitiativesMuStGallen.GuidUnderReview));

        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesCtStGallen.GuidLegislativeUnderReview || x.Id == InitiativesMuStGallen.GuidUnderReview,
            x =>
            {
                x.AdmissibilityDecisionState = AdmissibilityDecisionState.Valid;
                x.CollectionStartDate = GetService<TimeProvider>().GetUtcNowDateTime().AddDays(4);
            });
    }

    [Fact]
    public async Task ShouldFinishCorrectionInitiative()
    {
        await CtSgStammdatenverwalterClient.FinishCorrectionAsync(NewValidRequest());

        var initiative = await RunOnDb(db => db.Initiatives
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeUnderReview));
        initiative.State.Should().Be(CollectionState.ReadyForRegistration);

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCtStGallen.GuidLegislativeUnderReview)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesCtStGallen.GuidLegislativeUnderReview));
        await Verify(new { userNotifications, collectionMessage });
    }

    [Fact]
    public async Task AsMuOnOwnCollectionShouldWork()
    {
        await MuSgStammdatenverwalterClient.FinishCorrectionAsync(NewValidRequest(x => x.Id = InitiativesMuStGallen.IdUnderReview));

        var initiative = await RunOnDb(db => db.Initiatives
            .FirstAsync(x => x.Id == InitiativesMuStGallen.GuidUnderReview));
        initiative.State.Should().Be(CollectionState.ReadyForRegistration);

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesMuStGallen.GuidUnderReview)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesMuStGallen.GuidUnderReview));
        await Verify(new { userNotifications, collectionMessage });
    }

    [Fact]
    public async Task AsMuOnCtCollectionShouldFail()
    {
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.FinishCorrectionAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsMuOnOtherMuCollectionShouldFail()
    {
        var req = NewValidRequest(x => x.Id = InitiativesMuStGallen.IdUnderReview);
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.FinishCorrectionAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsCtOnMuCollectionShouldFail()
    {
        var req = NewValidRequest(x => x.Id = InitiativesMuStGallen.IdUnderReview);
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.FinishCorrectionAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.FinishCorrectionAsync(NewValidRequest(x => x.Id = "dbcef2db-e04f-493d-a226-c6dc33f7e5d2")),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AdmissibilityDecisionStateNotValidShouldFail()
    {
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesCtStGallen.GuidLegislativeUnderReview,
            x => x.AdmissibilityDecisionState = AdmissibilityDecisionState.ValidButSubjectToConditions);

        var req = NewValidRequest();
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.FinishCorrectionAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task CollectionStartDateInPastShouldFail()
    {
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesCtStGallen.GuidLegislativeUnderReview,
            x => x.CollectionStartDate = GetService<TimeProvider>().GetUtcNowDateTime().AddDays(-1));

        var req = NewValidRequest();
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.FinishCorrectionAsync(req),
            StatusCode.NotFound);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeUnderReview)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        if (state is CollectionState.UnderReview)
        {
            await CtSgStammdatenverwalterClient.FinishCorrectionAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await CtSgStammdatenverwalterClient.FinishCorrectionAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStammdatenverwalterClient.FinishCorrectionAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new InitiativeService.InitiativeServiceClient(channel)
            .FinishCorrectionAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private FinishCorrectionInitiativeRequest NewValidRequest(Action<FinishCorrectionInitiativeRequest>? customizer = null)
    {
        var request = new FinishCorrectionInitiativeRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeUnderReview,
        };
        customizer?.Invoke(request);
        return request;
    }
}
