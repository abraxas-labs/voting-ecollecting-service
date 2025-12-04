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

public class CollectionUpdateInformalReviewRequestedTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionUpdateInformalReviewRequestedTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation));
    }

    [Fact]
    public async Task ShouldUpdateInformalReviewRequested()
    {
        await AuthenticatedClient.UpdateRequestInformalReviewAsync(new UpdateRequestInformalReviewRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeInPreparation,
            RequestInformalReview = true,
        });

        var initiative = await RunOnDb(db => db.Initiatives
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.InformalReviewRequested.Should().BeTrue();

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
            await AuthenticatedClient.UpdateRequestInformalReviewAsync(new UpdateRequestInformalReviewRequest
            {
                Id = InitiativesCtStGallen.IdLegislativeInPreparation,
                RequestInformalReview = true,
            });
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldUpdateInformalReviewRequestedWithdrawn()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.InformalReviewRequested, true)));

        await AuthenticatedClient.UpdateRequestInformalReviewAsync(new UpdateRequestInformalReviewRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeInPreparation,
            RequestInformalReview = false,
        });

        var initiative = await RunOnDb(db => db.Initiatives
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.InformalReviewRequested.Should().BeFalse();

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesCtStGallen.GuidLegislativeInPreparation));
        await Verify(new { userNotifications, collectionMessage });
    }

    [Fact]
    public async Task ShouldUpdateInformalReviewRequestedAsDeputy()
    {
        await DeputyClient.UpdateRequestInformalReviewAsync(new UpdateRequestInformalReviewRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeInPreparation,
            RequestInformalReview = true,
        });

        var initiative = await RunOnDb(db => db.Initiatives
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.InformalReviewRequested.Should().BeTrue();

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesCtStGallen.GuidLegislativeInPreparation));
        await Verify(new { userNotifications, collectionMessage });
    }

    [Fact]
    public async Task AlreadyRequestedShouldFail()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.InformalReviewRequested, true)));

        await AssertStatus(
            async () => await AuthenticatedClient.UpdateRequestInformalReviewAsync(
                new UpdateRequestInformalReviewRequest
                {
                    Id = InitiativesCtStGallen.IdLegislativeInPreparation,
                    RequestInformalReview = true,
                }),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task AlreadyWithdrawnShouldFail()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.UpdateRequestInformalReviewAsync(
                new UpdateRequestInformalReviewRequest
                {
                    Id = InitiativesCtStGallen.IdLegislativeInPreparation,
                    RequestInformalReview = false,
                }),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.UpdateRequestInformalReviewAsync(new UpdateRequestInformalReviewRequest { Id = "a9761677-5ef6-46b8-bb12-7419b0be6ce9" }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsReaderShouldFail()
    {
        await AssertStatus(
            async () => await ReaderClient.UpdateRequestInformalReviewAsync(new UpdateRequestInformalReviewRequest { Id = InitiativesCtStGallen.IdLegislativeInPreparation }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsDeputyNotAcceptedShouldFail()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.UpdateRequestInformalReviewAsync(new UpdateRequestInformalReviewRequest { Id = InitiativesCtStGallen.IdLegislativeInPreparation }),
            StatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(
            async () => await Client.UpdateRequestInformalReviewAsync(new UpdateRequestInformalReviewRequest { Id = InitiativesCtStGallen.IdLegislativeInPreparation }),
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
            await AuthenticatedClient.UpdateRequestInformalReviewAsync(new UpdateRequestInformalReviewRequest { Id = InitiativesCtStGallen.IdLegislativeInPreparation, RequestInformalReview = true });
        }
        else
        {
            await AssertStatus(
                async () => await AuthenticatedClient.UpdateRequestInformalReviewAsync(new UpdateRequestInformalReviewRequest { Id = InitiativesCtStGallen.IdLegislativeInPreparation, RequestInformalReview = true }),
                StatusCode.NotFound);
        }
    }
}
