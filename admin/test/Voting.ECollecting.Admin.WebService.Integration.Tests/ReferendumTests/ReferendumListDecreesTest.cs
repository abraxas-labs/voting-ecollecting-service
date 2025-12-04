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

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.ReferendumTests;

public class ReferendumListDecreesTest : BaseGrpcTest<ReferendumService.ReferendumServiceClient>
{
    public ReferendumListDecreesTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Referendums);
    }

    [Fact]
    public async Task TestAsCtTenantShouldWork()
    {
        var resp = await CtSgStammdatenverwalterClient.ListDecreesAsync(new());
        await Verify(resp);
    }

    [Fact]
    public async Task TestAsMuTenantShouldWork()
    {
        var resp = await MuSgStammdatenverwalterClient.ListDecreesAsync(new());
        await Verify(resp);
    }

    [Fact]
    public async Task TestFilterCtShouldWork()
    {
        var resp = await CtSgStammdatenverwalterClient.ListDecreesAsync(new ListReferendumDecreesRequest
        {
            Types_ = { DomainOfInfluenceType.Ct },
        });
        await Verify(resp);
    }

    [Fact]
    public async Task TestFilterBfsShouldWork()
    {
        var resp = await CtSgStammdatenverwalterClient.ListDecreesAsync(new ListReferendumDecreesRequest
        {
            Bfs = Bfs.MunicipalityStGallen,
        });
        await Verify(resp);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new ReferendumService.ReferendumServiceClient(channel).ListDecreesAsync(new());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        return Roles.All();
    }
}
