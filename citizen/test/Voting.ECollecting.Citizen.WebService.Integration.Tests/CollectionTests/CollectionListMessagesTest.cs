// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.CollectionTests;

public class CollectionListMessagesTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionListMessagesTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCh.GuidInPreparation) with { SeedInitiativeMessages = true });
    }

    [Fact]
    public async Task ShouldWorkAsOwner()
    {
        var messages = await AuthenticatedClient.ListMessagesAsync(NewValidRequest());
        await Verify(messages);
    }

    [Fact]
    public async Task ShouldWorkWithDeputyPermission()
    {
        var messages = await DeputyClient.ListMessagesAsync(NewValidRequest());
        await Verify(messages);
    }

    [Fact]
    public async Task ShouldWorkWithReaderPermission()
    {
        var messages = await ReaderClient.ListMessagesAsync(NewValidRequest());
        await Verify(messages);
    }

    [Fact]
    public async Task AsDeputyNotAcceptedShouldFail()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.ListMessagesAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowWithoutPermissions()
    {
        await AssertStatus(
            async () => await AuthenticatedNoPermissionClient.ListMessagesAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowForUnknownCollection()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.ListMessagesAsync(new ListCollectionMessagesRequest
            {
                CollectionId = "e239e756-e823-4193-b04c-1cf371ff9d2e",
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(async () => await Client.ListMessagesAsync(NewValidRequest()), StatusCode.Unauthenticated);
    }

    private ListCollectionMessagesRequest NewValidRequest()
    {
        return new ListCollectionMessagesRequest
        {
            CollectionId = InitiativesCh.IdInPreparation,
        };
    }
}
