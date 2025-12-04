// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Diagnostics.CodeAnalysis;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Queries;

namespace Voting.ECollecting.Admin.Core.Permissions;

[SuppressMessage(
    "StyleCop.CSharp.OrderingRules",
    "SA1202:Elements should be ordered by access",
    Justification = "To make it easier to compare permission related methods, keep them close to each other.")]
internal static class DecreePermissions
{
    public static IQueryable<DecreeEntity> WhereCanRead(this IQueryable<DecreeEntity> query, IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfsOrChildren(permissionService);
    }

    public static IQueryable<DecreeEntity> WhereCanReadOnReferendums(this IQueryable<DecreeEntity> query, IPermissionService permissionService)
    {
        return query
            .WhereCanAccessOwnBfsOrChildrenOrParents(permissionService);
    }

    public static IQueryable<DecreeEntity> WhereCanEdit(this IQueryable<DecreeEntity> query, IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfs(permissionService)
            .WhereInPeriodState(CollectionPeriodState.Published, permissionService.Now);
    }

    private static bool CanEdit(IPermissionService permissionService, DecreeEntity decree)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stammdatenverwalter)
               && AclPermissions.CanAccessOwnBfs(permissionService, decree)
               && decree.PeriodState == CollectionPeriodState.Published;
    }

    public static IQueryable<DecreeEntity> WhereCanFinish(
        this IQueryable<DecreeEntity> query,
        IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfs(permissionService)
            .WhereInCollectionOrExpired(permissionService.Now)
            .WhereInState(DecreeState.CollectionApplicable);
    }

    private static bool CanFinish(IPermissionService permissionService, DecreeEntity decree)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stammdatenverwalter)
               && AclPermissions.CanAccessOwnBfs(permissionService, decree)
               && decree.PeriodState is CollectionPeriodState.InCollection or CollectionPeriodState.Expired
               && decree.State is DecreeState.CollectionApplicable;
    }

    public static IQueryable<DecreeEntity> WhereCanGenerateDocuments(
        this IQueryable<DecreeEntity> query,
        IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfs(permissionService)
            .WhereInCollectionOrExpired(permissionService.Now)
            .Where(x => x.State == DecreeState.CollectionApplicable);
    }

    private static bool CanGenerateDocuments(IPermissionService permissionService, DecreeEntity decree)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stammdatenverwalter)
               && AclPermissions.CanAccessOwnBfs(permissionService, decree)
               && decree.PeriodState is CollectionPeriodState.InCollection or CollectionPeriodState.Expired
               && decree.State is DecreeState.CollectionApplicable;
    }

    public static IQueryable<DecreeEntity> WhereCanAddCollection(
        this IQueryable<DecreeEntity> query,
        IPermissionService permissionService)
    {
        return query
            .WhereHasAnyRole(permissionService, [Roles.Stammdatenverwalter, Roles.Kontrollzeichenerfasser])
            .WhereCanAccessOwnBfsOrChildrenOrParents(permissionService)
            .WhereInPeriodState(CollectionPeriodState.InCollection, permissionService.Now);
    }

    private static bool CanAddCollection(IPermissionService permissionService, DecreeEntity decree)
    {
        return AclPermissions.HasAnyRole(permissionService, [Roles.Stammdatenverwalter, Roles.Kontrollzeichenerfasser])
               && AclPermissions.CanAccessOwnBfsOrChildrenOrParents(permissionService, decree)
               && decree.PeriodState is CollectionPeriodState.InCollection;
    }

    public static IQueryable<DecreeEntity> WhereCanSetSensitiveDataExpiryDate(this IQueryable<DecreeEntity> query, IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Kontrollzeichenloescher)
            .WhereCanAccessOwnBfs(permissionService)
            .Where(x => x.State == DecreeState.EndedCameAbout || x.State == DecreeState.EndedCameNotAbout);
    }

    public static IQueryable<DecreeEntity> WhereCanDelete(this IQueryable<DecreeEntity> query, IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Kontrollzeichenloescher)
            .WhereCanAccessOwnBfs(permissionService)
            .Where(x => x.SensitiveDataExpiryDate.HasValue && x.SensitiveDataExpiryDate <= DateOnly.FromDateTime(permissionService.Now))
            .Where(x => x.State == DecreeState.EndedCameAbout || x.State == DecreeState.EndedCameNotAbout);
    }

    private static bool CanDelete(IPermissionService permissionService, DecreeEntity decree)
    {
        return AclPermissions.HasRole(permissionService, Roles.Kontrollzeichenloescher)
               && AclPermissions.CanAccessOwnBfs(permissionService, decree)
               && decree.SensitiveDataExpiryDate.HasValue
               && decree.SensitiveDataExpiryDate <= DateOnly.FromDateTime(permissionService.Now)
               && decree.State is DecreeState.EndedCameAbout or DecreeState.EndedCameNotAbout;
    }

    public static DecreeUserPermissions Build(IPermissionService permissionService, DecreeEntity decree)
    {
        return new DecreeUserPermissions(
            CanEdit(permissionService, decree),
            CanFinish(permissionService, decree),
            CanGenerateDocuments(permissionService, decree),
            CanAddCollection(permissionService, decree),
            CanDelete(permissionService, decree));
    }
}
