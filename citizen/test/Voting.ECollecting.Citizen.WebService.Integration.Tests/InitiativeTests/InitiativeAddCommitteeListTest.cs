// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Citizen.Api.Http.Responses;
using Voting.ECollecting.Citizen.Core.Exceptions;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeAddCommitteeListTest : BaseRestTest
{
    public InitiativeAddCommitteeListTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation, InitiativesCtStGallen.GuidLegislativeReturnedForCorrection));
    }

    [Fact]
    public async Task ShouldWork()
    {
        var content = BuildSimpleContent();
        using var resp =
            await AuthenticatedClient.PostAsync(BuildUrl(), content);
        resp.EnsureSuccessStatusCode();
        var jsonResponse = await resp.Content.ReadFromJsonAsync<AddCommitteeListResponse>();
        jsonResponse.Should().NotBeNull();
        jsonResponse!.Id.Should().NotBe(Guid.Empty);
        jsonResponse.Name.Should().Be("placeholder-committee-list_20200110-1412.pdf");

        var file = await RunOnDb(db => db.Files
            .Include(x => x.Content!)
            .SingleAsync(x => x.Id == jsonResponse.Id));
        file.CommitteeListOfInitiativeId.Should().Be(InitiativesCtStGallen.GuidLegislativeInPreparation);
        file.Name.Should().Be("placeholder-committee-list_20200110-1412.pdf");
        file.ContentType.Should().Be("application/pdf");
        file.Content!.Data.Should().BeEquivalentTo(Files.PlaceholderCommitteeListPdf);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            var content = BuildSimpleContent();
            using var resp = await AuthenticatedClient.PostAsync(BuildUrl(), content);
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldWorkAsDeputy()
    {
        var content = BuildSimpleContent();
        using var resp =
            await DeputyClient.PostAsync(BuildUrl(), content);
        resp.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ShouldThrowAsReader()
    {
        var content = BuildSimpleContent();
        await AssertStatus(
            async () => await ReaderClient.PostAsync(BuildUrl(), content),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsDeputyNotAccepted()
    {
        var content = BuildSimpleContent();
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.PostAsync(BuildUrl(), content),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowWithLockedFields()
    {
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection,
            x => x.LockedFields = new InitiativeLockedFields
            {
                CommitteeMembers = true,
            });

        var content = BuildSimpleContent();
        await AssertStatus(
            async () => await AuthenticatedClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeReturnedForCorrection), content),
            HttpStatusCode.BadRequest,
            nameof(CannotEditLockedFieldException),
            "Cannot edit locked field CommitteeMembers");
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        var content = BuildSimpleContent();
        var expectedStatus = state.InPreparationOrReturnForCorrection()
            ? HttpStatusCode.OK
            : HttpStatusCode.NotFound;

        await AssertStatus(
            async () => await AuthenticatedClient.PostAsync(BuildUrl(), content),
            expectedStatus);
    }

    private static MultipartFormDataContent BuildSimpleContent(string? contentType = null)
    {
        var imageContent = new ByteArrayContent(Files.PlaceholderCommitteeListPdf);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue(contentType ?? "application/pdf");

        var data = new MultipartFormDataContent();
        data.Add(imageContent, "file", Files.PlaceholderCommitteeListPdfName);
        return data;
    }

    private static string BuildUrl(string id = InitiativesCtStGallen.IdLegislativeInPreparation)
        => $"v1/api/initiatives/{id}/committee-lists";
}
