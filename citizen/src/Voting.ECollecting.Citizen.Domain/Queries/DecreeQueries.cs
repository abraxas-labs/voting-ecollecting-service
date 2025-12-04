// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Domain.Queries;

public static class DecreeQueries
{
    public static IQueryable<DecreeEntity> IncludeReadableCollections(this IQueryable<DecreeEntity> q, string userId)
    {
        return q.Include(x =>
            x.Collections.Where(c => c.AuditInfo.CreatedById == userId
                                     || c.Permissions!.Any(p =>
                                         p.State == CollectionPermissionState.Accepted
                                         && p.IamUserId == userId
                                         && (p.Role == CollectionPermissionRole.Deputy || p.Role == CollectionPermissionRole.Reader)))
                .OrderBy(c => c.AuditInfo.CreatedAt));
    }

    public static IQueryable<DecreeEntity> IncludeInCollectionAndReadableCollections(this IQueryable<DecreeEntity> q, string userId)
    {
        return q.Include(x =>
            x.Collections.Where(c => c.State == CollectionState.EnabledForCollection
                                     || c.State == CollectionState.SignatureSheetsSubmitted
                                     || c.AuditInfo.CreatedById == userId
                                     || c.Permissions!.Any(p =>
                                         p.State == CollectionPermissionState.Accepted
                                         && p.IamUserId == userId
                                         && (p.Role == CollectionPermissionRole.Deputy || p.Role == CollectionPermissionRole.Reader)))
                .OrderBy(c => c.AuditInfo.CreatedAt));
    }
}
