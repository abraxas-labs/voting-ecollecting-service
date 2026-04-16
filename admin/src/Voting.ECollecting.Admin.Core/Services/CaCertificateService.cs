// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.Lib.Common;

namespace Voting.ECollecting.Admin.Core.Services;

public class CaCertificateService : ICaCertificateService
{
    private readonly BackupCertificateConfig _config;
    private readonly ILogger<CaCertificateService> _logger;

    public CaCertificateService(BackupCertificateConfig config, ILogger<CaCertificateService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public X509Certificate2 GetCertificateAuthorityCertificate()
    {
        ArgumentException.ThrowIfNullOrEmpty(_config.CACertificate);

        var cert = X509Certificate2.CreateFromPem(_config.CACertificate);
        if (!cert.HasPrivateKey)
        {
            return cert;
        }

        // This should never happen: CreateFromPem(string) ignores private key blocks and cannot produce a
        // certificate with HasPrivateKey == true. The guard is kept as a safety net in case the underlying
        // BCL behaviour changes or a future code path introduces a different loading method.
        cert.Dispose();
        _logger.LogCritical(SecurityLogging.SecurityEventId, "Private key in backup ca certificate detected");
        throw new InvalidOperationException("Private key in backup ca certificate detected");
    }
}
