// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Text;
using Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.Common.Files;
using Voting.Lib.DmDoc.Serialization;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.Mocks;

public abstract class PdfGeneratorMock<TEntity, TTemplateBag> : IPdfGenerator<TEntity>
{
    private readonly IDmDocDataSerializer _dmDocDataSerializer;

    protected PdfGeneratorMock(IDmDocDataSerializer dmDocDataSerializer)
    {
        _dmDocDataSerializer = dmDocDataSerializer;
    }

    public Task<FileEntity> GenerateFile(TEntity entity, CancellationToken cancellationToken = default)
        => Task.FromResult(JsonFile(entity));

    public Task<Stream> Generate(TEntity entity, CancellationToken cancellationToken = default)
        => Task.FromResult<Stream>(Json(entity));

    public Task<IFile> GenerateFileModel(TEntity entity, CancellationToken cancellationToken = default)
        => Task.FromResult<IFile>(new StreamFile(Json(entity), BuildFileName(entity), "application/json"));

    protected abstract TTemplateBag Map(TEntity entity);

    protected abstract string BuildFileName(TEntity entity);

    private MemoryStream Json(TEntity entity)
    {
        var bag = Map(entity);
        var content = Encoding.UTF8.GetBytes(_dmDocDataSerializer.Serialize(bag));
        return new MemoryStream(content);
    }

    private FileEntity JsonFile(TEntity entity)
    {
        return new FileEntity
        {
            Content = new FileContentEntity
            {
                Data = Json(entity).ToArray(),
            },
            ContentType = "application/json",
            Name = BuildFileName(entity),
        };
    }
}
