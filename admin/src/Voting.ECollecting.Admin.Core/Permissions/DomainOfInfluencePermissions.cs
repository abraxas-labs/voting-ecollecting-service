// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Core.Permissions;

internal static class DomainOfInfluencePermissions
{
    public static IQueryable<DomainOfInfluenceEntity> WhereCanEdit(this IQueryable<DomainOfInfluenceEntity> query, IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfs(permissionService);
    }

    public static DomainOfInfluenceUserPermissions Build(
        IPermissionService permissionService,
        DomainOfInfluence domainOfInfluence)
    {
        return new DomainOfInfluenceUserPermissions(CanEdit(permissionService, domainOfInfluence));
    }

    private static bool CanEdit(IPermissionService permissionService, DomainOfInfluence domainOfInfluence)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stammdatenverwalter)
               && AclPermissions.CanAccessOwnBfs(permissionService, domainOfInfluence);
    }
}
