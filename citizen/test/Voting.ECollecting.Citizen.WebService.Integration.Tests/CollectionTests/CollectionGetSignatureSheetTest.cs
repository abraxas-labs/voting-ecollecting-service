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

public class CollectionGetSignatureSheetTest : BaseRestTest
{
    public CollectionGetSignatureSheetTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidUnityEnabledForCollectionCollecting, InitiativesCtStGallen.GuidLegislativeInPreparation));
    }

    [Fact]
    public async Task ShouldGet()
    {
        var data = await Client.GetByteArrayAsync(BuildUrl(InitiativesCtStGallen.IdUnityEnabledForCollectionCollecting));
        data.Should().BeEquivalentTo(Files.PlaceholderSignaturesPdf);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.GetAsync(BuildUrl("0c3406fe-ccba-4798-891d-18239a0d1b5d")),
            HttpStatusCode.NotFound);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksOnlyInEnabledCollection(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        if (state == CollectionState.EnabledForCollection)
        {
            await AuthenticatedClient.GetByteArrayAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation));
        }
        else
        {
            await AssertStatus(
                async () => await AuthenticatedClient.GetAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation)),
                HttpStatusCode.NotFound);
        }
    }

    private static string BuildUrl(string id)
        => $"v1/api/collections/{id}/signature-sheet-template";
}
