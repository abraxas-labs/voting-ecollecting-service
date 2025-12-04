// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.DomainOfInfluenceTests;

public class DomainOfInfluenceListTypesTest : BaseGrpcTest<DomainOfInfluenceService.DomainOfInfluenceServiceClient>
{
    public DomainOfInfluenceListTypesTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Test()
    {
        var response = await Client.ListTypesAsync(new ListDomainOfInfluenceTypesRequest());
        await Verify(response);
    }

    [Fact]
    public async Task TestOnlyCtDoiType()
    {
        await WithOnlyCtDomainOfInfluenceTypeEnabled(async () =>
        {
            var response = await Client.ListTypesAsync(new ListDomainOfInfluenceTypesRequest());
            await Verify(response);
        });
    }
}
