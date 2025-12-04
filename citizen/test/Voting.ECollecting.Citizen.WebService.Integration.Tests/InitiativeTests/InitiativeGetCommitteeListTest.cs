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

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeGetCommitteeListTest : BaseRestTest
{
    private readonly Guid _id = Files.BuildGuid(
        InitiativesCtStGallen.GuidLegislativeInPreparation,
        "committee-list-1.pdf");

    public InitiativeGetCommitteeListTest(TestApplicationFactory factory)
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
        var resp = await AuthenticatedClient.GetByteArrayAsync(BuildUrl());
        resp.Should().BeEquivalentTo(Files.PlaceholderCommitteeListPdf);
    }

    [Fact]
    public async Task ShouldGetAsDeputy()
    {
        var resp = await DeputyClient.GetByteArrayAsync(BuildUrl());
        resp.Should().BeEquivalentTo(Files.PlaceholderCommitteeListPdf);
    }

    [Fact]
    public async Task ShouldGetAsReader()
    {
        var resp = await DeputyClient.GetByteArrayAsync(BuildUrl());
        resp.Should().BeEquivalentTo(Files.PlaceholderCommitteeListPdf);
    }

    [Fact]
    public async Task TestInitiativeNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.GetAsync(BuildUrl("a20b30d1-e039-4954-a358-1d41a0aae06c")),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TestListNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.GetAsync(BuildUrl(fileId: "e29c4eb2-de55-48db-bcd2-d3d823f243c0")),
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

        await AuthenticatedClient.GetByteArrayAsync(BuildUrl());
    }

    private string BuildUrl(
        string initiativeId = InitiativesCtStGallen.IdLegislativeInPreparation,
        string? fileId = null)
        => $"v1/api/initiatives/{initiativeId}/committee-lists/{fileId ?? _id.ToString()}";
}
