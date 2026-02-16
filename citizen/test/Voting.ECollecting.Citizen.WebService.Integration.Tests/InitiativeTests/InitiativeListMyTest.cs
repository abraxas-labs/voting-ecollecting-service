// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeListMyTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeListMyTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives);
    }

    [Fact]
    public async Task ListMyAsCreator()
    {
        var response = await AuthenticatedClient.ListMyAsync(new ListMyInitiativesRequest());
        await Verify(response);
    }

    [Fact]
    public async Task ListMyAsDeputy()
    {
        var response = await DeputyClient.ListMyAsync(new ListMyInitiativesRequest());
        await Verify(response);
    }

    [Fact]
    public async Task ListMyAsDeputyNotAcceptedShouldReturnEmpty()
    {
        var response = await DeputyNotAcceptedClient.ListMyAsync(new ListMyInitiativesRequest());
        response.Initiatives.Should().BeEmpty();
    }

    [Fact]
    public async Task ListMyAsReader()
    {
        var response = await ReaderClient.ListMyAsync(new ListMyInitiativesRequest());
        await Verify(response);
    }

    [Fact]
    public async Task ListMyAsReaderNotAcceptedShouldReturnEmpty()
    {
        var response = await ReaderNotAcceptedClient.ListMyAsync(new ListMyInitiativesRequest());
        response.Initiatives.Should().BeEmpty();
    }

    [Fact]
    public async Task UnauthenticatedShouldReturnEmpty()
    {
        await AssertStatus(
            async () => await Client.ListMyAsync(new ListMyInitiativesRequest()),
            StatusCode.Unauthenticated);
    }

    [Fact]
    public async Task NoPermissionsShouldReturnEmpty()
    {
        var response = await AuthenticatedNoPermissionClient.ListMyAsync(new ListMyInitiativesRequest());
        response.Initiatives.Should().BeEmpty();
    }

    [Fact]
    public async Task TestOnlyCtEnabled()
    {
        await WithOnlyCtDomainOfInfluenceTypeEnabled(async () =>
        {
            var response = await AuthenticatedClient.ListMyAsync(new ListMyInitiativesRequest());
            await Verify(response);
        });
    }
}
