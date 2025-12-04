// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Api.Http.Mappings;
using Voting.ECollecting.Admin.Api.Http.Responses;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.Lib.RestValidation;

namespace Voting.ECollecting.Admin.Api.Http.Controllers;

[ApiController]
[Route("v1/api/certificates")]
[Zertifikatsverwalter]
public class CertificatesController
{
    private readonly ICertificateService _certificateService;

    public CertificatesController(ICertificateService certificateService)
    {
        _certificateService = certificateService;
    }

    [RequestSizeLimit(5 * 1024 * 1024)] // 5MB max size
    [HttpPost("backup/validate")]
    public async Task<CertificateValidationSummaryResponse> ValidateBackupCertificate([FromForm] IFormFile file, CancellationToken ct)
    {
        var result = await _certificateService.ValidateBackupCertificate(
            file.OpenReadStream(),
            file.ContentType,
            file.FileName,
            ct);
        return ResponseMapper.Map(result);
    }

    [RequestSizeLimit(5 * 1024 * 1024)] // 5MB max size
    [HttpPost("backup")]
    public async Task SetBackupCertificate(
        [FromForm, ComplexSlText, MaxLength(100)] string? label,
        [FromForm] IFormFile file,
        CancellationToken ct)
    {
        await _certificateService.SetBackupCertificate(
            label,
            file.OpenReadStream(),
            file.ContentType,
            file.FileName,
            ct);
    }
}
