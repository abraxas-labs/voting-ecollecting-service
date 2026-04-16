// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Security.Cryptography.X509Certificates;

namespace Voting.ECollecting.Admin.Abstractions.Core.Services;

public interface ICaCertificateService
{
    X509Certificate2 GetCertificateAuthorityCertificate();
}
