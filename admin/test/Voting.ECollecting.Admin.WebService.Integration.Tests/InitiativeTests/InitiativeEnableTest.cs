// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
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
using Voting.Lib.Testing.Mocks;
using CollectionState = Voting.ECollecting.Shared.Domain.Enums.CollectionState;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class InitiativeEnableTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeEnableTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeRegistered, InitiativesMuStGallen.GuidRegistered, InitiativesCtStGallen.GuidLegislativeUnderReview));
    }

    [Fact]
    public async Task ShouldEnableInitiative()
    {
        await CtSgStammdatenverwalterClient.EnableAsync(NewValidRequest());

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.Municipalities!.OrderBy(y => y.Bfs))
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeRegistered));
        initiative.State.Should().Be(CollectionState.EnabledForCollection);
        initiative.SetPeriodState(GetService<TimeProvider>().GetUtcNowDateTime());

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCtStGallen.GuidLegislativeRegistered)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesCtStGallen.GuidLegislativeRegistered));
        await Verify(new { userNotifications, collectionMessage, initiative });
    }

    [Fact]
    public async Task AsMuOnOwnCollectionShouldWork()
    {
        await MuSgStammdatenverwalterClient.EnableAsync(NewValidRequest(x => x.Id = InitiativesMuStGallen.IdRegistered));

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.Municipalities!.OrderBy(y => y.Bfs))
            .FirstAsync(x => x.Id == InitiativesMuStGallen.GuidRegistered));
        initiative.State.Should().Be(CollectionState.EnabledForCollection);
        initiative.SetPeriodState(GetService<TimeProvider>().GetUtcNowDateTime());

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesMuStGallen.GuidRegistered)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesMuStGallen.GuidRegistered));
        await Verify(new { userNotifications, collectionMessage, initiative });
    }

    [Fact]
    public async Task ShouldEnableInitiativeForUnderReview()
    {
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesCtStGallen.GuidLegislativeUnderReview,
            x =>
            {
                x.AdmissibilityDecisionState = AdmissibilityDecisionState.Valid;
                x.CollectionStartDate = GetService<TimeProvider>().GetUtcNowDateTime().AddDays(-1);
                x.CollectionEndDate = GetService<TimeProvider>().GetUtcNowDateTime().AddDays(30);
            });

        await CtSgStammdatenverwalterClient.EnableAsync(NewValidRequest(x =>
        {
            x.Id = InitiativesCtStGallen.IdLegislativeUnderReview;
            x.CollectionStartDate = null;
            x.CollectionEndDate = null;
        }));

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.Municipalities!.OrderBy(y => y.Bfs))
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeUnderReview));
        initiative.State.Should().Be(CollectionState.EnabledForCollection);
        initiative.SetPeriodState(GetService<TimeProvider>().GetUtcNowDateTime());

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCtStGallen.GuidLegislativeUnderReview)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesCtStGallen.GuidLegislativeUnderReview));
        await Verify(new { userNotifications, collectionMessage, initiative });
    }

    [Fact]
    public async Task UnderReviewButNotInCollectionShouldFail()
    {
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesCtStGallen.GuidLegislativeUnderReview,
            x =>
            {
                x.AdmissibilityDecisionState = AdmissibilityDecisionState.Valid;
                x.CollectionStartDate = GetService<TimeProvider>().GetUtcNowDateTime().AddDays(5);
                x.CollectionEndDate = GetService<TimeProvider>().GetUtcNowDateTime().AddDays(30);
            });

        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.EnableAsync(NewValidRequest(x =>
            {
                x.Id = InitiativesCtStGallen.IdLegislativeUnderReview;
                x.CollectionStartDate = null;
                x.CollectionEndDate = null;
            })),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AdmissibilityDecisionStateNotValidShouldFail()
    {
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesCtStGallen.GuidLegislativeUnderReview,
            x =>
            {
                x.AdmissibilityDecisionState = AdmissibilityDecisionState.ValidButSubjectToConditions;
                x.CollectionStartDate = GetService<TimeProvider>().GetUtcNowDateTime().AddDays(-1);
                x.CollectionEndDate = GetService<TimeProvider>().GetUtcNowDateTime().AddDays(30);
            });

        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.EnableAsync(NewValidRequest(x =>
            {
                x.Id = InitiativesCtStGallen.IdLegislativeUnderReview;
                x.CollectionStartDate = null;
                x.CollectionEndDate = null;
            })),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldFailWithoutCollectionStartDate()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.EnableAsync(NewValidRequest(x => x.CollectionStartDate = null)),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task ShouldFailWithoutCollectionEndDate()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.EnableAsync(NewValidRequest(x => x.CollectionEndDate = null)),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task ShouldFailForCollectionStartDateInPast()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.EnableAsync(NewValidRequest(x => x.CollectionStartDate = Timestamp.FromDateTime(GetService<TimeProvider>().GetUtcNowDateTime().AddDays(-1)))),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task ShouldFailForCollectionEndDateBeforeStartDate()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.EnableAsync(NewValidRequest(x =>
            {
                x.CollectionStartDate = MockedClock.GetTimestampDate();
                x.CollectionEndDate = MockedClock.GetTimestampDate(-1);
            })),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task AsMuOnCtCollectionShouldFail()
    {
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.EnableAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsMuOnOtherMuCollectionShouldFail()
    {
        var req = NewValidRequest(x => x.Id = InitiativesMuStGallen.IdRegistered);
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.EnableAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsCtOnMuCollectionShouldFail()
    {
        var req = NewValidRequest(x => x.Id = InitiativesMuStGallen.IdRegistered);
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.EnableAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.EnableAsync(NewValidRequest(x => x.Id = "c73c6291-483f-4435-bff9-00764accf016")),
            StatusCode.NotFound);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeRegistered)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        if (state is CollectionState.Registered)
        {
            await CtSgStammdatenverwalterClient.EnableAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await CtSgStammdatenverwalterClient.EnableAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStammdatenverwalterClient.EnableAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new InitiativeService.InitiativeServiceClient(channel)
            .EnableAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private EnableInitiativeRequest NewValidRequest(Action<EnableInitiativeRequest>? customizer = null)
    {
        var request = new EnableInitiativeRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeRegistered,
            CollectionStartDate = MockedClock.GetTimestampDate(10),
            CollectionEndDate = MockedClock.GetTimestampDate(50),
        };
        customizer?.Invoke(request);
        return request;
    }
}
