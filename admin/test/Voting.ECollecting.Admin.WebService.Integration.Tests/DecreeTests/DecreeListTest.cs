// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Net.Client;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.DecreeTests;

public class DecreeListTest : BaseGrpcTest<DecreeService.DecreeServiceClient>
{
    public DecreeListTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Decrees);
    }

    [Fact]
    public async Task TestAsCtTenantShouldWork()
    {
        var response = await CtSgStammdatenverwalterClient.ListAsync(new ListDecreesRequest());
        await Verify(response);
    }

    [Fact]
    public async Task TestAsMuTenantShouldWork()
    {
        var response = await MuSgStammdatenverwalterClient.ListAsync(new ListDecreesRequest());
        await Verify(response);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
        => await new DecreeService.DecreeServiceClient(channel)
            .ListAsync(new ListDecreesRequest());

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }
}
