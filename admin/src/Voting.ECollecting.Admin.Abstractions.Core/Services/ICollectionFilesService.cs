// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Abstractions.Core.Services;

public interface ICollectionFilesService
{
    Task DeleteImage(Guid collectionId);

    Task<FileEntity> GetImage(Guid collectionId);

    Task DeleteLogo(Guid collectionId);

    Task<FileEntity> GetLogo(Guid collectionId);

    Task<FileEntity> GetSignatureSheetTemplate(Guid collectionId);

    Task DeleteSignatureSheetTemplate(Guid collectionId);
}
