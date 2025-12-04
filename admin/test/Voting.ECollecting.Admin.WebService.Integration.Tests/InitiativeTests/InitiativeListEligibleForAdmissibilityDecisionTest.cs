// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Net.Client;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class InitiativeListEligibleForAdmissibilityDecisionTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeListEligibleForAdmissibilityDecisionTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Initiatives);
    }

    [Fact]
    public async Task ShouldWorkCt()
    {
        var resp = await CtSgStammdatenverwalterClient.ListEligibleForAdmissibilityDecisionAsync(
            new ListEligibleForAdmissibilityDecisionRequest());
        await Verify(resp);
    }

    [Fact]
    public async Task ShouldWorkMu()
    {
        var resp = await MuSgStammdatenverwalterClient.ListEligibleForAdmissibilityDecisionAsync(
            new ListEligibleForAdmissibilityDecisionRequest());
        await Verify(resp);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new InitiativeService.InitiativeServiceClient(channel)
            .ListEligibleForAdmissibilityDecisionAsync(new ListEligibleForAdmissibilityDecisionRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }
}
