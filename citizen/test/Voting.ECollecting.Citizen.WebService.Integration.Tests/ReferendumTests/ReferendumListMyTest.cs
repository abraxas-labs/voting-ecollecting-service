// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.ReferendumTests;

public class ReferendumListMyTest : BaseGrpcTest<ReferendumService.ReferendumServiceClient>
{
    public ReferendumListMyTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Referendums);
    }

    [Fact]
    public async Task ListMyAsCreator()
    {
        var response = await AuthenticatedClient.ListMyAsync(new ListMyReferendumsRequest());
        await Verify(response);
    }

    [Fact]
    public async Task ListMyAsDeputy()
    {
        var response = await DeputyClient.ListMyAsync(new ListMyReferendumsRequest());
        await Verify(response);
    }

    [Fact]
    public async Task ListMyAsDeputyNotAcceptedShouldReturnEmpty()
    {
        var response = await DeputyNotAcceptedClient.ListMyAsync(new ListMyReferendumsRequest());
        response.Decrees.Should().BeEmpty();
        response.WithoutDecreeReferendums.Should().BeEmpty();
    }

    [Fact]
    public async Task ListMyAsReader()
    {
        var response = await ReaderClient.ListMyAsync(new ListMyReferendumsRequest());
        await Verify(response);
    }

    [Fact]
    public async Task ListMyAsReaderNotAcceptedShouldReturnEmpty()
    {
        var response = await ReaderNotAcceptedClient.ListMyAsync(new ListMyReferendumsRequest());
        response.Decrees.Should().BeEmpty();
        response.WithoutDecreeReferendums.Should().BeEmpty();
    }

    [Fact]
    public async Task UnauthenticatedShouldReturnEmpty()
    {
        await AssertStatus(
            async () => await Client.ListMyAsync(new ListMyReferendumsRequest()),
            StatusCode.Unauthenticated);
    }

    [Fact]
    public async Task NoPermissionsShouldReturnEmpty()
    {
        var response = await AuthenticatedNoPermissionClient.ListMyAsync(new ListMyReferendumsRequest());
        response.Decrees.Should().BeEmpty();
        response.WithoutDecreeReferendums.Should().BeEmpty();
    }

    [Fact]
    public async Task TestOnlyCtEnabled()
    {
        await WithOnlyCtDomainOfInfluenceTypeEnabled(async () =>
        {
            var response = await AuthenticatedClient.ListMyAsync(new ListMyReferendumsRequest());
            await Verify(response);
        });
    }
}
