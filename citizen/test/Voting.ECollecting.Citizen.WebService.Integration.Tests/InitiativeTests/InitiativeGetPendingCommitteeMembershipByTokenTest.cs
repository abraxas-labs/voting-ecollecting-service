// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.Lib.Common;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeGetPendingCommitteeMembershipByTokenTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    private static readonly UrlToken _token =
        InitiativeCommitteeMembers.BuildToken(
            InitiativesCtStGallen.GuidLegislativeInPreparation,
            "margarita@example.com");

    public InitiativeGetPendingCommitteeMembershipByTokenTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation));
    }

    [Fact]
    public async Task ShouldWork()
    {
        var committee = await Client.GetPendingCommitteeMembershipByTokenAsync(NewValidRequest());
        await Verify(committee);
    }

    [Fact]
    public async Task ExpiredShouldThrow()
    {
        await ModifyDbEntities(
            (InitiativeCommitteeMemberEntity e) => e.Token == _token,
            e => e.TokenExpiry = MockedClock.GetDate(-4));

        await AssertStatus(
            async () => await Client.GetPendingCommitteeMembershipByTokenAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await Client.GetPendingCommitteeMembershipByTokenAsync(new GetPendingCommitteeMembershipByTokenRequest
            {
                Token = UrlToken.New(),
            }),
            StatusCode.NotFound);
    }

    private GetPendingCommitteeMembershipByTokenRequest NewValidRequest()
    {
        return new GetPendingCommitteeMembershipByTokenRequest { Token = _token.ToString() };
    }
}
