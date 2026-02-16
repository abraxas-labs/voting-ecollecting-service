// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Domain.Queries;
using CollectionUserPermissions = Voting.ECollecting.Citizen.Domain.Models.CollectionUserPermissions;

namespace Voting.ECollecting.Citizen.Core.Permissions;

/// <summary>
/// Permissions for collections.
///
/// The `Where*` queryable methods include role checks inside each `Where` call,
/// whereas the `Can*` methods do not.
///
/// The reason for this difference is performance:
/// - `Where*` methods are typically used once in queries to filter collections, so the role check inside them is efficient.
/// - `Can*` methods, however, are used when constructing a <see cref="CollectionUserPermissions"/> object.
///   Since all `Can*` methods are called for every collection, including a role check in each one
///   would result in an O(n * m) complexity for each collection, where:
///     - n = number of `Can*` checks
///     - m = number of permissions in the collection.
///
/// To avoid this overhead, role checks for the Can* methods are performed only once in the <see cref="Build{T}"/> method.
/// </summary>
[SuppressMessage(
    "StyleCop.CSharp.OrderingRules",
    "SA1202:Elements should be ordered by access",
    Justification = "To make it easier to compare permission related methods, keep them close to each other.")]
internal static class CollectionPermissions
{
    public static IQueryable<T> WhereCanReadOrIsPastRegistered<T>(
        this IQueryable<T> q,
        IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return q.Where(x =>
            x.State > CollectionState.Registered
            || x.AuditInfo.CreatedById == permissionService.UserId
            || x.Permissions!.Any(p =>
                p.State == CollectionPermissionState.Accepted
                && p.IamUserId == permissionService.UserId
                && (p.Role == CollectionPermissionRole.Deputy || p.Role == CollectionPermissionRole.Reader)));
    }

    public static IQueryable<T> WhereCanEdit<T>(this IQueryable<T> queryable, IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return queryable
            .WhereCanWrite(permissionService)
            .WhereInPreparationOrReturnedForCorrection();
    }

    private static bool CanEdit(CollectionBaseEntity collection)
        => collection.State.InPreparationOrReturnForCorrection();

    public static IQueryable<T> WhereCanEditSignatureSheetTemplate<T>(this IQueryable<T> queryable, IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return queryable
            .WhereCanWrite(permissionService)
            .WhereIsNotEndedAndNotAborted();
    }

    private static bool CanEditSignatureSheetTemplate(CollectionBaseEntity collection)
        => collection.State.IsNotEndedAndNotAborted();

    public static IQueryable<T> WhereCanDeleteSignatureSheetTemplate<T>(this IQueryable<T> queryable, IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return queryable.WhereCanEdit(permissionService);
    }

    private static bool CanDeleteSignatureSheetTemplate(CollectionBaseEntity collection)
        => CanEdit(collection);

    public static IQueryable<T> WhereCanGenerateSignatureSheetTemplatePreview<T>(this IQueryable<T> queryable, IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        // generalInformationIsValid is checked separately and not part of the permission.
        return queryable
            .WhereCanWrite(permissionService)
            .WhereIsNotEndedAndNotAborted();
    }

    private static bool CanGenerateSignatureSheetTemplatePreview(
        CollectionBaseEntity collection,
        bool generalInformationIsValid)
        => collection.State.IsNotEndedAndNotAborted() && generalInformationIsValid;

    public static IQueryable<T> WhereCanGetElectronicSignaturesProtocol<T>(
        this IQueryable<T> queryable,
        IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return queryable
            .WhereCanWrite(permissionService)
            .WhereIsEnded()
            .Where(x => x.DomainOfInfluenceType != DomainOfInfluenceType.Mu);
    }

    private static bool CanGetElectronicSignaturesProtocol(CollectionBaseEntity collection)
        => collection.State.IsEnded()
            && collection.DomainOfInfluenceType != DomainOfInfluenceType.Mu;

    private static bool CanEditPermissions(CollectionBaseEntity collection)
        => collection.State.IsNotEndedAndNotAborted();

    public static IQueryable<T> WhereCanCreateMessages<T>(this IQueryable<T> query, IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return query
            .WhereCanWrite(permissionService)
            .WhereIsNotEndedAndNotAborted();
    }

    private static bool CanCreateMessages(CollectionBaseEntity collection)
        => collection.State.IsNotEndedAndNotAborted();

    public static IQueryable<T> WhereCanRegister<T>(this IQueryable<T> query, IPermissionService permissionService)
    where T : CollectionBaseEntity
    {
        return query
            .WhereCanWrite(permissionService)
            .WhereInState(CollectionState.ReadyForRegistration);
    }

    private static bool CanRegister(CollectionBaseEntity collection)
        => collection.State is CollectionState.ReadyForRegistration;

    public static IQueryable<T> WhereCanWithdraw<T>(this IQueryable<T> query, IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return query
            .WhereCanWrite(permissionService)
            .Where(x => !(x is InitiativeEntity) || (x as InitiativeEntity)!.AdmissibilityDecisionState == null)
            .WhereInState(CollectionState.InPreparation)
            .WhereIsElectronicSubmission();
    }

