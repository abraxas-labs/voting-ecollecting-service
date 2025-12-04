// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.IO;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.Common.Files;
using Voting.Lib.DmDoc;

namespace Voting.ECollecting.Shared.Core.Services.Documents;

public abstract class PdfGenerator<TEntity, TTemplateBag> : IPdfGenerator<TEntity>
{
    private readonly IDmDocService _dmDoc;
    private readonly string _templateKey;
    private readonly RecyclableMemoryStreamManager _memoryStreamManager;

    protected PdfGenerator(
        string templateKey,
        RecyclableMemoryStreamManager memoryStreamManager,
        IDmDocService dmDoc)
    {
        _templateKey = templateKey;
        _memoryStreamManager = memoryStreamManager;
        _dmDoc = dmDoc;
    }

    public async Task<Stream> Generate(TEntity entity, CancellationToken cancellationToken = default)
    {
        var templateBag = Map(entity);
        return await _dmDoc.FinishAsPdf(_templateKey, templateBag, ct: cancellationToken);
    }

    public async Task<FileEntity> GenerateFile(TEntity entity, CancellationToken cancellationToken = default)
    {
        var data = await Generate(entity, cancellationToken);
        return await ReadToFile(entity, data, cancellationToken);
    }

    public async Task<IFile> GenerateFileModel(TEntity entity, CancellationToken cancellationToken = default)
    {
        var data = await Generate(entity, cancellationToken);
        return new StreamFile(data, BuildFileName(entity), "application/pdf");
    }

    protected abstract TTemplateBag Map(TEntity entity);

    protected abstract string BuildFileName(TEntity entity);

    private async Task<FileEntity> ReadToFile(TEntity entity, Stream data, CancellationToken cancellationToken)
    {
        await using var ms = _memoryStreamManager.GetStream();
        await data.CopyToAsync(ms, cancellationToken);
        var content = ms.ToArray();
        return new FileEntity
        {
            Content = new FileContentEntity { Data = content },
            ContentType = "application/pdf",
            Name = BuildFileName(entity),
        };
    }
}
