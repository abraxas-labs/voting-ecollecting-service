// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Domain.Authorization;

namespace Voting.ECollecting.Admin.Api.Http.Controllers;

[Stammdatenverwalter]
[ApiController]
[Route("v1/api/domain-of-influences/{bfs}")]
public class DomainOfInfluenceController : ControllerBase
{
    private readonly IDomainOfInfluenceFilesService _filesService;

    public DomainOfInfluenceController(IDomainOfInfluenceFilesService filesService)
    {
        _filesService = filesService;
    }

    [HttpGet("logo")]
    public async Task<FileResult> GetLogo(string bfs)
    {
        var logo = await _filesService.GetLogo(bfs);
        return new FileContentResult(logo.Content!.Data, logo.ContentType);
    }

    [RequestSizeLimit(3 * 1024 * 1024)] // 3MB max size
    [HttpPost("logo")]
    public Task SetLogo(string bfs, [FromForm] IFormFile logo, CancellationToken ct)
        => _filesService.UpdateLogo(
            bfs,
            logo.OpenReadStream(),
            logo.ContentType,
            logo.FileName,
            ct);
}
