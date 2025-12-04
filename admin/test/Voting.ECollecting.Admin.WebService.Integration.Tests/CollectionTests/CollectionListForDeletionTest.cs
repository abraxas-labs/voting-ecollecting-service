// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Net.Client;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Enums;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionListForDeletionTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionListForDeletionTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Collections);
    }

    [Fact]
    public async Task ShouldWorkReadyToDelete()
    {
        var resp = await CtSgKontrollzeichenloescherClient.ListForDeletionAsync(NewValidRequest());
        await Verify(resp);
    }

    [Fact]
    public async Task ShouldWorkReminderSet()
    {
        var req = NewValidRequest(x => x.Filter = CollectionControlSignFilter.ReminderSet);
        var resp = await CtSgKontrollzeichenloescherClient.ListForDeletionAsync(req);
        await Verify(resp);
    }

    [Fact]
    public async Task ShouldWorkWithDoiTypes()
    {
        var req = NewValidRequest();
        req.Types_.Clear();
        req.Types_.Add(DomainOfInfluenceType.Ct);
        var resp = await CtSgKontrollzeichenloescherClient.ListForDeletionAsync(req);
        await Verify(resp);
    }

    [Fact]
    public async Task ShouldWorkWithBfs()
    {
        var req = NewValidRequest();
        req.Bfs = Bfs.MunicipalityStGallen;
        var resp = await MuSgKontrollzeichenloescherClient.ListForDeletionAsync(req);
        await Verify(resp);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionService.CollectionServiceClient(channel)
            .ListForDeletionAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles() => [Roles.Kontrollzeichenloescher];

    private static ListCollectionsForDeletionRequest NewValidRequest(Action<ListCollectionsForDeletionRequest>? customizer = null)
    {
        var req = new ListCollectionsForDeletionRequest
        {
            Filter = CollectionControlSignFilter.ReadyToDelete,
        };
        customizer?.Invoke(req);
        return req;
    }
}
