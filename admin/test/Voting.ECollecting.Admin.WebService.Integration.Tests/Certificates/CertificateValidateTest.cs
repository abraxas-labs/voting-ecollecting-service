// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Time.Testing;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.Certificates;

public class CertificateValidateTest : BaseRestTest
{
    private const string Url = "v1/api/certificates/backup/validate";

    public CertificateValidateTest(TestApplicationFactory factory)
        : base(factory)
    {
        // use time when test cert is valid
        // adjustTime is already stable in .NET 10
#pragma warning disable EXTEXP0004
        GetService<FakeTimeProvider>().AdjustTime(new DateTimeOffset(new DateTime(2026, 01, 16)));
#pragma warning restore EXTEXP0004
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Certificates);
    }

    [Fact]
    public async Task ShouldWork()
    {
        var content = BuildSimpleContent();
        var result = await CtSgZertifikatsverwalterClient.PostAsync(Url, content);
        result.EnsureSuccessStatusCode();
        await VerifyJson(await result.Content.ReadAsStringAsync())
            .DontScrubDateTimes();
    }

    [Fact]
    public async Task ShouldReturnInvalidExpired()
    {
        // other validations are unit-tested.
        GetService<FakeTimeProvider>().Advance(TimeSpan.FromDays(3 * 365));
        var content = BuildSimpleContent();
        var result = await CtSgZertifikatsverwalterClient.PostAsync(Url, content);
        result.EnsureSuccessStatusCode();
        await VerifyJson(await result.Content.ReadAsStringAsync())
            .DontScrubDateTimes();
    }

    [Fact]
    public async Task ShouldThrowAsMu()
    {
        var content = BuildSimpleContent();
        await AssertStatus(
            async () => await MuSgZertifikatsverwalterClient.PostAsync(Url, content),
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ShouldThrowWrongFileExtension()
    {
        var content = BuildSimpleContent(fileName: "foo.bar");
        await AssertStatus(
            async () => await CtSgZertifikatsverwalterClient.PostAsync(Url, content),
            HttpStatusCode.BadRequest,
            nameof(ValidationException),
            "File extension bar is not allowed for uploads");
    }

    protected override async Task<HttpResponseMessage> AuthorizationTestCall(HttpClient httpClient)
    {
        using var content = BuildSimpleContent();
        return await httpClient.PostAsync(Url, content);
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Zertifikatsverwalter;
    }

    private static MultipartFormDataContent BuildSimpleContent(
        string? contentType = null,
        string? fileName = null,
        byte[]? content = null)
    {
        var fileContent = new ByteArrayContent(content ?? Files.BackupCertificatePem);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType ?? "application/x-pem-file");

        var data = new MultipartFormDataContent();
        data.Add(fileContent, "file", fileName ?? Files.BackupCertificateName);
        return data;
    }
}
