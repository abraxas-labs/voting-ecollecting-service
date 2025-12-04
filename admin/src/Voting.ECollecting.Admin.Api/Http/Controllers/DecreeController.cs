// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Mvc;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.Lib.Rest.Files;

namespace Voting.ECollecting.Admin.Api.Http.Controllers;

[Stammdatenverwalter]
[ApiController]
[Route("v1/api/decrees/{decreeId:guid}")]
public class DecreeController : ControllerBase
{
    private readonly IDecreeService _decreeService;
    private readonly TimeProvider _timeProvider;

    public DecreeController(IDecreeService decreeService, TimeProvider timeProvider)
    {
        _decreeService = decreeService;
        _timeProvider = timeProvider;
    }

    [HttpGet("documents")]
    public FileResult GetDocuments(Guid decreeId, CancellationToken ct)
    {
        var files = _decreeService.GetDocuments(decreeId, ct);
        return SingleFileResult.CreateZipFile(files, "export.zip", _timeProvider.GetSwissDateTime(), ct);
    }
}
