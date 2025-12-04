// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Diagnostics.CodeAnalysis;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Extensions;

namespace Voting.ECollecting.Admin.Core.Permissions;

[SuppressMessage(
    "StyleCop.CSharp.OrderingRules",
    "SA1202:Elements should be ordered by access",
    Justification = "To make it easier to compare permission related methods, keep them close to each other.")]
internal static class CollectionSignatureSheetPermissions
{
    public static IQueryable<CollectionSignatureSheetEntity> WhereCanRead(
        this IQueryable<CollectionSignatureSheetEntity> query,
        IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Kontrollzeichenerfasser)
            .WhereCanAccessOwnMunicipalityBfs(permissionService);
    }

    public static bool CanRead(IPermissionService permissionService, CollectionSignatureSheetEntity sheet)
    {
        return AclPermissions.HasRole(permissionService, Roles.Kontrollzeichenerfasser)
               && CanAccessOwnMunicipalityBfs(permissionService, sheet);
    }

    public static IQueryable<CollectionSignatureSheetEntity> WhereCanAttest(
        this IQueryable<CollectionSignatureSheetEntity> query,
        IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Kontrollzeichenerfasser)
            .WhereCanAccessOwnMunicipalityBfs(permissionService);
    }

    public static IQueryable<CollectionSignatureSheetEntity> WhereCanEdit(
        this IQueryable<CollectionSignatureSheetEntity> query,
        IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Kontrollzeichenerfasser)
            .WhereCanAccessOwnMunicipalityBfs(permissionService)
            .Where(x => x.State == CollectionSignatureSheetState.Created);
    }

    public static bool CanEdit(IPermissionService permissionService, CollectionSignatureSheetEntity sheet)
    {
        return AclPermissions.HasRole(permissionService, Roles.Kontrollzeichenerfasser)
               && CanAccessOwnMunicipalityBfs(permissionService, sheet)
               && sheet.State == CollectionSignatureSheetState.Created;
    }

    public static IQueryable<CollectionSignatureSheetEntity> WhereCanDelete(
        this IQueryable<CollectionSignatureSheetEntity> query,
        IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Kontrollzeichenerfasser)
            .WhereCanAccessOwnMunicipalityBfs(permissionService)
            .Where(x => x.State == CollectionSignatureSheetState.Created);
    }

    private static bool CanDelete(IPermissionService permissionService, CollectionSignatureSheetEntity sheet)
    {
        return AclPermissions.HasRole(permissionService, Roles.Kontrollzeichenerfasser)
                && CanAccessOwnMunicipalityBfs(permissionService, sheet)
                && sheet.State == CollectionSignatureSheetState.Created;
    }

    public static IQueryable<CollectionSignatureSheetEntity> WhereCanSubmit(
        this IQueryable<CollectionSignatureSheetEntity> query,
        IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Stichprobenverwalter)
            .WhereCanAccessOwnBfsOrChildren(permissionService)
            .Where(x => x.State == CollectionSignatureSheetState.Attested);
    }

    private static bool CanSubmit(IPermissionService permissionService, CollectionSignatureSheetEntity sheet)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stichprobenverwalter)
               && CanAccessOwnBfsOrChildren(permissionService, sheet)
               && sheet.State == CollectionSignatureSheetState.Attested;
    }

    public static IQueryable<CollectionSignatureSheetEntity> WhereCanUnsubmit(
        this IQueryable<CollectionSignatureSheetEntity> query,
        IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Stichprobenverwalter)
            .WhereCanAccessOwnBfsOrChildren(permissionService)
            .Where(x => x.State == CollectionSignatureSheetState.Submitted);
    }

    private static bool CanUnsubmit(IPermissionService permissionService, CollectionSignatureSheetEntity sheet)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stichprobenverwalter)
               && CanAccessOwnBfsOrChildren(permissionService, sheet)
               && sheet.State == CollectionSignatureSheetState.Submitted;
    }

    public static IQueryable<CollectionSignatureSheetEntity> WhereCanDiscard(
        this IQueryable<CollectionSignatureSheetEntity> query,
        IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Stichprobenverwalter)
            .WhereCanAccessOwnBfsOrChildren(permissionService)
            .Where(x => x.State == CollectionSignatureSheetState.Attested);
    }

    private static bool CanDiscard(IPermissionService permissionService, CollectionSignatureSheetEntity sheet)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stichprobenverwalter)
               && CanAccessOwnBfsOrChildren(permissionService, sheet)
               && sheet.State == CollectionSignatureSheetState.Attested;
    }

    public static IQueryable<CollectionSignatureSheetEntity> WhereCanRestore(
        this IQueryable<CollectionSignatureSheetEntity> query,
        IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Stichprobenverwalter)
            .WhereCanAccessOwnBfsOrChildren(permissionService)
            .Where(x => x.State == CollectionSignatureSheetState.NotSubmitted);
    }

    private static bool CanRestore(IPermissionService permissionService, CollectionSignatureSheetEntity sheet)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stichprobenverwalter)
               && CanAccessOwnBfsOrChildren(permissionService, sheet)
               && sheet.State == CollectionSignatureSheetState.NotSubmitted;
    }

    public static IQueryable<CollectionSignatureSheetEntity> WhereCanCheckSamples(
        this IQueryable<CollectionSignatureSheetEntity> query,
        IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Stichprobenverwalter)
            .WhereCanAccessOwnBfsOrChildren(permissionService)
            .Where(x => x.State >= CollectionSignatureSheetState.Attested);
    }

    public static bool CanCheckSamples(IPermissionService permissionService, CollectionSignatureSheetEntity sheet)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stichprobenverwalter)
               && CanAccessOwnBfsOrChildren(permissionService, sheet)
               && sheet.State.IsAttestedOrLater();
    }

    public static IQueryable<CollectionSignatureSheetEntity> WhereCanConfirm(
        this IQueryable<CollectionSignatureSheetEntity> query,
        IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Stichprobenverwalter)
            .WhereCanAccessOwnBfsOrChildren(permissionService)
            .Where(x => x.State == CollectionSignatureSheetState.Submitted);
    }

    public static bool CanConfirm(IPermissionService permissionService, CollectionSignatureSheetEntity sheet)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stichprobenverwalter)
               && CanAccessOwnBfsOrChildren(permissionService, sheet)
               && sheet.State == CollectionSignatureSheetState.Submitted;
    }

    private static IQueryable<CollectionSignatureSheetEntity> WhereCanAccessOwnMunicipalityBfs(this IQueryable<CollectionSignatureSheetEntity> query, IPermissionService permissionService)
    {
        return query.Where(x => permissionService.AclBfsLists.BfsMunicipalities.Contains(x.CollectionMunicipality!.Bfs));
    }

    private static bool CanAccessOwnMunicipalityBfs(IPermissionService permissionService, CollectionSignatureSheetEntity signatureSheet)
    {
        return permissionService.AclBfsLists.BfsMunicipalities.Contains(signatureSheet.CollectionMunicipality!.Bfs);
    }

    private static IQueryable<CollectionSignatureSheetEntity> WhereCanAccessOwnBfsOrChildren(this IQueryable<CollectionSignatureSheetEntity> query, IPermissionService permissionService)
    {
        return query.Where(x => permissionService.AclBfsLists.BfsInclChildren.Contains(x.CollectionMunicipality!.Bfs));
    }

    private static bool CanAccessOwnBfsOrChildren(IPermissionService permissionService, CollectionSignatureSheetEntity signatureSheet)
    {
        return permissionService.AclBfsLists.BfsInclChildren.Contains(signatureSheet.CollectionMunicipality!.Bfs);
    }

    public static CollectionSignatureSheetUserPermissions Build(IPermissionService permissionService, CollectionSignatureSheetEntity sheet)
    {
        return new CollectionSignatureSheetUserPermissions(
            CanEdit(permissionService, sheet),
            CanDelete(permissionService, sheet),
            CanSubmit(permissionService, sheet),
            CanUnsubmit(permissionService, sheet),
            CanDiscard(permissionService, sheet),
            CanRestore(permissionService, sheet),
            CanConfirm(permissionService, sheet));
    }
}
