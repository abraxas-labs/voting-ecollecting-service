// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.CollectionTests;

public class CollectionUpdateSignatureSheetTemplateTest : BaseRestTest
{
    public CollectionUpdateSignatureSheetTemplateTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation));
    }

    [Fact]
    public async Task ShouldUpdateSignatureSheet()
    {
        var oldFileId = await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.SignatureSheetTemplate!.Id)
            .SingleAsync());

        var content = BuildContent();
        using var resp =
            await AuthenticatedClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content);
        resp.EnsureSuccessStatusCode();

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.SignatureSheetTemplate!.Content)
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.SignatureSheetTemplateGenerated.Should().BeFalse();
        initiative.SignatureSheetTemplate.Should().NotBeNull();
        initiative.SignatureSheetTemplate!.Content.Should().NotBeNull();
        initiative.SignatureSheetTemplate.Name.Should().Be(Files.PlaceholderSignaturesPdfName);
        initiative.SignatureSheetTemplate.Content!.Data.Should().BeEquivalentTo(Files.PlaceholderSignaturesPdf);

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
            var content = BuildContent();
            using var resp =
                await AuthenticatedClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content);
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldUpdateSignatureSheetAsDeputy()
    {
        var oldFileId = await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.SignatureSheetTemplate!.Id)
            .SingleAsync());

        var content = BuildContent();
        using var resp =
            await DeputyClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content);
        resp.EnsureSuccessStatusCode();

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.SignatureSheetTemplate!.Content)
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.SignatureSheetTemplateGenerated.Should().BeFalse();
        initiative.SignatureSheetTemplate.Should().NotBeNull();
        initiative.SignatureSheetTemplate!.Content.Should().NotBeNull();
        initiative.SignatureSheetTemplate.Name.Should().Be(Files.PlaceholderSignaturesPdfName);
        initiative.SignatureSheetTemplate.Content!.Data.Should().BeEquivalentTo(Files.PlaceholderSignaturesPdf);

        var hasOldFile = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == oldFileId));
        hasOldFile.Should().BeFalse();

        var hasOldFileContent = await RunOnDb(db => db.FileContents.AnyAsync(x => x.FileId == oldFileId));
        hasOldFileContent.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldThrowMismatchedContentType()
    {
        var content = BuildContent(contentType: "application/json");
        await AssertStatus(
            async () => await AuthenticatedClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content),
            HttpStatusCode.BadRequest,
            "ValidationException",
            "File extensions differ. From file name: pdf, from content type: json, guessed from content: pdf");
    }

    [Fact]
    public async Task ShouldThrowMismatchedFileExtension()
    {
        var content = BuildContent(bytesContent: "{}"u8.ToArray(), contentType: "application/json", fileName: "sample.json");
        using var resp = await AuthenticatedClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content);
        await AssertStatus(
            async () => await AuthenticatedClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content),
            HttpStatusCode.BadRequest,
            "ValidationException",
            "File extension json is not allowed for uploads");
    }

    [Fact]
    public async Task TestNotFound()
    {
        var content = BuildContent();
        await AssertStatus(
            async () => await AuthenticatedClient.PostAsync(BuildUrl("36aa6325-bcf1-4c1f-a456-1c3dc7845c18"), content),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        var content = BuildContent();
        return AssertStatus(async () => await Client.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content), HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeputyNotAcceptedShouldFail()
    {
        var content = BuildContent();
        await AssertStatus(async () => await DeputyNotAcceptedClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content), HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReaderShouldFail()
    {
        var content = BuildContent();
        await AssertStatus(async () => await ReaderClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content), HttpStatusCode.NotFound);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        var content = BuildContent();
        if (state.IsEndedOrAborted())
        {
            await AssertStatus(
                async () => await AuthenticatedClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content),
                HttpStatusCode.NotFound);
        }
        else
        {
            await AuthenticatedClient.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparation), content);
        }
    }

    private static MultipartFormDataContent BuildContent(byte[]? bytesContent = null, string? contentType = null, string? fileName = null)
    {
        var content = new ByteArrayContent(bytesContent ?? Files.PlaceholderSignaturesPdf);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType ?? "application/pdf");

        var data = new MultipartFormDataContent();
        data.Add(content, "file", fileName ?? Files.PlaceholderSignaturesPdfName);
        return data;
    }

    private static string BuildUrl(string id)
        => $"v1/api/collections/{id}/signature-sheet-template";
}
