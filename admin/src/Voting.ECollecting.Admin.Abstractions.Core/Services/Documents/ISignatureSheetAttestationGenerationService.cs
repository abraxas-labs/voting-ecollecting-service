// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.Lib.Common.Files;

namespace Voting.ECollecting.Admin.Abstractions.Core.Services.Documents;

public interface ISignatureSheetAttestationGenerationService
{
    Task<IFile> GenerateFile(
        CollectionBaseEntity collection,
        DomainOfInfluenceEntity domainOfInfluence,
        ICollection<CollectionSignatureSheetEntity> signatureSheets);
}
