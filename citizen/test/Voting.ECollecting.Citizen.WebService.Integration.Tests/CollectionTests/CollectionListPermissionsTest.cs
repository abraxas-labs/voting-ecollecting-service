// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.CollectionTests;

public class CollectionListPermissionsTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionListPermissionsTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation));
    }

    [Fact]
    public async Task ShouldWorkAsCreator()
    {
        var response = await AuthenticatedClient.ListPermissionsAsync(NewValidRequest());
        await Verify(response);
    }

    [Fact]
    public async Task ShouldWorkAsDeputy()
    {
        var response = await DeputyClient.ListPermissionsAsync(NewValidRequest());
        await Verify(response);
    }

    [Fact]
    public async Task ShouldReturnEmptyAsDeputyNotAccepted()
    {
        var response = await DeputyNotAcceptedClient.ListPermissionsAsync(NewValidRequest());
        response.Permissions.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldWorkAsReader()
    {
        var response = await ReaderClient.ListPermissionsAsync(NewValidRequest());
        await Verify(response);
    }

    [Fact]
    public async Task ShouldReturnEmptyAsReaderNotAccepted()
    {
        var response = await ReaderNotAcceptedClient.ListPermissionsAsync(NewValidRequest());
        response.Permissions.Should().BeEmpty();
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(async () => await Client.ListPermissionsAsync(new ListCollectionPermissionsRequest()), StatusCode.Unauthenticated);
    }

    private ListCollectionPermissionsRequest NewValidRequest(Action<ListCollectionPermissionsRequest>? customizer = null)
    {
        var request = new ListCollectionPermissionsRequest
        {
            CollectionId = InitiativesCtStGallen.IdLegislativeInPreparation,
        };
        customizer?.Invoke(request);
        return request;
    }
}
