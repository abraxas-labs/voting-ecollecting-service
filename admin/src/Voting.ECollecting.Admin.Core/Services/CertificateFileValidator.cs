// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Security.Cryptography.X509Certificates;
using Voting.ECollecting.Admin.Core.Configuration;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Abstractions.Core.Services;

namespace Voting.ECollecting.Admin.Core.Services;

public class CertificateFileValidator
{
    private readonly IFileService _fileService;
    private readonly CertificateValidator _certificateValidator;
    private readonly CoreAppConfig _config;

    public CertificateFileValidator(IFileService fileService, CertificateValidator certificateValidator, CoreAppConfig config)
    {
        _fileService = fileService;
        _certificateValidator = certificateValidator;
        _config = config;
    }

    public async Task<CertificateValidationSummary> ValidateBackupCertificate(
        X509Certificate2 caCert,
        Stream stream,
        string contentType,
        string fileName,
        CancellationToken ct)
    {
        // some browsers send the wrong mime-type (e.g. chrome application/x-x509-ca-cert instead of application/x-pem-file)
        // therefore we cannot really validate it since application/x-x509-ca-cert is normally binary
        // but the real content is still a pem (which is text based).
        // We use the validation of the pem deserialization instead to validate the contents.
        var file = await _fileService.Validate(
            stream,
            contentType,
            fileName,
            _config.BackupCertificate.AllowedFileExtensions,
            false,
            ct);

        return _certificateValidator.ValidateBackupCertificate(file, caCert, _config.BackupCertificate);
    }
}
