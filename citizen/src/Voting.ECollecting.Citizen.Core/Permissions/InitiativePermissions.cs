// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Diagnostics.CodeAnalysis;
using Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin;
using Voting.ECollecting.Citizen.Domain.Queries;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Queries;
using Voting.Lib.Common;

namespace Voting.ECollecting.Citizen.Core.Permissions;

[SuppressMessage(
    "StyleCop.CSharp.OrderingRules",
    "SA1202:Elements should be ordered by access",
    Justification = "To make it easier to compare permission related methods, keep them close to each other.")]
internal static class InitiativePermissions
{
    public static IQueryable<InitiativeEntity> WhereCanReadWithMembershipToken(
        this IQueryable<InitiativeEntity> q,
        IPermissionService permissionService,
        UrlToken token)
    {
        return q
            .WhereInPreparationOrReturnedForCorrection()
            .Where(x => x.CommitteeMembers.Any(m =>
                m.ApprovalState == InitiativeCommitteeMemberApprovalState.Requested
                && m.Token == token
                && m.TokenExpiry >= permissionService.Now));
    }

    public static IQueryable<InitiativeEntity> WhereCanSubmit(this IQueryable<InitiativeEntity> query, IPermissionService permissionService)
    {
        return query
            .WhereCanWrite(permissionService)
            .WhereInState(CollectionState.InPreparation)
            .WhereHasNoAdmissibilityDecision();
    }

    public static bool CanSubmit(InitiativeEntity collection)
        => collection.State is CollectionState.InPreparation
           && collection.AdmissibilityDecisionState is null;

    public static bool IsSubmitVisible(InitiativeEntity collection)
        => CanSubmit(collection)
           || (collection.State is CollectionState.Submitted && collection.AdmissibilityDecisionState == null);

    public static IQueryable<InitiativeEntity> WhereCanFlagForReview(this IQueryable<InitiativeEntity> query, IPermissionService permissionService)
    {
        return query
            .WhereCanWrite(permissionService)
            .Where(x => x.State == CollectionState.ReturnedForCorrection
                        || (x.State == CollectionState.InPreparation && x.AdmissibilityDecisionState != null));
    }

    public static bool CanFlagForReview(InitiativeEntity collection)
        => collection.State is CollectionState.ReturnedForCorrection
           || collection is { State: CollectionState.InPreparation, AdmissibilityDecisionState: not null };

    public static bool IsFlagForReviewVisible(InitiativeEntity collection)
        => CanFlagForReview(collection) || collection.State == CollectionState.UnderReview;
}
