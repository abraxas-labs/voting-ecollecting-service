// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Common.Files;

namespace Voting.ECollecting.Admin.Abstractions.Core.Services;

public interface IInitiativeService
{
    Task<List<InitiativeSubTypeEntity>> ListSubTypes();

    Task<IReadOnlyDictionary<DomainOfInfluenceType, List<Initiative>>> ListByDoiType(IReadOnlySet<DomainOfInfluenceType>? doiTypes, string? bfs);

    Task<Initiative> Get(Guid id);

    Task FinishCorrection(Guid id);

    Task SetCollectionPeriod(Guid id, DateOnly collectionStartDate, DateOnly collectionEndDate);

    Task Enable(Guid id, DateOnly? collectionStartDate, DateOnly? collectionEndDate);

    Task CameAbout(Guid id, DateOnly sensitiveDataExpiryDate);

    Task CameNotAbout(Guid id, CollectionCameNotAboutReason reason, DateOnly sensitiveDataExpiryDate);

    Task Update(Guid id, UpdateInitiativeParams updateParams);

    IAsyncEnumerable<IFile> GetDocuments(Guid id, CancellationToken cancellationToken = default);

    Task ReturnForCorrection(Guid id, InitiativeLockedFields? lockedFields);

    Task SetSensitiveDataExpiryDate(Guid initiativeId, DateOnly date);

    Task<SecondFactorTransactionInfo> PrepareDelete(Guid initiativeId);

    Task Delete(Guid initiativeId, Guid secondFactorId, CancellationToken cancellationToken);
}
