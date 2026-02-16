// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.DecreeTests;

public class DecreeGetDocumentsTest : BaseRestTest
{
    public DecreeGetDocumentsTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Referendums with
        {
            SeedReferendumSignatureSheets = true,
            SeedReferendumCitizens = true,
        });
    }

    [Fact]
    public async Task ShouldGet()
    {
        var resp = await AssertZipDownloadAsStringEntries(
            async () => await CtStammdatenverwalterClient.GetAsync(BuildUrl(DecreesCh.IdInCollection)),
            "export.zip");

        resp.Count.Should().Be(5);
        await Verify(resp);
    }

    [Fact]
    public async Task ShouldReturnEmptyAsMu()
    {
        var resp = await AssertZipDownloadAsStringEntries(
            async () => await MuSgStammdatenverwalterClient.GetAsync(BuildUrl(DecreesMuStGallen.IdInCollectionWithReferendum)),
            "export.zip");

        resp.Should().HaveCount(2);
    }

    [Fact]
    public async Task ShouldGetWithReachedMaxElectronicSignatureCount()
    {
        await ModifyDbEntities<ReferendumEntity>(
            x => x.DecreeId == DecreesCh.GuidInCollection,
            x => x.MaxElectronicSignatureCount = 2);

        var resp = await AssertZipDownloadAsStringEntries(
            async () => await CtStammdatenverwalterClient.GetAsync(BuildUrl(DecreesCh.IdInCollection)),
            "export.zip");

        resp.Count.Should().Be(5);
        await Verify(resp);
    }

    protected override Task<HttpResponseMessage> AuthorizationTestCall(HttpClient httpClient)
    {
        return httpClient.GetAsync(BuildUrl(DecreesCh.IdInCollection));
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private static string BuildUrl(string decreeId)
        => $"v1/api/decrees/{decreeId}/documents";
}
