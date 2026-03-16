// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Common.Files;

namespace Voting.ECollecting.Citizen.Abstractions.Core.Services;

public interface ICollectionFilesService
{
    Task UpdateImage(Guid id, Stream image, string? contentType, string? fileName, CancellationToken ct);

    Task DeleteImage(Guid collectionId);

    Task<FileEntity> GetImage(Guid id);

    Task UpdateLogo(Guid id, Stream logo, string? contentType, string? fileName, CancellationToken ct);

    Task DeleteLogo(Guid collectionId);

    Task<FileEntity> GetLogo(Guid id);

    Task UpdateSignatureSheetTemplate(Guid id, Stream file, string? contentType, string? fileName, CancellationToken ct);

    Task<FileEntity> GetSignatureSheetTemplate(Guid id, bool requireEnabledForSubmission);

    Task<FileEntity> SetSignatureSheetTemplateGenerated(Guid id, CollectionType collectionType);

    Task DeleteSignatureSheetTemplate(Guid id);

    Task<FileEntity> GenerateSignatureSheetTemplatePreview(Guid id, CollectionType collectionType);

    Task<IFile> GetElectronicSignaturesProtocol(Guid collectionId, CancellationToken cancellationToken);
}
