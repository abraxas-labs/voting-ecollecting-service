// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Grpc.Net.Client;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.DecreeTests;

public class DecreeGetForDeleteTest : BaseGrpcTest<DecreeService.DecreeServiceClient>
{
    public DecreeGetForDeleteTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Referendums);
    }

    [Fact]
    public async Task TestAsCtTenantWithReferendumsShouldWork()
    {
        var response = await CtSgStammdatenverwalterClient.GetForDeleteAsync(NewValidRequest());
        await Verify(response);
    }

    [Fact]
    public async Task TestAsCtTenantWithoutReferendumsShouldWork()
    {
        var response = await CtSgStammdatenverwalterClient.GetForDeleteAsync(NewValidRequest(r => r.DecreeId = DecreesCh.IdFutureNoReferendum));
        await Verify(response);
    }

    [Fact]
    public async Task TestAsMuTenantShouldWork()
    {
        var response = await MuSgStammdatenverwalterClient.GetForDeleteAsync(NewValidRequest(r => r.DecreeId = DecreesMuStGallen.IdFutureNoReferendum));
        await Verify(response);
    }

    [Fact]
    public async Task TestAsCtTenantWhenNoPermissionShouldThrowNotFound()
    {
        var request = NewValidRequest(r => r.DecreeId = DecreesMuStGallen.IdFutureNoReferendum);
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.GetForDeleteAsync(request),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestWhenInvalidIdShouldThrowNotFound()
    {
        var request = NewValidRequest(r => r.DecreeId = "f4590e18-0439-4402-8695-d5696c2a21a7");
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.GetForDeleteAsync(request),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
        => await new DecreeService.DecreeServiceClient(channel)
            .GetForDeleteAsync(NewValidRequest());

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private GetDecreeForDeleteRequest NewValidRequest(Action<GetDecreeForDeleteRequest>? customizer = null)
    {
        var request = new GetDecreeForDeleteRequest
        {
            DecreeId = DecreesCtStGallen.IdInPreparationWithReferendum,
        };
        customizer?.Invoke(request);
        return request;
    }
}
