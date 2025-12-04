// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Net.Client;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class InitiativeListTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeListTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives);
    }

    [Fact]
    public async Task TestAsCtTenantShouldWork()
    {
        var response = await CtSgStammdatenverwalterClient.ListAsync(new ListInitiativesRequest());
        await Verify(response);
    }

    [Fact]
    public async Task TestAsMuTenantShouldWork()
    {
        var response = await MuSgStammdatenverwalterClient.ListAsync(new ListInitiativesRequest());
        await Verify(response);
    }

    [Fact]
    public async Task TestFilterCtShouldWork()
    {
        var response = await CtSgStammdatenverwalterClient.ListAsync(new ListInitiativesRequest
        {
            Types_ = { DomainOfInfluenceType.Ct },
        });
        await Verify(response);
    }

    [Fact]
    public async Task TestFilterBfsShouldWork()
    {
        var response = await CtSgStammdatenverwalterClient.ListAsync(new ListInitiativesRequest
        {
            Bfs = Bfs.MunicipalityStGallen,
        });
        await Verify(response);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
        => await new InitiativeService.InitiativeServiceClient(channel)
            .ListAsync(new ListInitiativesRequest());

    protected override IEnumerable<string> AuthorizedRoles()
    {
        return Roles.AllHumanUserRoles();
    }
}
