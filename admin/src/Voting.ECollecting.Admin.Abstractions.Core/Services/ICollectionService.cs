// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Admin.Abstractions.Core.Services;

public interface ICollectionService
{
    Task<(List<CollectionMessageEntity> Messages, bool InformalReviewRequested)> ListMessages(Guid collectionId);

    Task<CollectionMessageEntity> AddMessage(Guid collectionId, string content);

    Task NotifyPreparingForCollection();

    Task<IReadOnlyDictionary<DomainOfInfluenceType, CollectionsGroup>> ListForDeletionByDoiType(
        IReadOnlySet<DomainOfInfluenceType>? doiTypes,
        string? bfs,
        CollectionControlSignFilter filter);

    Task DeleteWithdrawn(Guid collectionId);

    Task<CollectionMessageEntity> FinishInformalReview(Guid collectionId);

    Task<List<CollectionPermission>> ListPermissions(Guid collectionId);

    Task<CollectionUserPermissions> SubmitSignatureSheets(Guid collectionId);

    Task SetCommitteeAddress(Guid collectionId, CollectionAddress address);
}
