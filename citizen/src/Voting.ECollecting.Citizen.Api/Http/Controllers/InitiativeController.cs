// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voting.ECollecting.Citizen.Abstractions.Core.Services;
using Voting.ECollecting.Citizen.Api.Http.Responses;

namespace Voting.ECollecting.Citizen.Api.Http.Controllers;

[ApiController]
[Route("v1/api/initiatives/{initiativeId:guid}")]
public class InitiativeController : ControllerBase
{
    private readonly IInitiativeCommitteeListService _initiativeCommitteeListService;

    public InitiativeController(IInitiativeCommitteeListService initiativeCommitteeListService)
    {
        _initiativeCommitteeListService = initiativeCommitteeListService;
    }

    [RequestSizeLimit(5 * 1024 * 1024)] // 5MB max size
    [HttpPost("committee-lists")]
    public async Task<AddCommitteeListResponse> AddCommitteeList(Guid initiativeId, [FromForm] IFormFile file, CancellationToken ct)
    {
        var fileEntity = await _initiativeCommitteeListService.AddCommitteeList(
            initiativeId,
            file.OpenReadStream(),
            file.ContentType,
            file.FileName,
            ct);
        return new AddCommitteeListResponse(fileEntity.Id, fileEntity.Name);
    }

    [HttpGet("committee-lists/template")]
    public async Task<FileResult> GetCommitteeListTemplate(Guid initiativeId, CancellationToken ct)
    {
        var file = await _initiativeCommitteeListService.GetCommitteeListTemplate(initiativeId, ct);
        return new FileStreamResult(file, "application/pdf");
    }

    [AllowAnonymous]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5MB max size
    [HttpPost("committee-members/accept")]
    public async Task AcceptCommitteeMembershipWithCommitteeList(
        Guid initiativeId,
        [FromForm] string token,
        [FromForm] IFormFile file,
        CancellationToken ct)
    {
        await _initiativeCommitteeListService.AcceptCommitteeMembershipWithCommitteeList(
            initiativeId,
            token,
            file.OpenReadStream(),
            file.ContentType,
            file.FileName,
            ct);
    }

    [AllowAnonymous]
    [HttpPost("committee-members/template")]
    public async Task<FileResult> GetCommitteeListTemplateByToken(
        Guid initiativeId,
        [FromForm] string token,
        CancellationToken ct)
    {
        var file = await _initiativeCommitteeListService.GetCommitteeListTemplateForMemberByToken(initiativeId, token, ct);
        return new FileStreamResult(file, "application/pdf");
    }

    [HttpGet("committee-lists/{fileId:guid}")]
    public async Task<FileResult> GetCommitteeList(
        Guid initiativeId,
        Guid fileId)
    {
        var file = await _initiativeCommitteeListService.GetCommitteeList(initiativeId, fileId);
        return new FileContentResult(file.Content!.Data, file.ContentType);
    }
}
