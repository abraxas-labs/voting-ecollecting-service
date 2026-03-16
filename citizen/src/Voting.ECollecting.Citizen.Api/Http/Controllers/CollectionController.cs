// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Voting.ECollecting.Citizen.Abstractions.Core.Services;
using Voting.Lib.Rest.Files;

namespace Voting.ECollecting.Citizen.Api.Http.Controllers;

[ApiController]
[Route("v1/api/collections/{collectionId:guid}")]
public class CollectionController : ControllerBase
{
    private readonly ICollectionFilesService _collectionFilesService;

    public CollectionController(ICollectionFilesService collectionFilesService)
    {
        _collectionFilesService = collectionFilesService;
    }

    [RequestSizeLimit(3 * 1024 * 1024)] // 3MB max size
    [HttpPost("image")]
    public Task SetImage(Guid collectionId, [FromForm] IFormFile image, CancellationToken ct)
        => _collectionFilesService.UpdateImage(
            collectionId,
            image.OpenReadStream(),
            image.ContentType,
            image.FileName,
            ct);

    [HttpGet("image")]
    public async Task<FileResult> GetImage(Guid collectionId)
    {
        var image = await _collectionFilesService.GetImage(collectionId);
        return File(image.Content!.Data, image.ContentType, image.Name);
    }

    [RequestSizeLimit(3 * 1024 * 1024)] // 3MB max size
    [HttpPost("logo")]
    public Task SetLogo(Guid collectionId, [FromForm] IFormFile logo, CancellationToken ct)
        => _collectionFilesService.UpdateLogo(
            collectionId,
            logo.OpenReadStream(),
            logo.ContentType,
            logo.FileName,
            ct);

    [HttpGet("logo")]
    public async Task<FileResult> GetLogo(Guid collectionId)
    {
        var image = await _collectionFilesService.GetLogo(collectionId);
        return File(image.Content!.Data, image.ContentType, image.Name);
    }

    [RequestSizeLimit(5 * 1024 * 1024)] // 5MB max size
    [HttpPost("signature-sheet-template")]
    public Task SetSignatureSheetTemplate(Guid collectionId, [FromForm] IFormFile file, CancellationToken ct)
        => _collectionFilesService.UpdateSignatureSheetTemplate(
            collectionId,
            file.OpenReadStream(),
            file.ContentType,
            file.FileName,
            ct);

    [HttpGet("signature-sheet-template/preview")]
    public async Task<FileResult> GetSignatureSheetTemplatePreview(Guid collectionId)
    {
        var image = await _collectionFilesService.GetSignatureSheetTemplate(collectionId, false);
        return File(image.Content!.Data, image.ContentType, image.Name);
    }

    [AllowAnonymous]
    [HttpGet("signature-sheet-template")]
    public async Task<FileResult> GetSignatureSheetTemplate(Guid collectionId)
    {
        var image = await _collectionFilesService.GetSignatureSheetTemplate(collectionId, true);
        return File(image.Content!.Data, image.ContentType, image.Name);
    }

    [HttpGet("electronic-signatures-protocol")]
    public async Task<FileResult> GetElectronicSignaturesProtocol(Guid collectionId, CancellationToken cancellationToken)
    {
        var file = await _collectionFilesService.GetElectronicSignaturesProtocol(collectionId, cancellationToken);
        return SingleFileResult.Create(file, cancellationToken);
    }
}
