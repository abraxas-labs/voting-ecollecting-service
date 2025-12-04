// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net;
using FluentAssertions;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class InitiativeGetCommitteeListTest : BaseRestTest
{
    private readonly string _idCommitteeListCt = Files.BuildGuid(
        InitiativesCtStGallen.GuidLegislativeInPreparation,
        "committee-list-1.pdf").ToString();

    private readonly string _idCommitteeListMu = Files.BuildGuid(
        InitiativesMuStGallen.GuidInPreparation,
        "committee-list-1.pdf").ToString();

    public InitiativeGetCommitteeListTest(TestApplicationFactory factory)
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
        var data = await CtStammdatenverwalterClient.GetByteArrayAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation, _idCommitteeListCt));
        data.Should().BeEquivalentTo(Files.PlaceholderCommitteeListPdf);
    }

    [Fact]
    public async Task AsMuOnCtInitiativeShouldWork()
    {
        var data = await MuSgStammdatenverwalterClient.GetByteArrayAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation, _idCommitteeListCt));
        data.Should().BeEquivalentTo(Files.PlaceholderCommitteeListPdf);
    }

    [Fact]
    public async Task AsMuOnOwnCollectionShouldWork()
    {
        var data = await MuSgStammdatenverwalterClient.GetByteArrayAsync(BuildUrl(InitiativesMuStGallen.IdInPreparation, _idCommitteeListMu));
        data.Should().BeEquivalentTo(Files.PlaceholderCommitteeListPdf);
    }

    [Fact]
    public async Task AsMuOnOtherMuCollectionShouldThrow()
    {
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.GetAsync(BuildUrl(InitiativesMuStGallen.IdInPreparation, _idCommitteeListMu)),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AsCtOnMuCollectionShouldWork()
    {
        var data = await CtStammdatenverwalterClient.GetByteArrayAsync(BuildUrl(InitiativesMuStGallen.IdInPreparation, _idCommitteeListMu));
        data.Should().BeEquivalentTo(Files.PlaceholderCommitteeListPdf);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await CtStammdatenverwalterClient.GetAsync(BuildUrl("641c92b4-ff60-47a5-ae4d-ee4acd4e9c31", _idCommitteeListCt)),
            HttpStatusCode.NotFound);
    }

    protected override Task<HttpResponseMessage> AuthorizationTestCall(HttpClient httpClient)
    {
        return httpClient.GetAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation, _idCommitteeListCt));
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private static string BuildUrl(string initiativeId, string id)
        => $"v1/api/initiatives/{initiativeId}/committee-lists/{id}";
}
