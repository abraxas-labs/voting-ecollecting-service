// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Common.Files;

namespace Voting.ECollecting.Admin.Abstractions.Core.Services;

public interface IDecreeService
{
    Task<Decree> Create(Decree decree);

    Task<List<Decree>> List();

    Task Update(Decree decree);

    Task DeletePublished(Guid id);

    Task SetSensitiveDataExpiryDate(Guid id, DateOnly date);

    Task<SecondFactorTransactionInfo> PrepareDelete(Guid id);

    Task Delete(Guid id, Guid secondFactorId, CancellationToken cancellationToken);

    Task<Decree> GetForEdit(Guid id);

    Task<DeleteDecreeInfo> GetForDelete(Guid id);

    Task CameAbout(Guid id, DateOnly sensitiveDataExpiryDate);

    Task CameNotAbout(Guid id, CollectionCameNotAboutReason reason, DateOnly sensitiveDataExpiryDate);

    IAsyncEnumerable<IFile> GetDocuments(Guid id, CancellationToken cancellationToken = default);
}
