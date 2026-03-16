// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.Lib.Testing.Utils;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.Certificates;

public class CertificateSetBackupCertificateTest : BaseRestTest
{
    private const string Url = "v1/api/certificates/backup";

    public CertificateSetBackupCertificateTest(TestApplicationFactory factory)
        : base(factory)
    {
        // use time when test cert is valid
        // adjustTime is already stable in .NET 10
#pragma warning disable EXTEXP0004
        GetService<FakeTimeProvider>().AdjustTime(new DateTimeOffset(new DateTime(2025, 06, 12)));
#pragma warning restore EXTEXP0004
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        // ensure there is already an active certificate to test replacement logic.
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Certificates);
    }

    [Fact]
    public async Task ShouldWork()
    {
        var content = BuildSimpleContent();
        var result = await CtSgZertifikatsverwalterClient.PostAsync(Url, content);
        result.EnsureSuccessStatusCode();

        // active should be replaced.
        var cert = await RunOnDb(db => db.Certificates.Include(x => x.Content!.Content).SingleAsync(x => x.Active));
        await Verify(new
        {
            Certificate = cert,
            Data = Encoding.ASCII.GetString(cert.Content!.Content!.Data),
        });
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            var content = BuildSimpleContent();
            await CtSgZertifikatsverwalterClient.PostAsync(Url, content);
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldWorkWithLabel()
    {
        var content = BuildSimpleContent(label: "foo bar baz");
        var result = await CtSgZertifikatsverwalterClient.PostAsync(Url, content);
        result.EnsureSuccessStatusCode();

        // active should be replaced.
        var cert = await RunOnDb(db => db.Certificates.Include(x => x.Content!.Content).SingleAsync(x => x.Active));
        cert.Label.Should().Be("foo bar baz");
    }

    [Fact]
    public async Task ShouldWorkWithOtherMimeType()
    {
        // some browsers send the wrong mime-type (e.g. chrome application/x-x509-ca-cert instead of application/x-pem-file)
        var content = BuildSimpleContent("application/x-x509-ca-cert");
        var result = await CtSgZertifikatsverwalterClient.PostAsync(Url, content);
        result.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ShouldThrowExpired()
    {
        // other validations are unit-tested.
        GetService<FakeTimeProvider>().Advance(TimeSpan.FromDays(3 * 365));
        var content = BuildSimpleContent();
        await AssertStatus(
            async () => await CtSgZertifikatsverwalterClient.PostAsync(Url, content),
            HttpStatusCode.BadRequest,
            nameof(ValidationException),
            "Certificate validation error");
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

    [Fact]
    public async Task ShouldThrowInvalidLabel()
    {
        var content = BuildSimpleContent(label: RandomStringUtil.GenerateComplexSingleLineText(101));
        await AssertStatus(
            async () => await CtSgZertifikatsverwalterClient.PostAsync(Url, content),
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldThrowAsMu()
    {
        var content = BuildSimpleContent();
        await AssertStatus(
            async () => await MuSgZertifikatsverwalterClient.PostAsync(Url, content),
            HttpStatusCode.Forbidden);
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
        byte[]? content = null,
        string? label = null)
    {
        var fileContent = new ByteArrayContent(content ?? Files.BackupCertificatePem);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType ?? "application/x-pem-file");

        var data = new MultipartFormDataContent();
        data.Add(fileContent, "file", fileName ?? Files.BackupCertificateName);

        if (label != null)
        {
            data.Add(new StringContent(label), "label");
        }

        return data;
    }
}
