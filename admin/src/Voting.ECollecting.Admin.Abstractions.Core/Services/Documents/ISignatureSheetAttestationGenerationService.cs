// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Abstractions.Core.Services.Documents;

public interface ISignatureSheetAttestationGenerationService
{
    Task<Stream> GenerateFile(
        CollectionBaseEntity collection,
        AccessControlListDoiEntity aclDoi,
        ICollection<CollectionSignatureSheetEntity> signatureSheets);
}
