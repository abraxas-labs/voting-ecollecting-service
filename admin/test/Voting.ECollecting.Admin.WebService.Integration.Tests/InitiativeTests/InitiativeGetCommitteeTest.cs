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

public class InitiativeGetCommitteeTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeGetCommitteeTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation, InitiativesCtStGallen.GuidUnityEnabledForCollectionCollecting, InitiativesMuStGallen.GuidInPreparation));
    }

    [Fact]
    public async Task ShouldGetInitiativeCommittee()
    {
        var committee = await CtSgStammdatenverwalterClient.GetCommitteeAsync(NewValidRequest());
        await Verify(committee);
    }

    [Fact]
    public async Task AsMuOnOwnCollectionShouldWork()
    {
        var committee = await MuSgStammdatenverwalterClient.GetCommitteeAsync(NewValidRequest(x => x.Id = InitiativesMuStGallen.IdInPreparation));
        await Verify(committee);
    }

    [Fact]
    public async Task AsMuOnCtCollectionShouldWork()
    {
        var committee = await MuSgStammdatenverwalterClient.GetCommitteeAsync(NewValidRequest(x => x.Id = InitiativesCtStGallen.IdUnityEnabledForCollectionCollecting));
        await Verify(committee);
    }

    [Fact]
    public async Task AsMuOnOtherMuCollectionShouldWork()
    {
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.GetCommitteeAsync(NewValidRequest(x => x.Id = InitiativesMuStGallen.IdInPreparation)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsCtOnMuCollectionShouldWork()
    {
        var committee = await CtSgStammdatenverwalterClient.GetCommitteeAsync(NewValidRequest(x => x.Id = InitiativesMuStGallen.IdInPreparation));
        await Verify(committee);
    }

    [Fact]
    public async Task AsMuOnOnCtCollectionInPreparationShouldThrow()
    {
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.GetCommitteeAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.GetCommitteeAsync(NewValidRequest(x => x.Id = "b09acc7b-65f7-4a02-9d8b-09f6a3f9a546")),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new InitiativeService.InitiativeServiceClient(channel).GetCommitteeAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private GetInitiativeCommitteeRequest NewValidRequest(Action<GetInitiativeCommitteeRequest>? customizer = null)
    {
        var request = new GetInitiativeCommitteeRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeInPreparation,
        };

        customizer?.Invoke(request);
        return request;
    }
}
