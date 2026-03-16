// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionReattestSignatureSheetTest : BaseRestTest
{
    public CollectionReattestSignatureSheetTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Default
                    .WithReferendums(ReferendumsCtStGallen.GuidInCollectionEnabledForCollection)
                    .WithInitiatives(InitiativesCtStGallen.GuidUnityEnabledForCollectionCollecting) with
            {
                SeedReferendumSignatureSheets = true,
                SeedInitiativeSignatureSheets = true,
            });
    }

    [Fact]
    public async Task ShouldWorkReferendum()
    {
        HashSet<Guid> sheetIds =
        [
            CollectionSignatureSheets.BuildGuid(
                CollectionMunicipalities.BuildGuid(
                    ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
                    Bfs.MunicipalityStGallen),
                4),
            CollectionSignatureSheets.BuildGuid(
                CollectionMunicipalities.BuildGuid(
                    ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
                    Bfs.MunicipalityStGallen),
                5),
        ];

        var resp = await MuSgKontrollzeichenerstellerClient.PostAsJsonAsync(
            BuildUrl(ReferendumsCtStGallen.IdInCollectionEnabledForCollection),
            sheetIds);
        resp.EnsureSuccessStatusCode();

        var responseData = await resp.Content.ReadAsStringAsync();

        var sheets = await RunOnDb(db => db.CollectionSignatureSheets
            .Where(x => sheetIds.Contains(x.Id))
            .OrderBy(x => x.Number)
            .ToListAsync());
        sheets.All(x => x.State == CollectionSignatureSheetState.Attested).Should().BeTrue();

        await Verify(new { sheets, responseData });
    }

    [Fact]
    public async Task ShouldWorkInitiative()
    {
        HashSet<Guid> sheetIds =
        [
            CollectionSignatureSheets.BuildGuid(
                CollectionMunicipalities.BuildGuid(
                    InitiativesCtStGallen.GuidUnityEnabledForCollectionCollecting,
                    Bfs.MunicipalityStGallen),
                4),
            CollectionSignatureSheets.BuildGuid(
                CollectionMunicipalities.BuildGuid(
                    InitiativesCtStGallen.GuidUnityEnabledForCollectionCollecting,
                    Bfs.MunicipalityStGallen),
                5),
        ];

        var resp = await MuSgKontrollzeichenerstellerClient.PostAsJsonAsync(
            BuildUrl(InitiativesCtStGallen.IdUnityEnabledForCollectionCollecting),
            sheetIds);
        resp.EnsureSuccessStatusCode();

        var responseData = await resp.Content.ReadAsStringAsync();

        var sheets = await RunOnDb(db => db.CollectionSignatureSheets
            .Where(x => sheetIds.Contains(x.Id))
            .OrderBy(x => x.Number)
            .ToListAsync());
        sheets.All(x => x.State == CollectionSignatureSheetState.Attested).Should().BeTrue();

        await Verify(new { sheets, responseData });
    }

    [Fact]
    public async Task ShouldThrowOneNotFound()
    {
        HashSet<Guid> sheetIds =
        [
            CollectionSignatureSheets.BuildGuid(
                CollectionMunicipalities.BuildGuid(
                    ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
                    Bfs.MunicipalityStGallen),
                1),
            CollectionSignatureSheets.BuildGuid(
                CollectionMunicipalities.BuildGuid(
                    ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
                    Bfs.MunicipalityStGallen),
                99),
        ];

        await AssertStatus(
            async () => await MuSgKontrollzeichenerstellerClient.PostAsJsonAsync(
                BuildUrl(ReferendumsCtStGallen.IdInCollectionEnabledForCollection),
                sheetIds),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowOtherTenant()
    {
        HashSet<Guid> sheetIds =
        [
            CollectionSignatureSheets.BuildGuid(
                CollectionMunicipalities.BuildGuid(
                    ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
                    Bfs.MunicipalityStGallen),
                4),
            CollectionSignatureSheets.BuildGuid(
                CollectionMunicipalities.BuildGuid(
                    ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
                    Bfs.MunicipalityStGallen),
                5),
        ];

        await AssertStatus(
            async () => await MuGoldachKontrollzeichenerstellerClient.PostAsJsonAsync(
                BuildUrl(ReferendumsCtStGallen.IdInCollectionEnabledForCollection),
                sheetIds),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MunicipalityLockedShouldThrow()
    {
        await ModifyDbEntities<CollectionMunicipalityEntity>(
            x => x.CollectionId == ReferendumsCtStGallen.GuidInCollectionEnabledForCollection && x.Bfs == Bfs.MunicipalityStGallen,
            x => x.IsLocked = true);

        HashSet<Guid> sheetIds =
        [
            CollectionSignatureSheets.BuildGuid(
                CollectionMunicipalities.BuildGuid(
                    ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
                    Bfs.MunicipalityStGallen),
                4),
            CollectionSignatureSheets.BuildGuid(
                CollectionMunicipalities.BuildGuid(
                    ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
                    Bfs.MunicipalityStGallen),
                5),
        ];

        await AssertStatus(
            async () => await MuSgKontrollzeichenerstellerClient.PostAsJsonAsync(
                BuildUrl(ReferendumsCtStGallen.IdInCollectionEnabledForCollection),
                sheetIds),
            HttpStatusCode.NotFound);
    }

    protected override async Task<HttpResponseMessage> AuthorizationTestCall(HttpClient httpClient)
    {
        return await httpClient.PostAsJsonAsync<IEnumerable<Guid>>(
            BuildUrl(ReferendumsCtStGallen.IdInCollectionEnabledForCollection),
            [
                CollectionSignatureSheets.BuildGuid(
                    CollectionMunicipalities.BuildGuid(
                        ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
                        Bfs.MunicipalityStGallen),
                    4)
            ]);
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Kontrollzeichenerfasser;
    }

    private static string BuildUrl(string collectionId)
        => $"v1/api/collections/{collectionId}/signature-sheets/reattest";
}
