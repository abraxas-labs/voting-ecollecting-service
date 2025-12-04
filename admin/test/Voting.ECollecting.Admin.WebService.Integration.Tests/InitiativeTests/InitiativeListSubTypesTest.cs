// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Net.Client;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class InitiativeListSubTypesTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeListSubTypesTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Test()
    {
        var response = await CtSgStammdatenverwalterClient.ListSubTypesAsync(new ListInitiativeSubTypesRequest());
        await Verify(response);
    }

    [Fact]
    public async Task TestAsMu()
    {
        var response = await MuSgStammdatenverwalterClient.ListSubTypesAsync(new ListInitiativeSubTypesRequest());
        await Verify(response);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new InitiativeService.InitiativeServiceClient(channel)
            .ListSubTypesAsync(new ListInitiativeSubTypesRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }
}
