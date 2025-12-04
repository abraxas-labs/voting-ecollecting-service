// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.CollectionTests;

public class CollectionUpdateLogoTest : BaseRestTest
{
    public CollectionUpdateLogoTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation));
    }

    [Fact]
    public async Task ShouldUpdateLogo()
    {
        var fileId = await RunOnDb(db => db.Initiatives.Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.Logo!.Id)
            .SingleAsync());

        var content = BuildSimpleContent();
        using var resp =
            await AuthenticatedClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content);
        resp.EnsureSuccessStatusCode();

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.Logo!.Content)
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.Logo.Should().NotBeNull();
        initiative.Logo!.Content.Should().NotBeNull();
        initiative.Logo.Name.Should().Be(Files.PlaceholderPngName);
        initiative.Logo.ContentType.Should().Be("image/png");
        initiative.Logo.Content!.Data.Should().BeEquivalentTo(Files.PlaceholderPng);

        var hasOldFile = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == fileId));
        hasOldFile.Should().BeFalse();

        var hasOldFileContent = await RunOnDb(db => db.FileContents.AnyAsync(x => x.FileId == fileId));
        hasOldFileContent.Should().BeFalse();
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            var content = BuildSimpleContent();
            using var resp =
                await AuthenticatedClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content);
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldUpdateJpgLogo()
    {
        var fileId = await RunOnDb(db => db.Initiatives.Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.Logo!.Id)
            .SingleAsync());

        var content = BuildSimpleJpgContent();
        using var resp =
            await AuthenticatedClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content);
        resp.EnsureSuccessStatusCode();

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.Logo!.Content)
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.Logo.Should().NotBeNull();
        initiative.Logo!.Content.Should().NotBeNull();
        initiative.Logo.Name.Should().Be(Files.PlaceholderJpgName);
        initiative.Logo.ContentType.Should().Be("image/jpeg");
        initiative.Logo.Content!.Data.Should().BeEquivalentTo(Files.PlaceholderJpg);

        var hasOldFile = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == fileId));
        hasOldFile.Should().BeFalse();

        var hasOldFileContent = await RunOnDb(db => db.FileContents.AnyAsync(x => x.FileId == fileId));
        hasOldFileContent.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldThrowMismatchedContentType()
    {
        var content = BuildSimpleContent("application/pdf");
        await AssertStatus(
            async () => await AuthenticatedClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content),
            HttpStatusCode.BadRequest,
            "ValidationException",
            "File extensions differ. From file name: png, from content type: pdf, guessed from content: png");
    }

    [Fact]
    public async Task ShouldThrowMismatchedFileExtension()
    {
        var content = BuildSimpleJsonContent();
        await AssertStatus(
            async () => await AuthenticatedClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content),
            HttpStatusCode.BadRequest,
            "ValidationException",
            "File extension json is not allowed for uploads");
    }

    [Fact]
    public async Task TestShouldUpdateLogoAsDeputy()
    {
        var content = BuildSimpleContent();
        using var resp =
            await DeputyClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content);
        resp.EnsureSuccessStatusCode();

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.Logo!.Content)
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.Logo.Should().NotBeNull();
        initiative.Logo!.Content.Should().NotBeNull();
        initiative.Logo.Name.Should().Be(Files.PlaceholderPngName);
        initiative.Logo.ContentType.Should().Be("image/png");
        initiative.Logo.Content!.Data.Should().BeEquivalentTo(Files.PlaceholderPng);
    }

    [Fact]
    public async Task AsReaderShouldFail()
    {
        var content = BuildSimpleContent();
        await AssertStatus(
            async () => await ReaderClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AsDeputyNotAcceptedShouldFail()
    {
        var content = BuildSimpleContent();
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TestNotFound()
    {
        var content = BuildSimpleContent();
        await AssertStatus(
            async () => await AuthenticatedClient.PostAsync(BuildUrl("bc60715a-09c4-45d8-bd71-f447a3879b0c"), content),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        var content = BuildSimpleContent();
        return AssertStatus(async () => await Client.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content), HttpStatusCode.Unauthorized);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        var content = BuildSimpleContent();
        if (state.InPreparationOrReturnForCorrection())
        {
            await AuthenticatedClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content);
        }
        else
        {
            await AssertStatus(
                async () => await AuthenticatedClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content),
                HttpStatusCode.NotFound);
        }
    }

    private static MultipartFormDataContent BuildSimpleContent(string? contentType = null)
    {
        var logoContent = new ByteArrayContent(Files.PlaceholderPng);
        logoContent.Headers.ContentType = new MediaTypeHeaderValue(contentType ?? "image/png");

        var data = new MultipartFormDataContent();
        data.Add(logoContent, "logo", Files.PlaceholderPngName);
        return data;
    }

    private static MultipartFormDataContent BuildSimpleJpgContent()
    {
        var logoContent = new ByteArrayContent(Files.PlaceholderJpg);
        logoContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        var data = new MultipartFormDataContent();
        data.Add(logoContent, "logo", Files.PlaceholderJpgName);
        return data;
    }

    private static MultipartFormDataContent BuildSimpleJsonContent()
    {
        var jsonContent = new StringContent("{}", Encoding.UTF8, "application/json");
        var data = new MultipartFormDataContent();
        data.Add(jsonContent, "logo", "simple.json");
        return data;
    }

    private static string BuildUrl(string id)
        => $"v1/api/collections/{id}/logo";
}
