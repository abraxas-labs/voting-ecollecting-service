// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeListSubTypesTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeListSubTypesTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Test()
    {
        var response = await AuthenticatedClient.ListSubTypesAsync(new ListInitiativeSubTypesRequest());
        await Verify(response);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(async () => await Client.ListSubTypesAsync(new ListInitiativeSubTypesRequest()), StatusCode.Unauthenticated);
    }
}
