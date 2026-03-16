// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Abstractions.Core.Services;

public interface ICollectionFilesService
{
    // returns the signature sheet template if 'SignatureSheetTemplateGenerated' is true
    Task<FileEntity?> DeleteImage(Guid collectionId);

    Task<FileEntity> GetImage(Guid collectionId);

    // returns the signature sheet template if 'SignatureSheetTemplateGenerated' is true
    Task<FileEntity?> DeleteLogo(Guid collectionId);

    Task<FileEntity> GetLogo(Guid collectionId);

    Task<FileEntity> GetSignatureSheetTemplate(Guid collectionId);

    // returns the signature sheet template
    Task<FileEntity> DeleteSignatureSheetTemplate(Guid collectionId);
}
