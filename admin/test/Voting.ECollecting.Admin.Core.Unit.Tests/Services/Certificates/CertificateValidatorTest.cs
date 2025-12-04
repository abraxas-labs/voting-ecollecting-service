// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Admin.Core.Services;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Abstractions.Core.Services;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Core.Unit.Tests.Services.Certificates;

public class CertificateValidatorTest
{
    private const string PemMimeType = "application/x-pem-file";

    private readonly FakeTimeProvider _timeProvider;
    private readonly CertificateValidator _validator;
    private readonly X509Certificate2 _caCert;

    public CertificateValidatorTest()
    {
        _timeProvider = new(new DateTimeOffset(new DateTime(2025, 6, 13, 9, 0, 0)));
        _validator = new(
            new MockFileService(),
            new CoreAppConfig(),
            NullLogger<CertificateValidator>.Instance,
            _timeProvider);
        _caCert = X509Certificate2.CreateFromPem(File.ReadAllText(BuildCertFilePath("ca-certificate.pem")));
    }

    [Fact]
    public async Task ValidShouldWork()
    {
        var result = await Validate(
            "certificate.pem",
            CertificateValidationState.Ok,
            CertificateValidation.CertificateChainValidation);
        await Verify(result);
    }

    [Fact]
    public async Task InvalidFormattedError()
    {
        await Validate(
            "not-a-pem.pem",
            CertificateValidationState.Error,
            CertificateValidation.ContainsSingleEntry);
    }

    [Fact]
    public async Task SelfSignedShouldError()
    {
        await Validate(
            "ca-certificate.pem",
            CertificateValidationState.Error,
            CertificateValidation.CertificateSelfSigned);
    }

    [Fact]
    public async Task WithPrivateKeyShouldError()
    {
        await Validate(
            "unencrypted-private-key.pem",
            CertificateValidationState.Error,
            CertificateValidation.Format);
    }

    [Fact]
    public async Task WithMultipleShouldError()
    {
        await Validate(
            "certificate-invalid-multiple.pem",
            CertificateValidationState.Error,
            CertificateValidation.ContainsSingleEntry);
    }

    [Fact]
    public async Task WithOtherCAShouldError()
    {
        await Validate(
            "certificate2.pem",
            CertificateValidationState.Error,
            CertificateValidation.CertificateChainValidation);
    }

    [Fact]
    public async Task WithExpiredCertificate()
    {
        _timeProvider.Advance(TimeSpan.FromDays(366));
        await Validate(
            "certificate.pem",
            CertificateValidationState.Error,
            CertificateValidation.CertificateNotAfter);
    }

    [Fact]
    public async Task WithCertificateValidityLessThan14Days()
    {
        // cert is valid for 1 year
        // advance 1 year - 10 days to be in 14 days range within expiry
        _timeProvider.Advance(TimeSpan.FromDays(365 - 10));
        await Validate(
            "certificate.pem",
            CertificateValidationState.Warning,
            CertificateValidation.CertificateNotAfter);
    }

    [Fact]
    public async Task WithCaCertificateValidityLessThan4Months()
    {
        // ca cert is valid for 5 years
        // advance 5 years - 3 months to be within 4 months timerange
        _timeProvider.Advance(TimeSpan.FromDays((5 * 365) - (3 * 30)));
        await Validate(
            "certificate-long-validity.pem",
            CertificateValidationState.Error,
            CertificateValidation.CACertificateNotAfter);
    }

    [Fact]
    public async Task WithCaCertificateValidityLessThan6Months()
    {
        // ca cert is valid for 5 years
        // advance 5 years - 5 months to be within 6 months timerange
        _timeProvider.Advance(TimeSpan.FromDays((5 * 365) - (5 * 30)));
        await Validate(
            "certificate-long-validity.pem",
            CertificateValidationState.Warning,
            CertificateValidation.CACertificateNotAfter);
    }

    private async Task<CertificateValidationSummary> Validate(
        string name,
        CertificateValidationState state,
        CertificateValidation validation)
    {
        await using var f = OpenCert(name);
        var result = await _validator.ValidateBackupCertificate(
            _caCert,
            f,
            PemMimeType,
            "cert.pem",
            CancellationToken.None);
        result.State.Should().Be(state);
        result.Validations.Single(x => x.Validation == validation)
            .State
            .Should()
            .Be(state);
        return result;
    }

    private FileStream OpenCert(string name)
    {
        var path = BuildCertFilePath(name);
        return File.OpenRead(path);
    }

    private string BuildCertFilePath(string name, [CallerFilePath] string? thisFilePath = null)
    {
        return Path.Join(Path.GetDirectoryName(thisFilePath), name);
    }

    private class MockFileService : IFileService
    {
        public Task<FileEntity> Validate(
            Stream file,
            [NotNull] string? contentType,
            [NotNull] string? fileName,
            IReadOnlySet<string> allowedFileExtensions,
            bool validateMimeType = true,
            CancellationToken ct = default)
        {
            contentType ??= string.Empty;
            fileName ??= string.Empty;
            return Task.FromResult(new FileEntity
            {
                Content = new FileContentEntity
                {
                    Data = StreamToByteArray(file),
                },
            });
        }

        private static byte[] StreamToByteArray(Stream input)
        {
            using var ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
