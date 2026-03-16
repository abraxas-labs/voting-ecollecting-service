// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Mvc;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.Lib.Rest.Files;

namespace Voting.ECollecting.Admin.Api.Http.Controllers;

[ApiController]
[Route("v1/api/collections/{collectionId:guid}")]
public class CollectionController : ControllerBase
{
    private readonly ICollectionFilesService _collectionFilesService;
    private readonly ICollectionSignatureSheetService _collectionSignatureSheetService;

    public CollectionController(ICollectionFilesService collectionFilesService, ICollectionSignatureSheetService collectionSignatureSheetService)
    {
        _collectionFilesService = collectionFilesService;
        _collectionSignatureSheetService = collectionSignatureSheetService;
    }

    [HumanUser]
    [HttpGet("image")]
    public async Task<FileResult> GetImage(Guid collectionId)
    {
        var image = await _collectionFilesService.GetImage(collectionId);
        return File(image.Content!.Data, image.ContentType, image.Name);
    }

    [HumanUser]
    [HttpGet("logo")]
    public async Task<FileResult> GetLogo(Guid collectionId)
    {
        var image = await _collectionFilesService.GetLogo(collectionId);
        return File(image.Content!.Data, image.ContentType, image.Name);
    }

    [HumanUser]
    [HttpGet("signature-sheet-template")]
    public async Task<FileResult> GetSignatureSheetTemplate(Guid collectionId)
    {
        var file = await _collectionFilesService.GetSignatureSheetTemplate(collectionId);
        return File(file.Content!.Data, file.ContentType, file.Name);
    }

    [Kontrollzeichenerfasser]
    [HttpPost("signature-sheets/attest")]
    public async Task<FileResult> AttestSignatureSheets(Guid collectionId, HashSet<Guid> signatureSheetIds)
    {
        var file = await _collectionSignatureSheetService.Attest(collectionId, signatureSheetIds);
        return SingleFileResult.Create(file);
    }

    [Kontrollzeichenerfasser]
    [HttpPost("signature-sheets/reattest")]
    public async Task<FileResult> ReattestSignatureSheets(Guid collectionId, HashSet<Guid> signatureSheetIds)
    {
        var file = await _collectionSignatureSheetService.Reattest(collectionId, signatureSheetIds);
        return SingleFileResult.Create(file);
    }
}
