// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Abstractions.Core.Services;

public interface ICollectionService
{
    Task<Dictionary<DomainOfInfluenceType, CollectionsGroup>> ListByDoiType(CollectionPeriodState periodState, IReadOnlySet<DomainOfInfluenceType>? doiTypes, string? bfs);

    Task<(List<CollectionMessageEntity> Messages, bool InformalReviewRequested)> ListMessages(Guid collectionId);

    Task<CollectionMessageEntity> AddMessage(Guid collectionId, string content);

    Task<CollectionMessageEntity> UpdateRequestInformalReview(Guid id, bool requestInformalReview);

    Task Withdraw(Guid id);

    Task<ValidationSummary> Validate(Guid id);
}
