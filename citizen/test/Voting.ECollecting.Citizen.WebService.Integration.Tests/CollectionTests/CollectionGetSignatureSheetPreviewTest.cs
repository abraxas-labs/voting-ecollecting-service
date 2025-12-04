// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.CollectionTests;

public class CollectionGetSignatureSheetPreviewTest : BaseRestTest
{
    public CollectionGetSignatureSheetPreviewTest(TestApplicationFactory factory)
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
        var data = await AuthenticatedClient.GetByteArrayAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation));
        data.Should().BeEquivalentTo(Files.PlaceholderSignaturesPdf);
    }

    [Fact]
    public async Task ShouldGetAsDeputy()
    {
        var data = await DeputyClient.GetByteArrayAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation));
        data.Should().BeEquivalentTo(Files.PlaceholderSignaturesPdf);
    }

    [Fact]
    public async Task ShouldGetAsReader()
    {
        var data = await ReaderClient.GetByteArrayAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation));
        data.Should().BeEquivalentTo(Files.PlaceholderSignaturesPdf);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.GetAsync(BuildUrl("0c3406fe-ccba-4798-891d-18239a0d1b5d")),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public Task DeputyNotAcceptedShouldFail()
    {
        return AssertStatus(async () => await DeputyNotAcceptedClient.GetAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation)), HttpStatusCode.NotFound);
    }

    [Fact]
    public Task ReaderNotAcceptedShouldFail()
    {
        return AssertStatus(async () => await ReaderNotAcceptedClient.GetAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation)), HttpStatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(async () => await Client.GetAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation)), HttpStatusCode.Unauthorized);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInAllStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        await AuthenticatedClient.GetByteArrayAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation));
    }

    private static string BuildUrl(string id)
        => $"v1/api/collections/{id}/signature-sheet-template/preview";
}
