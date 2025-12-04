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

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionGetSignatureSheetTest : BaseGrpcTest<CollectionSignatureSheetService.CollectionSignatureSheetServiceClient>
{
    private static readonly Guid _sheetId = CollectionSignatureSheets.BuildGuid(
        CollectionMunicipalities.BuildGuid(
            ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
            Bfs.MunicipalityStGallen),
        1);

    public CollectionGetSignatureSheetTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Referendums.WithReferendums(ReferendumsCtStGallen.GuidInCollectionEnabledForCollection, ReferendumsCtStGallen.GuidInCollectionEnabledForCollection2) with
            {
                SeedReferendumSignatureSheets = true,
            });
    }

    [Fact]
    public async Task ShouldWork()
    {
        var sheet = await MuSgKontrollzeichenerfasserClient.GetAsync(NewValidRequest());
        await Verify(sheet);
    }

    [Fact]
    public async Task ShouldThrowSheetNotFound()
    {
        var req = NewValidRequest();
        req.SignatureSheetId = "7cacc2b8-f77e-4d0c-9e7f-d846fe7ae840";
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.GetAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowCollectionNotFound()
    {
        var req = NewValidRequest();
        req.CollectionId = "79142d08-6c46-43ef-b434-a87ff8576b43";
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.GetAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowOtherCollection()
    {
        var req = NewValidRequest();
        req.CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection2;
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.GetAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowOtherTenant()
    {
        var req = NewValidRequest();
        req.SignatureSheetId = CollectionSignatureSheets.BuildGuid(
            CollectionMunicipalities.BuildGuid(
                ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
                Bfs.MunicipalityBergSG),
            1).ToString();
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.GetAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldWorkAttestedState()
    {
        var req = NewValidRequest();
        req.SignatureSheetId = CollectionSignatureSheets.BuildGuid(
            CollectionMunicipalities.BuildGuid(
                ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
                Bfs.MunicipalityStGallen),
            4).ToString();
        var resp = await MuSgKontrollzeichenerfasserClient.GetAsync(req);
        await Verify(resp);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionSignatureSheetService.CollectionSignatureSheetServiceClient(channel)
            .GetAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Kontrollzeichenerfasser;
        yield return Roles.Stichprobenverwalter;
    }

    private static GetSignatureSheetRequest NewValidRequest()
    {
        return new GetSignatureSheetRequest
        {
            CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection,
            SignatureSheetId = _sheetId.ToString(),
        };
    }
}
