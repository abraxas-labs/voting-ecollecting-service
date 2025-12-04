// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.DomainOfInfluenceTests;

public class DomainOfInfluenceListTest : BaseGrpcTest<DomainOfInfluenceService.DomainOfInfluenceServiceClient>
{
    public DomainOfInfluenceListTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Acl);
    }

    [Fact]
    public async Task Test()
    {
        var response = await Client.ListAsync(new ListDomainOfInfluencesRequest());
        await Verify(response);
    }

    [Fact]
    public async Task TestOnlyECollecting()
    {
        var response = await Client.ListAsync(new ListDomainOfInfluencesRequest { ECollectingEnabled = true });
        await Verify(response);
    }

    [Fact]
    public async Task TestLimitedDoiTypes()
    {
        var response = await Client.ListAsync(new ListDomainOfInfluencesRequest { Types_ = { DomainOfInfluenceType.Mu } });
        await Verify(response);
    }

    [Fact]
    public async Task TestLimitedDoiTypesAndECollecting()
    {
        var response = await Client.ListAsync(new ListDomainOfInfluencesRequest { ECollectingEnabled = true, Types_ = { DomainOfInfluenceType.Mu } });
        await Verify(response);
    }
}
