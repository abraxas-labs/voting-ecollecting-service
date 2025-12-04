// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Queries;

namespace Voting.ECollecting.Citizen.Core.Permissions;

internal static class ReferendumPermissions
{
    public static IQueryable<ReferendumEntity> WhereCanSubmit(this IQueryable<ReferendumEntity> query, IPermissionService permissionService)
    {
        return query
            .WhereCanWrite(permissionService)
            .WhereInState(CollectionState.InPreparation);
    }

    public static bool CanSubmit(ReferendumEntity collection)
        => collection.State is CollectionState.InPreparation;

    public static bool IsSubmitVisible(ReferendumEntity collection)
        => CanSubmit(collection);
}
