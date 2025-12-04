// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Diagnostics.CodeAnalysis;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Abstractions.Core.Services;

public interface IFileService
{
    Task<FileEntity> Validate(
        Stream file,
        [NotNull] string? contentType,
        [NotNull] string? fileName,
        IReadOnlySet<string> allowedFileExtensions,
        bool validateMimeType = true,
        CancellationToken ct = default);
}
