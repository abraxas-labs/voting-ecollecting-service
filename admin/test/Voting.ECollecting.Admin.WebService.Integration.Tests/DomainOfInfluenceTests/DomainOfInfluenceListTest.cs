// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Net.Client;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.DomainOfInfluenceTests;

public class DomainOfInfluenceListTest : BaseGrpcTest<DomainOfInfluenceService.DomainOfInfluenceServiceClient>
{
    public DomainOfInfluenceListTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.DomainOfInfluences);
    }

    [Fact]
    public async Task TestCt()
    {
        var response = await CtSgStammdatenverwalterClient.ListAsync(new ListDomainOfInfluencesRequest());
        await Verify(response);
    }

    [Fact]
    public async Task TestMu()
    {
        var response = await MuSgStammdatenverwalterClient.ListAsync(new ListDomainOfInfluencesRequest());
        await Verify(response);
    }

    [Fact]
    public async Task TestOnlyECollecting()
    {
        var response = await CtSgStammdatenverwalterClient.ListAsync(new ListDomainOfInfluencesRequest { ECollectingEnabled = true });
        await Verify(response);
    }

    [Fact]
    public async Task TestLimitedDoiTypes()
    {
        var response = await CtSgStammdatenverwalterClient.ListAsync(new ListDomainOfInfluencesRequest { Types_ = { DomainOfInfluenceType.Ct } });
        await Verify(response);
    }

    [Fact]
    public async Task TestLimitedDoiTypesAndECollectingOnly()
    {
        var response = await CtSgStammdatenverwalterClient.ListAsync(new ListDomainOfInfluencesRequest { ECollectingEnabled = true, Types_ = { DomainOfInfluenceType.Ct } });
        await Verify(response);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new DomainOfInfluenceService.DomainOfInfluenceServiceClient(channel).ListAsync(new());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        return Roles.AllHumanUserRoles();
    }
}
