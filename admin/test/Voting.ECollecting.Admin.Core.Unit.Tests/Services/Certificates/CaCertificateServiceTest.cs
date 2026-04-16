// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging.Abstractions;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Admin.Core.Services;

namespace Voting.ECollecting.Admin.Core.Unit.Tests.Services.Certificates;

public class CaCertificateServiceTest
{
    [Fact]
    public void ValidCertificateShouldReturnCertificate()
    {
        var pemContent = File.ReadAllText(BuildCertFilePath("ca-certificate.pem"));
        using var cert = GetService(pemContent).GetCertificateAuthorityCertificate();
        Assert.Equal("9F8B797048DBDBFB592D9DBCF717A0FA2CA54407", cert.Thumbprint);
        Assert.False(cert.HasPrivateKey);
    }

    [Fact]
    public void EmptyPemShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => GetService(string.Empty).GetCertificateAuthorityCertificate());
    }

    [Fact]
    public void NullPemShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => GetService(null!).GetCertificateAuthorityCertificate());
    }

    private static string BuildCertFilePath(string name, [CallerFilePath] string? thisFilePath = null)
        => Path.Join(Path.GetDirectoryName(thisFilePath), name);

    private CaCertificateService GetService(string pem)
    {
        var config = new BackupCertificateConfig { CACertificate = pem };
        return new CaCertificateService(config, NullLogger<CaCertificateService>.Instance);
    }
}
