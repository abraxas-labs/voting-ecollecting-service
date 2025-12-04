// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.ECollecting.Admin.Domain.Models;

public record CertificateValidationResult(CertificateValidation Validation, CertificateValidationState State)
{
    public CertificateValidationResult(CertificateValidation validation, bool ok)
        : this(validation, ok ? CertificateValidationState.Ok : CertificateValidationState.Error)
    {
    }
}
