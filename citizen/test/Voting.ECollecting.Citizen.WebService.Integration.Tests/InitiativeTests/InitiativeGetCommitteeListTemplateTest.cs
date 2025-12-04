// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeGetCommitteeListTemplateTest : BaseRestTest
{
    public InitiativeGetCommitteeListTemplateTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation));
    }

    [Fact]
    public async Task ShouldGet()
    {
        var resp = await AuthenticatedClient.GetStringAsync(BuildUrl());
        await VerifyJson(resp);
    }

    [Fact]
    public async Task ShouldGetAsDeputy()
    {
        var resp = await DeputyClient.GetStringAsync(BuildUrl());
        await VerifyJson(resp);
    }

    [Fact]
    public async Task ShouldGetAsReader()
    {
        var resp = await DeputyClient.GetStringAsync(BuildUrl());
        await VerifyJson(resp);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.GetAsync(BuildUrl("a20b30d1-e039-4954-a358-1d41a0aae06c")),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public Task DeputyNotAcceptedShouldFail()
    {
        return AssertStatus(async () => await DeputyNotAcceptedClient.GetAsync(BuildUrl()), HttpStatusCode.NotFound);
    }

    [Fact]
    public Task ReaderNotAcceptedShouldFail()
    {
        return AssertStatus(async () => await ReaderNotAcceptedClient.GetAsync(BuildUrl()), HttpStatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(async () => await Client.GetAsync(BuildUrl()), HttpStatusCode.Unauthorized);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInAllStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        var expectedStatus = state.InPreparationOrReturnForCorrection()
            ? HttpStatusCode.OK
            : HttpStatusCode.NotFound;
        await AssertStatus(
            async () => await AuthenticatedClient.GetAsync(BuildUrl()),
            expectedStatus);
    }

    private static string BuildUrl(string id = InitiativesCtStGallen.IdLegislativeInPreparation)
        => $"v1/api/initiatives/{id}/committee-lists/template";
}
