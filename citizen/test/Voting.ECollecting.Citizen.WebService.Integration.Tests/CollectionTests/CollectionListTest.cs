// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Adapter.VotingStimmregister;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.CollectionTests;

public class CollectionListTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionListTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Default);
    }

    [Fact]
    public async Task Test()
    {
        var referendumCitizenClient = new ReferendumService.ReferendumServiceClient(Factory.CreateGrpcChannel(
            true,
            null,
            CitizenAuthMockDefaults.CitizenUserId,
            [],
            [
                (CitizenAuthMockDefaults.UserAcrHeaderName, CitizenAuthMockDefaults.AcrValue400),
                (CitizenAuthMockDefaults.UserEMailHeaderName, CitizenAuthMockDefaults.UserCitizenTestEMail),
                (CitizenAuthMockDefaults.UserSocialSecurityNumberHeaderName, VotingStimmregisterAdapterMock.VotingRightPerson12Ssn)
            ]));

        await referendumCitizenClient.SignAsync(new SignReferendumRequest { Id = ReferendumsCtStGallen.IdInCollectionEnabledForCollection });

        var initiativeCitizenClient = new InitiativeService.InitiativeServiceClient(Factory.CreateGrpcChannel(
            true,
            null,
            CitizenAuthMockDefaults.CitizenUserId,
            [],
            [
                (CitizenAuthMockDefaults.UserAcrHeaderName, CitizenAuthMockDefaults.AcrValue400),
                (CitizenAuthMockDefaults.UserEMailHeaderName, CitizenAuthMockDefaults.UserCitizenTestEMail),
                (CitizenAuthMockDefaults.UserSocialSecurityNumberHeaderName, VotingStimmregisterAdapterMock.VotingRightPerson12Ssn)
            ]));

        await initiativeCitizenClient.SignAsync(new SignInitiativeRequest() { Id = InitiativesCtStGallen.IdUnityEnabledForCollectionCollecting });

        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            ssn: VotingStimmregisterAdapterMock.VotingRightPerson12Ssn);
        var response = await client.ListAsync(NewValidRequest());
        await Verify(response);
    }

    [Fact]
    public async Task TestOnlyCtEnabled()
    {
        await WithOnlyCtDomainOfInfluenceTypeEnabled(async () =>
        {
            var response = await Client.ListAsync(NewValidRequest());
            await Verify(response);
        });
    }

    [Fact]
    public async Task TestOnlyCtFiltered()
    {
        var response = await Client.ListAsync(NewValidRequest(x => x.Types_.Add(DomainOfInfluenceType.Ct)));
        await Verify(response);
    }

    [Fact]
    public async Task TestOnlyBfsFiltered()
    {
        var response = await Client.ListAsync(NewValidRequest(x => x.Bfs = Bfs.MunicipalityStGallen));
        await Verify(response);
    }

    [Fact]
    public async Task TestAsCreator()
    {
        var response = await AuthenticatedClient.ListAsync(NewValidRequest());
        await Verify(response);
    }

    [Fact]
    public async Task TestEndedCollections()
    {
        var response = await Client.ListAsync(NewValidRequest(x => x.PeriodState = CollectionPeriodState.Unspecified));
        await Verify(response);
    }

    private ListCollectionsRequest NewValidRequest(Action<ListCollectionsRequest>? customizer = null)
    {
        var request = new ListCollectionsRequest
        {
            PeriodState = CollectionPeriodState.InCollection,
        };
        customizer?.Invoke(request);
        return request;
    }
}
