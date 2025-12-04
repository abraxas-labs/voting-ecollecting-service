// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Models;

public enum CertificateValidation
{
    ContainsSingleEntry,
    Format,
    CertificateNotAfter,
    CertificateSelfSigned,
    CACertificateNotAfter,
    CertificateChainValidation,
}
