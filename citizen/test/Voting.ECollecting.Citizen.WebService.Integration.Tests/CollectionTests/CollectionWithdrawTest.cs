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

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.CollectionTests;

public class CollectionWithdrawTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionWithdrawTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation));
    }

    [Fact]
    public async Task ShouldWithdrawAsCreator()
    {
        await AuthenticatedClient.WithdrawAsync(NewValidRequest());

        var initiative = await RunOnDb(db => db.Initiatives
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.State.Should().Be(CollectionState.Withdrawn);

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesCtStGallen.GuidLegislativeInPreparation));
        await Verify(new { userNotifications, collectionMessage });
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await AuthenticatedClient.WithdrawAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldWithdrawAsDeputy()
    {
        await DeputyClient.WithdrawAsync(NewValidRequest());

        var initiative = await RunOnDb(db => db.Initiatives
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.State.Should().Be(CollectionState.Withdrawn);

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesCtStGallen.GuidLegislativeInPreparation));
        await Verify(new { userNotifications, collectionMessage });
    }

    [Fact]
    public async Task InPaperSubmissionShouldFail()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x
                .SetProperty(y => y.IsElectronicSubmission, false)
                .SetProperty(y => y.SecureIdNumber, "XXXXXXXXXXXX")));
        await AssertStatus(
            async () => await AuthenticatedClient.WithdrawAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.WithdrawAsync(new WithdrawCollectionRequest { Id = "729d7e2e-407c-409d-b46a-88455424111d" }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsReaderShouldFail()
    {
        await AssertStatus(
            async () => await ReaderClient.WithdrawAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsDeputyNotAcceptedShouldFail()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.WithdrawAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(
            async () => await Client.WithdrawAsync(NewValidRequest()),
            StatusCode.Unauthenticated);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        if (state == CollectionState.InPreparation)
        {
            await AuthenticatedClient.WithdrawAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await AuthenticatedClient.WithdrawAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    private WithdrawCollectionRequest NewValidRequest(Action<WithdrawCollectionRequest>? customizer = null)
    {
        var request = new WithdrawCollectionRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeInPreparation,
        };
        customizer?.Invoke(request);
        return request;
    }
}
