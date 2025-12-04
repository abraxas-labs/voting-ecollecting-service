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

public class CollectionUpdateImageTest : BaseRestTest
{
    public CollectionUpdateImageTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation));
    }

    [Fact]
    public async Task ShouldUpdateImage()
    {
        var oldFileId = await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.Image!.Id)
            .SingleAsync());

        var content = BuildSimpleContent();
        using var resp =
            await AuthenticatedClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content);
        resp.EnsureSuccessStatusCode();

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.Image!.Content)
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.Image.Should().NotBeNull();
        initiative.Image!.Content.Should().NotBeNull();
        initiative.ImageId.Should().NotBe(oldFileId);
        initiative.Image.Name.Should().Be(Files.PlaceholderPngName);
        initiative.Image.ContentType.Should().Be("image/png");
        initiative.Image.Content!.Data.Should().BeEquivalentTo(Files.PlaceholderPng);

        var hasOldFile = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == oldFileId));
        hasOldFile.Should().BeFalse();

        var hasOldFileContent = await RunOnDb(db => db.FileContents.AnyAsync(x => x.FileId == oldFileId));
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
    public async Task ShouldUpdateJpgImage()
    {
        var oldFileId = await RunOnDb(db => db.Initiatives.Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.Image!.Id)
            .SingleAsync());

        var content = BuildSimpleJpgContent();
        using var resp =
            await AuthenticatedClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content);
        resp.EnsureSuccessStatusCode();

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.Image!.Content)
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.Image.Should().NotBeNull();
        initiative.Image!.Content.Should().NotBeNull();
        initiative.Image.Name.Should().Be(Files.PlaceholderJpgName);
        initiative.Image.ContentType.Should().Be("image/jpeg");
        initiative.Image.Content!.Data.Should().BeEquivalentTo(Files.PlaceholderJpg);

        var hasOldFile = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == oldFileId));
        hasOldFile.Should().BeFalse();

        var hasOldFileContent = await RunOnDb(db => db.FileContents.AnyAsync(x => x.FileId == oldFileId));
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
    public async Task TestShouldUpdateImageAsDeputy()
    {
        var fileId = await RunOnDb(db => db.Initiatives.Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.Image!.Id)
            .SingleAsync());

        var content = BuildSimpleContent();
        using var resp =
            await DeputyClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content);
        resp.EnsureSuccessStatusCode();

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.Image!.Content)
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.Image.Should().NotBeNull();
        initiative.Image!.Content.Should().NotBeNull();
        initiative.Image.Name.Should().Be(Files.PlaceholderPngName);
        initiative.Image.ContentType.Should().Be("image/png");
        initiative.Image.Content!.Data.Should().BeEquivalentTo(Files.PlaceholderPng);

        var hasFile = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == fileId));
        hasFile.Should().BeFalse();

        var hasFileContent = await RunOnDb(db => db.FileContents.AnyAsync(x => x.FileId == fileId));
        hasFileContent.Should().BeFalse();
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
            async () => await AuthenticatedClient.PostAsync(BuildUrl("3bb0a980-e449-40a1-a918-9158edf6dc5d"), content),
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
        var imageContent = new ByteArrayContent(Files.PlaceholderPng);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue(contentType ?? "image/png");

        var data = new MultipartFormDataContent();
        data.Add(imageContent, "image", Files.PlaceholderPngName);
        return data;
    }

    private static MultipartFormDataContent BuildSimpleJpgContent()
    {
        var imageContent = new ByteArrayContent(Files.PlaceholderJpg);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        var data = new MultipartFormDataContent();
        data.Add(imageContent, "image", Files.PlaceholderJpgName);
        return data;
    }

    private static MultipartFormDataContent BuildSimpleJsonContent()
    {
        var jsonContent = new StringContent("{}", Encoding.UTF8, "application/json");
        var data = new MultipartFormDataContent();
        data.Add(jsonContent, "image", "simple.json");
        return data;
    }

    private static string BuildUrl(string id)
        => $"v1/api/collections/{id}/image";
}
