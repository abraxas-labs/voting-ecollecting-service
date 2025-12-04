// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Net.Client;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.DomainOfInfluenceTests;

public class DomainOfInfluenceListOwnTypesTest : BaseGrpcTest<DomainOfInfluenceService.DomainOfInfluenceServiceClient>
{
    public DomainOfInfluenceListOwnTypesTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Acl);
    }

    [Fact]
    public async Task TestCt()
    {
        var response = await CtSgStammdatenverwalterClient.ListOwnTypesAsync(new ListDomainOfInfluenceOwnTypesRequest());
        await Verify(response);
    }

    [Fact]
    public async Task TestMu()
    {
        var response = await MuSgStammdatenverwalterClient.ListOwnTypesAsync(new ListDomainOfInfluenceOwnTypesRequest());
        await Verify(response);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new DomainOfInfluenceService.DomainOfInfluenceServiceClient(channel)
            .ListOwnTypesAsync(new ListDomainOfInfluenceOwnTypesRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles() => Roles.AllHumanUserRoles();
}
