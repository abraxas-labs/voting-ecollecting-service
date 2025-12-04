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

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.ReferendumTests;

public class ReferendumGetElectronicSignaturesProtocolTest : BaseRestTest
{
    public ReferendumGetElectronicSignaturesProtocolTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Referendums.WithReferendums(
                ReferendumsCtStGallen.GuidSignatureSheetsSubmitted,
                ReferendumsMuStGallen.GuidSignatureSheetsSubmitted));
    }

    [Fact]
    public async Task ShouldWork()
    {
        var resp = await AuthenticatedClient.GetStringAsync(BuildUrl());
        await VerifyJson(resp);
    }

    [Fact]
    public async Task ShouldWorkAsDeputy()
    {
        var resp = await DeputyClient.GetStringAsync(BuildUrl());
        await VerifyJson(resp);
    }

    [Fact]
    public async Task ShouldThrowAsReader()
    {
        await AssertStatus(
            async () => await ReaderClient.GetAsync(BuildUrl()),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsDeputyNotAccepted()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.GetAsync(BuildUrl()),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsUnauthenticated()
    {
        await AssertStatus(
            async () => await Client.GetAsync(BuildUrl()),
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShouldThrowNotFound()
    {
        await AssertStatus(
            async () => await ReaderClient.GetAsync(BuildUrl("b4362434-3466-4fc9-a200-24e0ca2d5e90")),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowMu()
    {
        await AssertStatus(
            async () => await ReaderClient.GetAsync(BuildUrl(ReferendumsMuStGallen.IdSignatureSheetsSubmitted)),
            HttpStatusCode.NotFound);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStates(CollectionState state)
    {
        await RunOnDb(db => db.Referendums
            .Where(x => x.Id == ReferendumsCtStGallen.GuidSignatureSheetsSubmitted)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        var expectedStatus = state.IsEnded()
            ? HttpStatusCode.OK
            : HttpStatusCode.NotFound;
        await AssertStatus(
            async () => await AuthenticatedClient.GetAsync(BuildUrl()),
            expectedStatus);
    }

    private static string BuildUrl(string collectionId = ReferendumsCtStGallen.IdSignatureSheetsSubmitted)
        => $"v1/api/collections/{collectionId}/electronic-signatures-protocol";
}
