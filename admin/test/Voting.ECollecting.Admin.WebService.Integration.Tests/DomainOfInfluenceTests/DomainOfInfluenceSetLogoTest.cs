// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.DomainOfInfluenceTests;

public class DomainOfInfluenceSetLogoTest : BaseRestTest
{
    public DomainOfInfluenceSetLogoTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.DomainOfInfluences);
    }

    [Fact]
    public async Task ShouldUpdateLogo()
    {
        var fileId = await RunOnDb(db => db.DomainOfInfluences
            .Where(x => x.Id == DomainOfInfluences.GuidCtStGallen)
            .Select(x => x.Logo!.Id)
            .SingleAsync());

        var content = BuildSimpleContent();
        using var resp =
            await CtStammdatenverwalterClient.PostAsync(BuildUrl(Bfs.CantonStGallen), content);
        resp.EnsureSuccessStatusCode();

        var doi = await RunOnDb(db => db.DomainOfInfluences
            .Include(x => x.Logo!.Content)
            .FirstAsync(x => x.Id == DomainOfInfluences.GuidCtStGallen));
        doi.Logo.Should().NotBeNull();
        doi.Logo!.Content.Should().NotBeNull();
        doi.Logo.Name.Should().Be(Files.PlaceholderPngName);
        doi.Logo.ContentType.Should().Be("image/png");
        doi.Logo.Content!.Data.Should().BeEquivalentTo(Files.PlaceholderPng);

        var hasOldFile = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == fileId));
        hasOldFile.Should().BeFalse();

        var hasOldFileContent = await RunOnDb(db => db.FileContents.AnyAsync(x => x.FileId == fileId));
        hasOldFileContent.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldUpdateJpgLogo()
    {
        var fileId = await RunOnDb(db => db.DomainOfInfluences
            .Where(x => x.Id == DomainOfInfluences.GuidCtStGallen)
            .Select(x => x.Logo!.Id)
            .SingleAsync());

        var content = BuildSimpleJpgContent();
        using var resp =
            await CtStammdatenverwalterClient.PostAsync(BuildUrl(Bfs.CantonStGallen), content);
        resp.EnsureSuccessStatusCode();

        var initiative = await RunOnDb(db => db.DomainOfInfluences
            .Include(x => x.Logo!.Content)
            .FirstAsync(x => x.Id == DomainOfInfluences.GuidCtStGallen));
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
    public async Task ShouldWorkAsMu()
    {
        var content = BuildSimpleContent();
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.PostAsync(BuildUrl(Bfs.MunicipalityStGallen), content),
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task ShouldThrowAsCtOnMu()
    {
        var content = BuildSimpleContent();
        await AssertStatus(
            async () => await CtStammdatenverwalterClient.PostAsync(BuildUrl(Bfs.MunicipalityStGallen), content),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsMuOnCt()
    {
        var content = BuildSimpleContent();
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.PostAsync(BuildUrl(Bfs.CantonStGallen), content),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsMuOnOtherMu()
    {
        var content = BuildSimpleContent();
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.PostAsync(BuildUrl(Bfs.MunicipalityStGallen), content),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowMismatchedContentType()
    {
        var content = BuildSimpleContent("application/pdf");
        await AssertStatus(
            async () => await CtStammdatenverwalterClient.PostAsync(BuildUrl(Bfs.CantonStGallen), content),
            HttpStatusCode.BadRequest,
            nameof(ValidationException),
            "File extensions differ. From file name: png, from content type: pdf, guessed from content: png");
    }

    [Fact]
    public async Task ShouldThrowMismatchedFileExtension()
    {
        var content = BuildSimpleJsonContent();
        await AssertStatus(
            async () => await CtStammdatenverwalterClient.PostAsync(BuildUrl(Bfs.CantonStGallen), content),
            HttpStatusCode.BadRequest,
            "ValidationException",
            "File extension json is not allowed for uploads");
    }

    [Fact]
    public async Task TestNotFound()
    {
        var content = BuildSimpleContent();
        await AssertStatus(
            async () => await CtStammdatenverwalterClient.PostAsync(BuildUrl("foobarbaz"), content),
            HttpStatusCode.NotFound);
    }

    protected override async Task<HttpResponseMessage> AuthorizationTestCall(HttpClient httpClient)
        => await httpClient.PostAsync(BuildUrl("1234"), BuildSimpleContent());

    protected override IEnumerable<string> AuthorizedRoles() => [Roles.Stammdatenverwalter];

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

    private static string BuildUrl(string bfs)
        => $"v1/api/domain-of-influences/{bfs}/logo";
}
