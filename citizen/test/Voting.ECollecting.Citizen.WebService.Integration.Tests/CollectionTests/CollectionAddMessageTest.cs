// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.CollectionTests;

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
    public async Task ShouldWorkAsCreator()
    {
        var id = await AuthenticatedClient.AddMessageAsync(NewValidRequest());
        id.Id.Should().NotBeEmpty();

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCh.GuidInPreparation)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.Id == Guid.Parse(id.Id)));
        await Verify(new { userNotifications, collectionMessage });
    }

    [Fact]
    public async Task ShouldWorkAsDeputy()
    {
        var id = await DeputyClient.AddMessageAsync(NewValidRequest());
        id.Id.Should().NotBeEmpty();

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCh.GuidInPreparation)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.Id == Guid.Parse(id.Id)));
        await Verify(new { userNotifications, collectionMessage });
    }

    [Fact]
    public async Task AsReaderShouldFail()
    {
        await AssertStatus(
            async () => await ReaderClient.AddMessageAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldReturnNotFoundWithoutPermissions()
    {
        await AssertStatus(
            async () => await AuthenticatedNoPermissionClient.AddMessageAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowNotFoundForUnknownCollection()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.AddMessageAsync(new AddCollectionMessageRequest
            {
                CollectionId = "e239e756-e823-4193-b04c-1cf371ff9d2e",
                Content = "foo bar",
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(async () => await Client.AddMessageAsync(NewValidRequest()), StatusCode.Unauthenticated);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInState(CollectionState state)
    {
        await ModifyDbEntities<InitiativeEntity>(
            e => e.Id == InitiativesCh.GuidInPreparation,
            e => e.State = state);

        if (state.IsEndedOrAborted())
        {
            await AssertStatus(
                async () => await AuthenticatedClient.AddMessageAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
        else
        {
            await AuthenticatedClient.AddMessageAsync(NewValidRequest());
        }
    }

    private AddCollectionMessageRequest NewValidRequest()
    {
        return new AddCollectionMessageRequest
        {
            CollectionId = InitiativesCh.IdInPreparation,
            Content = "Hey there, nice to meet you!",
        };
    }
}
