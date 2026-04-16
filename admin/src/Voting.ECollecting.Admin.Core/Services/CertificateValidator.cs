// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Extensions.Logging;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.Common;

namespace Voting.ECollecting.Admin.Core.Services;

public class CertificateValidator
{
    private readonly ILogger<CertificateValidator> _logger;
    private readonly TimeProvider _timeProvider;

    public CertificateValidator(ILogger<CertificateValidator> logger, TimeProvider timeProvider)
    {
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public CertificateValidationSummary ValidateBackupCertificate(
        FileEntity certificate,
        X509Certificate2 caCert,
        BackupCertificateConfig config)
    {
        var content = Encoding.ASCII.GetString(certificate.Content!.Data);
        var hasSingleEntry = HasSingleEntry(content);

        List<CertificateValidationResult> validations = [new(CertificateValidation.ContainsSingleEntry, hasSingleEntry)];
        if (!hasSingleEntry)
        {
            return new CertificateValidationSummary(null, null, null, validations, CertificateValidationState.Error);
        }

        if (!TryReadCertificate(content, out var cert))
        {
            validations.Add(new(CertificateValidation.Format, false));
            return new CertificateValidationSummary(null, null, null, validations, CertificateValidationState.Error);
        }

        validations.Add(new(CertificateValidation.CertificateSelfSigned, !cert.Subject.Equals(cert.Issuer, StringComparison.Ordinal)));
        validations.Add(new(CertificateValidation.CertificateNotAfter, ValidateNotAfter(
            cert,
            config.NotAfterGracePeriod)));
        validations.Add(new(CertificateValidation.CACertificateNotAfter, ValidateNotAfter(
            caCert,
            config.CACertificateNotAfterGracePeriod,
            config.CACertificateNotAfterValidityPeriod)));
        validations.Add(BuildChainValidation(cert, caCert));

        var state = validations.Max(v => v.State);
        switch (state)
        {
            case CertificateValidationState.Warning:
                _logger.LogInformation(SecurityLogging.SecurityEventId, "Certificate validation completed with warnings");
                break;
            case CertificateValidationState.Error:
                _logger.LogInformation(SecurityLogging.SecurityEventId, "Certificate validation failed");
                break;
        }

        var info = new CertificateInfo(cert);
        var caInfo = new CertificateInfo(caCert);
        return new CertificateValidationSummary(certificate, info, caInfo, validations, state);
    }

    private bool TryReadCertificate(string content, [NotNullWhen(true)] out X509Certificate2? cert)
    {
        try
        {
            cert = X509Certificate2.CreateFromPem(content);
            return true;
        }
        catch (Exception e)
        {
            cert = null;
            _logger.LogInformation(SecurityLogging.SecurityEventId, e, "Certificate deserialization failed");
            return false;
        }
    }

    private CertificateValidationResult BuildChainValidation(X509Certificate2 cert, X509Certificate2 caCert)
    {
        var chain = BuildChain(caCert);
        if (chain.Build(cert))
        {
            return new CertificateValidationResult(CertificateValidation.CertificateChainValidation, true);
        }

        var statusBuilder = new StringBuilder();
        foreach (var status in chain.ChainStatus)
        {
            statusBuilder.AppendLine($"{status.Status}: {status.StatusInformation}");
        }

        _logger.LogInformation(SecurityLogging.SecurityEventId, "Certificate chain validation failed: {Status}", statusBuilder.ToString());
        return new CertificateValidationResult(CertificateValidation.CertificateChainValidation, false);
    }

    private CertificateValidationState ValidateNotAfter(
        X509Certificate2 cert,
        TimeSpan gracePeriod,
        TimeSpan? minValidityPeriod = null)
    {
        var now = _timeProvider.GetUtcNowDateTime();
        if (cert.NotAfter <= now.Add(minValidityPeriod ?? TimeSpan.Zero))
        {
            return CertificateValidationState.Error;
        }

        if (cert.NotAfter <= now.Add(gracePeriod))
        {
            return CertificateValidationState.Warning;
        }

        return CertificateValidationState.Ok;
    }

    private bool HasSingleEntry(ReadOnlySpan<char> content)
    {
        if (!PemEncoding.TryFind(content, out var pemInfo))
        {
            return false;
        }

        return !PemEncoding.TryFind(content[pemInfo.Location.End..], out _);
    }

    private X509Chain BuildChain(X509Certificate2 caCert)
    {
        var chain = new X509Chain();

        // the certificates provided by the authorities don't have OCSP/CRL set,
        // therefore we can't check revocation.
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        chain.ChainPolicy.VerificationTime = _timeProvider.GetUtcNowDateTime();
        chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        chain.ChainPolicy.CustomTrustStore.Add(caCert);
        return chain;
    }
}
