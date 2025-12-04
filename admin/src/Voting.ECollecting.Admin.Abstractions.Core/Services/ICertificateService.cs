// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Abstractions.Core.Services;

public interface ICertificateService
{
    Task<ActiveCertificate> GetActive();

    Task<IReadOnlyList<CertificateEntity>> List();

    Task<CertificateValidationSummary> ValidateBackupCertificate(Stream stream, string contentType, string fileName, CancellationToken ct);

    Task SetBackupCertificate(
        string? label,
        Stream stream,
        string contentType,
        string fileName,
        CancellationToken ct);
}
