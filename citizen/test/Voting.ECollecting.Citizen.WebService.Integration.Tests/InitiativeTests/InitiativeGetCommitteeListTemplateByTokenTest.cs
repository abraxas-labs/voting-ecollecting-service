// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.Lib.Common;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeGetCommitteeListTemplateByTokenTest : BaseRestTest
{
    private static readonly UrlToken _token =
        InitiativeCommitteeMembers.BuildToken(
            InitiativesCtStGallen.GuidLegislativeInPreparation,
            "margarita@example.com");

    public InitiativeGetCommitteeListTemplateByTokenTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation));
    }

    [Fact]
    public async Task ShouldWork()
    {
        var resp = await Client.PostAsync(BuildUrl(), BuildSimpleContent());
        resp.EnsureSuccessStatusCode();

        // the mock just returns the template bag as dmdock-json
        var content = await resp.Content.ReadAsStringAsync();
        await VerifyJson(content);
    }

    [Fact]
    public async Task ExpiredShouldThrow()
    {
        await ModifyDbEntities(
            (InitiativeCommitteeMemberEntity e) => e.Token == _token,
            e => e.TokenExpiry = MockedClock.GetDate(-4));

        await AssertStatus(
            async () => await Client.PostAsync(BuildUrl(), BuildSimpleContent()),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task NotFound()
    {
        await AssertStatus(
            async () => await Client.PostAsync(BuildUrl(), BuildSimpleContent(UrlToken.New())),
            HttpStatusCode.NotFound);
    }

    private static MultipartFormDataContent BuildSimpleContent(UrlToken? token = null)
    {
        var data = new MultipartFormDataContent();
        data.Add(new StringContent(token ?? _token), "token");
        return data;
    }

    private static string BuildUrl(string id = InitiativesCtStGallen.IdLegislativeInPreparation)
        => $"v1/api/initiatives/{id}/committee-members/template";
}