    private static bool CanWithdraw(CollectionBaseEntity collection)
    {
        return (collection is not InitiativeEntity initiative || initiative.AdmissibilityDecisionState == null)
               && collection is
               {
                   State: CollectionState.InPreparation,
                   IsElectronicSubmission: true
               };
    }

    public static IQueryable<T> WhereCanRequestInformalReview<T>(
        this IQueryable<T> query,
        IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        // no InformalReviewRequested check since it can also be unset and is checked outside the permissions.
        return query
            .WhereCanWrite(permissionService)
            .WhereInState(CollectionState.InPreparation)
            .Where(x => !(x is ReferendumEntity) || (x as ReferendumEntity)!.DecreeId.HasValue);
    }

    private static bool CanRequestInformalReview(CollectionBaseEntity collection)
        => collection is { State: CollectionState.InPreparation, InformalReviewRequested: false }
            and not ReferendumEntity { DecreeId: null };

    private static bool IsRequestInformalReviewVisible(CollectionBaseEntity collection)
        => collection.State is CollectionState.InPreparation;

    public static IQueryable<T> WhereCanReadPermissions<T>(
        this IQueryable<T> queryable,
        IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return queryable
            .WhereCanRead(permissionService)
            .WhereIsNotEndedAndNotAborted();
    }

    public static IQueryable<T> WhereCanEditPermissions<T>(
        this IQueryable<T> queryable,
        IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return queryable
            .WhereCanWrite(permissionService)
            .WhereIsNotEndedAndNotAborted();
    }

    private static bool CanSubmit<T>(T collection)
        where T : CollectionBaseEntity
    {
        return collection switch
        {
            InitiativeEntity initiative => InitiativePermissions.CanSubmit(initiative),
            ReferendumEntity referendum => ReferendumPermissions.CanSubmit(referendum),
            _ => false,
        };
    }

    private static bool IsSubmitVisible<T>(T collection)
        where T : CollectionBaseEntity
    {
        return collection switch
        {
            InitiativeEntity initiative => InitiativePermissions.IsSubmitVisible(initiative),
            ReferendumEntity referendum => ReferendumPermissions.IsSubmitVisible(referendum),
            _ => false,
        };
    }

    private static bool CanFlagForReview<T>(T collection)
        where T : CollectionBaseEntity
    {
        return collection switch
        {
            InitiativeEntity initiative => InitiativePermissions.CanFlagForReview(initiative),
            _ => false,
        };
    }

    private static bool IsFlagForReviewVisible<T>(T collection)
        where T : CollectionBaseEntity
    {
        return collection switch
        {
            InitiativeEntity initiative => InitiativePermissions.IsFlagForReviewVisible(initiative),
            _ => false,
        };
    }

    public static bool CanEditSubType(CollectionBaseEntity collection)
        => CanEdit(collection)
           && collection is InitiativeEntity { AdmissibilityDecisionState: null or AdmissibilityDecisionState.Unspecified };

    public static IQueryable<T> WhereCanEditCommitteeMemberPoliticalDuty<T>(
        this IQueryable<T> queryable,
        IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return queryable
            .WhereCanWrite(permissionService)
            .WhereIsNotEndedAndNotAborted();
    }

    public static CollectionUserPermissions? Build<T>(
        IPermissionService permissionService,
        T collection,
        bool generalInformationIsValid)
        where T : CollectionBaseEntity
    {
        var role = GetRole(permissionService, collection);
        return role switch
        {
            CollectionPermissionRole.Reader => CollectionUserPermissions.ReadOnly,
            CollectionPermissionRole.Deputy => BuildForOwnerOrDeputy(role, collection, generalInformationIsValid),
            CollectionPermissionRole.Owner => BuildForOwnerOrDeputy(role, collection, generalInformationIsValid),
            _ => null,
        };
    }

    private static CollectionUserPermissions BuildForOwnerOrDeputy<T>(
        CollectionPermissionRole role,
        T collection,
        bool generalInformationIsValid)
        where T : CollectionBaseEntity
    {
        return new CollectionUserPermissions(
            role,
            CanEdit(collection),
            CanEditSignatureSheetTemplate(collection),
            CanDeleteSignatureSheetTemplate(collection),
            CanGenerateSignatureSheetTemplatePreview(collection, generalInformationIsValid),
            CanEditPermissions(collection),
            CanCreateMessages(collection),
            CanSubmit(collection),
            IsSubmitVisible(collection),
            CanFlagForReview(collection),
            IsFlagForReviewVisible(collection),
            CanRegister(collection),
            CanWithdraw(collection),
            CanRequestInformalReview(collection),
            IsRequestInformalReviewVisible(collection),
            CanGetElectronicSignaturesProtocol(collection),
            CanEditSubType(collection),
            true);
    }

    private static CollectionPermissionRole GetRole(IPermissionService permissionService, CollectionBaseEntity collection)
    {
        if (permissionService.UserId == collection.AuditInfo.CreatedById)
        {
            return CollectionPermissionRole.Owner;
        }

        Debug.Assert(collection.Permissions != null, "Collection permission should be loaded with integrated query.");
        return collection.Permissions?
            .OrderByDescending(p => p.Role)
            .FirstOrDefault(p => p.State == CollectionPermissionState.Accepted && p.IamUserId == permissionService.UserId)
            ?.Role ?? CollectionPermissionRole.Unspecified;
    }
}
