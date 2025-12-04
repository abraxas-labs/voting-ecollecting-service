// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Mvc;
using Voting.ECollecting.Admin.Abstractions.Core.Services;
using Voting.ECollecting.Admin.Domain.Authorization;

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
        return new FileContentResult(image.Content!.Data, image.ContentType);
    }

    [HumanUser]
    [HttpGet("logo")]
    public async Task<FileResult> GetLogo(Guid collectionId)
    {
        var image = await _collectionFilesService.GetLogo(collectionId);
        return new FileContentResult(image.Content!.Data, image.ContentType);
    }

    [HumanUser]
    [HttpGet("signature-sheet-template")]
    public async Task<FileResult> GetSignatureSheetTemplate(Guid collectionId)
    {
        var file = await _collectionFilesService.GetSignatureSheetTemplate(collectionId);
        return new FileContentResult(file.Content!.Data, file.ContentType);
    }

    [Kontrollzeichenerfasser]
    [HttpPost("signature-sheets/attest")]
    public async Task<FileResult> AttestSignatureSheet(Guid collectionId, HashSet<Guid> signatureSheetIds)
    {
        var file = await _collectionSignatureSheetService.Attest(collectionId, signatureSheetIds);
        return new FileStreamResult(file, "application/pdf");
    }
}
