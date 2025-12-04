// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Abstractions.Core.Services;

public interface IDomainOfInfluenceFilesService
{
    Task DeleteLogo(string bfs);

    Task UpdateLogo(string bfs, Stream logo, string? contentType, string? fileName, CancellationToken ct);

    Task<FileEntity> GetLogo(string bfs);
}
