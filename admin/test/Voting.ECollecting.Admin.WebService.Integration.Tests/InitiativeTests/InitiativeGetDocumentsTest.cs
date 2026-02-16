// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class InitiativeGetDocumentsTest : BaseRestTest
{
    public InitiativeGetDocumentsTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidUnitySignatureSheetsSubmitted, InitiativesMuStGallen.GuidSignatureSheetsSubmitted) with
        {
            SeedInitiativeSignatureSheets = true,
            SeedInitiativeCitizens = true,
        });
    }

    [Fact]
    public async Task ShouldGet()
    {
        var resp = await AssertZipDownloadAsStringEntries(
            async () => await CtStammdatenverwalterClient.GetAsync(BuildUrl(InitiativesCtStGallen.IdUnitySignatureSheetsSubmitted)),
            "export.zip");

        resp.Count.Should().Be(4);
        await Verify(resp);
    }

    [Fact]
    public async Task ShouldReturnEmptyAsMu()
    {
        var resp = await AssertZipDownloadAsStringEntries(
            async () => await MuSgStammdatenverwalterClient.GetAsync(BuildUrl(InitiativesMuStGallen.IdSignatureSheetsSubmitted)),
            "export.zip");

        resp.Should().HaveCount(2);
    }

    [Fact]
    public async Task ShouldGetWithReachedMaxElectronicSignatureCount()
    {
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesMuStGallen.GuidSignatureSheetsSubmitted,
            x => x.MaxElectronicSignatureCount = 2);

        var resp = await AssertZipDownloadAsStringEntries(
            async () => await MuSgStammdatenverwalterClient.GetAsync(BuildUrl(InitiativesMuStGallen.IdSignatureSheetsSubmitted)),
            "export.zip");

        resp.Count.Should().Be(2);
        await Verify(resp);
    }

    protected override Task<HttpResponseMessage> AuthorizationTestCall(HttpClient httpClient)
    {
        return httpClient.GetAsync(BuildUrl(InitiativesCtStGallen.IdUnitySignatureSheetsSubmitted));
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private static string BuildUrl(string initiativeId)
        => $"v1/api/initiatives/{initiativeId}/documents";
}
