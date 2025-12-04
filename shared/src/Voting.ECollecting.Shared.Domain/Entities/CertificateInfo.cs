// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Voting.ECollecting.Shared.Domain.Entities;

public record CertificateInfo(
    DateTime NotBefore,
    DateTime NotAfter,
    string Subject)
{
    public CertificateInfo(X509Certificate2 cert)
        : this(cert.NotBefore.ToUniversalTime(), cert.NotAfter.ToUniversalTime(), cert.SubjectName.Format(true))
    {
        // store thumbprint as SHA-256 instead of default SHA-1
        var hash = SHA256.HashData(cert.RawData);
        Thumbprint = BitConverter.ToString(hash).Replace("-", string.Empty).ToUpperInvariant();
    }

    public string Thumbprint { get; init; } = string.Empty;
}
