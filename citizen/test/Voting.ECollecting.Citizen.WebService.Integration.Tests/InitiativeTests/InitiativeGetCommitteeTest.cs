// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

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

public class InitiativeGetCommitteeTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeGetCommitteeTest(TestApplicationFactory factory)
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
        var committee = await AuthenticatedClient.GetCommitteeAsync(NewValidRequest());
        await Verify(committee);
    }

    [Fact]
    public async Task ShouldWorkAsDeputy()
    {
        var committee = await DeputyClient.GetCommitteeAsync(NewValidRequest());
        await Verify(committee);
    }

    [Fact]
    public async Task ShouldWorkAsReader()
    {
        var committee = await ReaderClient.GetCommitteeAsync(NewValidRequest());
        await Verify(committee);
    }

    [Fact]
    public async Task AsDeputyNotAcceptedShouldFail()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.GetCommitteeAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.GetCommitteeAsync(new GetInitiativeCommitteeRequest { Id = "f98b187c-6ad4-48ed-a529-d458f6e2666d" }),
            StatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(async () => await Client.GetCommitteeAsync(NewValidRequest()), StatusCode.Unauthenticated);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        await AuthenticatedClient.GetCommitteeAsync(NewValidRequest());
    }

    private GetInitiativeCommitteeRequest NewValidRequest()
    {
        return new GetInitiativeCommitteeRequest { Id = InitiativesCtStGallen.IdLegislativeInPreparation };
    }
}
