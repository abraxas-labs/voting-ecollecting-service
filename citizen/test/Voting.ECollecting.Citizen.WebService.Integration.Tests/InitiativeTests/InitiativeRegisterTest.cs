// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeRegisterTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeRegisterTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeReadyForRegistration));
    }

    [Fact]
    public async Task ShouldRegisterAsCreator()
    {
        await AuthenticatedClient.RegisterAsync(NewValidRequest());

        var initiative = await RunOnDb(db => db.Initiatives
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeReadyForRegistration));
        initiative.State.Should().Be(CollectionState.Registered);

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCtStGallen.GuidLegislativeReadyForRegistration)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesCtStGallen.GuidLegislativeReadyForRegistration));
        await Verify(new { userNotifications, collectionMessage });
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await AuthenticatedClient.RegisterAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldWithdrawAsDeputy()
    {
        await DeputyClient.RegisterAsync(NewValidRequest());

        var initiative = await RunOnDb(db => db.Initiatives
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeReadyForRegistration));
        initiative.State.Should().Be(CollectionState.Registered);

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCtStGallen.GuidLegislativeReadyForRegistration)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesCtStGallen.GuidLegislativeReadyForRegistration));
        await Verify(new { userNotifications, collectionMessage });
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.RegisterAsync(new RegisterInitiativeRequest { Id = "df431d05-b357-439f-bcfc-d5de42dde6fa" }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsReaderShouldFail()
    {
        await AssertStatus(
            async () => await ReaderClient.RegisterAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsDeputyNotAcceptedShouldFail()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.RegisterAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(
            async () => await Client.RegisterAsync(NewValidRequest()),
            StatusCode.Unauthenticated);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeReadyForRegistration)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        if (state == CollectionState.ReadyForRegistration)
        {
            await AuthenticatedClient.RegisterAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await AuthenticatedClient.RegisterAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    private RegisterInitiativeRequest NewValidRequest(Action<RegisterInitiativeRequest>? customizer = null)
    {
        var request = new RegisterInitiativeRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeReadyForRegistration,
        };
        customizer?.Invoke(request);
        return request;
    }
}
