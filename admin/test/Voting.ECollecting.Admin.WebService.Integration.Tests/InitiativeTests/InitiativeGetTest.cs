// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Grpc.Net.Client;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class InitiativeGetTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeGetTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation, InitiativesMuStGallen.GuidInPreparation));
    }

    [Fact]
    public async Task ShouldGetInitiative()
    {
        var initiative = await CtSgStammdatenverwalterClient.GetAsync(NewValidRequest());
        await Verify(initiative);
    }

    [Fact]
    public async Task AsMuOnOwnCollectionShouldWork()
    {
        var initiative = await MuSgStammdatenverwalterClient.GetAsync(NewValidRequest(x => x.Id = InitiativesMuStGallen.IdInPreparation));
        await Verify(initiative);
    }

    [Fact]
    public async Task AsMuOnCtCollectionShouldWork()
    {
        var initiative = await MuSgStammdatenverwalterClient.GetAsync(NewValidRequest());
        await Verify(initiative);
    }

    [Fact]
    public async Task AsCtOnMuCollectionShouldWork()
    {
        var initiative = await CtSgStammdatenverwalterClient.GetAsync(NewValidRequest(x => x.Id = InitiativesMuStGallen.IdInPreparation));
        await Verify(initiative);
    }

    [Fact]
    public async Task AsMuOnOtherMuCollectionShouldWork()
    {
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.GetAsync(NewValidRequest(x => x.Id = InitiativesMuStGallen.IdInPreparation)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.GetAsync(NewValidRequest(x => x.Id = "1c42e139-1f9b-427c-a236-8a1c6553ddb9")),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new InitiativeService.InitiativeServiceClient(channel).GetAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        return Roles.AllHumanUserRoles();
    }

    private GetInitiativeRequest NewValidRequest(Action<GetInitiativeRequest>? customizer = null)
    {
        var request = new GetInitiativeRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeInPreparation,
        };

        customizer?.Invoke(request);
        return request;
    }
}
