// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.Common.Files;

namespace Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;

public interface IPdfGenerator<in T>
{
    Task<FileEntity> GenerateFile(T entity, CancellationToken cancellationToken = default);

    Task<Stream> Generate(T entity, CancellationToken cancellationToken = default);

    Task<IFile> GenerateFileModel(T entity, CancellationToken cancellationToken = default);
}
