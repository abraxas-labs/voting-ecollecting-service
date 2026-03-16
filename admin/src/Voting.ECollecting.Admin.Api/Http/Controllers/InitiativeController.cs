// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Mvc;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.Lib.Rest.Files;

namespace Voting.ECollecting.Admin.Api.Http.Controllers;

[Stammdatenverwalter]
[ApiController]
[Route("v1/api/initiatives/{initiativeId:guid}")]
public class InitiativeController : ControllerBase
{
    private readonly IInitiativeCommitteeService _initiativeCommitteeService;
    private readonly IInitiativeService _initiativeService;
    private readonly TimeProvider _timeProvider;

    public InitiativeController(
        IInitiativeCommitteeService initiativeCommitteeService,
        IInitiativeService initiativeService,
        TimeProvider timeProvider)
    {
        _initiativeCommitteeService = initiativeCommitteeService;
        _initiativeService = initiativeService;
        _timeProvider = timeProvider;
    }

    [HttpGet("committee-lists/{fileId:guid}")]
    public async Task<FileResult> GetCommitteeList(
        Guid initiativeId,
        Guid fileId)
    {
        var file = await _initiativeCommitteeService.GetCommitteeList(initiativeId, fileId);
        return File(file.Content!.Data, file.ContentType, file.Name);
    }

    [HttpGet("documents")]
    public FileResult GetDocuments(Guid initiativeId, CancellationToken ct)
    {
        var files = _initiativeService.GetDocuments(initiativeId, ct);
        return SingleFileResult.CreateZipFile(files, "export.zip", _timeProvider.GetSwissDateTime(), ct);
    }
}
