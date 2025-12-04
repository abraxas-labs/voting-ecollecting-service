// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Common;

namespace Voting.ECollecting.Citizen.Abstractions.Core.Services;

public interface ICollectionPermissionService
{
    Task<List<CollectionPermissionEntity>> ListPermissions(Guid collectionId);

    Task<Guid> CreatePermission(Guid collectionId, string firstName, string lastName, string email, CollectionPermissionRole role, CancellationToken ct);

    Task DeletePermission(Guid id);

    Task ResendPermission(Guid id, CancellationToken ct);

    Task<PendingCollectionPermission> GetPendingByTokenInclCollection(UrlToken token);

    Task AcceptByToken(UrlToken token);

    Task RejectByToken(UrlToken token);
}
