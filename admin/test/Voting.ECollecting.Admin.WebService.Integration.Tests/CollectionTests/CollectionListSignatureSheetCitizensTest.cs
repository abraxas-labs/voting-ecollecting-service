// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Grpc.Net.Client;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionListSignatureSheetCitizensTest : BaseGrpcTest<CollectionSignatureSheetService.CollectionSignatureSheetServiceClient>
{
    private static readonly Guid _initiativeSgSheet1Guid = CollectionSignatureSheets.BuildGuid(
        CollectionMunicipalities.BuildGuid(
            InitiativesCh.GuidEnabledForCollectionCollecting,
            Bfs.MunicipalityStGallen),
        1);

    private static readonly Guid _referendumSgSheet1Guid = CollectionSignatureSheets.BuildGuid(
        CollectionMunicipalities.BuildGuid(
            ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
            Bfs.MunicipalityStGallen),
        1);

    public CollectionListSignatureSheetCitizensTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Default
                    .WithDecrees(DecreesCtStGallen.GuidInCollectionWithReferendum)
                    .WithInitiatives(InitiativesCh.GuidEnabledForCollectionCollecting));
    }

    [Fact]
    public async Task ShouldWork()
    {
        await Verify(await MuSgKontrollzeichenerfasserClient.ListCitizensAsync(NewValidInitiativeRequest()));
    }

    [Fact]
    public async Task ShouldWorkReferendum()
    {
        await Verify(await MuSgKontrollzeichenerfasserClient.ListCitizensAsync(NewValidReferendumRequest()));
    }

    [Fact]
    public async Task ShouldThrowAsOtherMu()
    {
        await AssertStatus(
            async () => await MuGoldachKontrollzeichenerfasserClient.ListCitizensAsync(NewValidInitiativeRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsCt()
    {
        await AssertStatus(
            async () => await CtSgKontrollzeichenerfasserClient.ListCitizensAsync(NewValidInitiativeRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowCollectionNotFound()
    {
        var req = NewValidInitiativeRequest(x => x.CollectionId = "70743aef-fd76-4a95-9dde-d033c3744001");
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.ListCitizensAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowSheetNotFound()
    {
        var req = NewValidInitiativeRequest(x => x.SignatureSheetId = "5a287c8e-db8a-4715-b971-2ed6fa4daae2");
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.ListCitizensAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldWorkSheetAttested()
    {
        var req = NewValidInitiativeRequest();
        await ModifyDbEntities<CollectionSignatureSheetEntity>(
            x => x.Id == Guid.Parse(req.SignatureSheetId),
            x => x.State = CollectionSignatureSheetState.Attested);
        await Verify(await MuSgKontrollzeichenerfasserClient.ListCitizensAsync(req));
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionSignatureSheetService.CollectionSignatureSheetServiceClient(channel)
            .ListCitizensAsync(NewValidInitiativeRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Kontrollzeichenerfasser;
        yield return Roles.Stichprobenverwalter;
    }

    private ListSignatureSheetCitizensRequest NewValidInitiativeRequest(Action<ListSignatureSheetCitizensRequest>? customizer = null)
    {
        var request = new ListSignatureSheetCitizensRequest
        {
            CollectionId = InitiativesCh.IdEnabledForCollectionCollecting,
            SignatureSheetId = _initiativeSgSheet1Guid.ToString(),
        };

        customizer?.Invoke(request);
        return request;
    }

    private ListSignatureSheetCitizensRequest NewValidReferendumRequest(Action<ListSignatureSheetCitizensRequest>? customizer = null)
    {
        var request = new ListSignatureSheetCitizensRequest
        {
            CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection,
            SignatureSheetId = _referendumSgSheet1Guid.ToString(),
        };

        customizer?.Invoke(request);
        return request;
    }
}
