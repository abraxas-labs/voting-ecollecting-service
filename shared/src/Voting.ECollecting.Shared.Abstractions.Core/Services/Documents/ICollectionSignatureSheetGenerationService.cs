// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Shared.Abstractions.Core.Services.Documents;

public interface ICollectionSignatureSheetGenerationService
{
    Task<FileEntity> GenerateSignatureSheetFile(Guid collectionId, CollectionType collectionType);
}
