// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net;
using FluentAssertions;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionGetSignatureSheetTemplateTest : BaseRestTest
{
    public CollectionGetSignatureSheetTemplateTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation, InitiativesMuStGallen.GuidInPreparation));
    }

    [Fact]
    public async Task ShouldGet()
    {
        var data = await CtStammdatenverwalterClient.GetByteArrayAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation));
        data.Should().BeEquivalentTo(Files.PlaceholderSignaturesPdf);
    }

    [Fact]
    public async Task ShouldGetAsMu()
    {
        var data = await MuSgStammdatenverwalterClient.GetByteArrayAsync(BuildUrl(InitiativesMuStGallen.IdInPreparation));
        data.Should().BeEquivalentTo(Files.PlaceholderSignaturesPdf);
    }

    [Fact]
    public async Task ShouldThrowAsMuOnOtherMu()
    {
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.GetAsync(BuildUrl(InitiativesMuStGallen.IdInPreparation)),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await CtStammdatenverwalterClient.GetAsync(BuildUrl("c1aed85c-9376-47cf-b811-a8da8d209448")),
            HttpStatusCode.NotFound);
    }

    protected override Task<HttpResponseMessage> AuthorizationTestCall(HttpClient httpClient)
    {
        return httpClient.GetAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation));
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        return Roles.AllHumanUserRoles();
    }

    private static string BuildUrl(string id)
        => $"v1/api/collections/{id}/signature-sheet-template";
}
