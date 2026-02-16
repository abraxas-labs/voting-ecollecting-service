// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Diagnostics.CodeAnalysis;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingIam;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Domain.Queries;

namespace Voting.ECollecting.Admin.Core.Permissions;

[SuppressMessage(
    "StyleCop.CSharp.OrderingRules",
    "SA1202:Elements should be ordered by access",
    Justification = "To make it easier to compare permission related methods, keep them close to each other.")]
internal static class CollectionPermissions
{
    public static IQueryable<T> WhereCanRead<T>(this IQueryable<T> query, IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return query.WhereCanAccessOwnBfsOrChildrenOrParentsInPeriodStateInCollectionOrExpired(permissionService);
    }

    public static IQueryable<T> WhereCanReadMessages<T>(this IQueryable<T> query, IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfsOrChildren(permissionService);
    }

    public static IQueryable<T> WhereCanReadPermissions<T>(this IQueryable<T> query, IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfsOrChildren(permissionService);
    }

    public static IQueryable<T> WhereCanReadCommittee<T>(this IQueryable<T> query, IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfsOrChildrenOrParentsInPeriodStateInCollectionOrExpired(permissionService);
    }

    public static IQueryable<T> WhereCanEditCommittee<T>(this IQueryable<T> query, IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfs(permissionService);
    }

    public static IQueryable<T> WhereCanEdit<T>(this IQueryable<T> query, IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfsOrChildren(permissionService);
    }

    private static bool CanEdit(IPermissionService permissionService, CollectionBaseEntity collection)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stammdatenverwalter)
               && AclPermissions.CanAccessOwnBfsOrChildren(permissionService, collection);
    }

    public static IQueryable<T> WhereCanCreateMessage<T>(this IQueryable<T> query, IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfs(permissionService)
            .WhereIsNotEndedAndNotAborted();
    }

    private static bool CanCreateMessage(IPermissionService permissionService, CollectionBaseEntity collection)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stammdatenverwalter)
               && AclPermissions.CanAccessOwnBfs(permissionService, collection)
               && collection.State.IsNotEndedAndNotAborted();
    }

    public static IQueryable<T> WhereCanDeleteWithdrawn<T>(this IQueryable<T> query, IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfsOrChildren(permissionService)
            .WhereInState(CollectionState.Withdrawn);
    }

    private static bool CanDeleteWithdrawn(IPermissionService permissionService, CollectionBaseEntity collection)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stammdatenverwalter)
               && AclPermissions.CanAccessOwnBfsOrChildren(permissionService, collection)
               && collection.State == CollectionState.Withdrawn;
    }

    public static IQueryable<InitiativeEntity> WhereCanDelete(this IQueryable<InitiativeEntity> query, IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Kontrollzeichenloescher)
            .WhereCanAccessOwnBfs(permissionService)
            .Where(x => x.SensitiveDataExpiryDate.HasValue && x.SensitiveDataExpiryDate <= permissionService.Today)
            .Where(x => x.State == CollectionState.EndedCameAbout || x.State == CollectionState.EndedCameNotAbout);
    }

    private static bool CanDelete(IPermissionService permissionService, CollectionBaseEntity collection)
    {
        return AclPermissions.HasRole(permissionService, Roles.Kontrollzeichenloescher)
               && AclPermissions.CanAccessOwnBfs(permissionService, collection)
               && collection is InitiativeEntity initiative
               && initiative.SensitiveDataExpiryDate.HasValue
               && initiative.SensitiveDataExpiryDate <= permissionService.Today
               && collection.State is CollectionState.EndedCameAbout or CollectionState.EndedCameNotAbout;
    }

    public static IQueryable<T> WhereCanSetSensitiveDataExpiryDate<T>(this IQueryable<T> query, IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return query
            .WhereHasRole(permissionService, Roles.Kontrollzeichenloescher)
            .WhereCanAccessOwnBfs(permissionService)
            .Where(x => x.State == CollectionState.EndedCameAbout || x.State == CollectionState.EndedCameNotAbout);
    }

    public static IQueryable<T> WhereCanSubmitSignatureSheets<T>(this IQueryable<T> query, IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfs(permissionService)
            .WhereIsNotEnded()
            .WhereInPeriodStateInCollectionOrExpired(permissionService.Today);
    }

    private static bool CanSubmitSignatureSheets(IPermissionService permissionService, CollectionBaseEntity collection)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stammdatenverwalter)
               && AclPermissions.CanAccessOwnBfs(permissionService, collection)
               && !collection.State.IsEnded()
               && collection.PeriodState is CollectionPeriodState.InCollection or CollectionPeriodState.Expired;
    }

    public static IQueryable<T> WhereCanReadSignatureSheets<T>(
        this IQueryable<T> query,
        IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return query
            .WhereHasRole(permissionService, Roles.Kontrollzeichenerfasser)
            .WhereCanAccessOwnMunicipalityBfsInclParents(permissionService)
            .WhereInPeriodStateInCollectionOrExpired(permissionService.Today);
    }

    public static bool CanReadSignatureSheets(IPermissionService permissionService, CollectionBaseEntity collection)
    {
        return AclPermissions.HasRole(permissionService, Roles.Kontrollzeichenerfasser)
               && AclPermissions.CanAccessOwnMunicipalityBfsInclParents(permissionService, collection)
               && collection.PeriodState is CollectionPeriodState.InCollection or CollectionPeriodState.Expired;
    }

    public static IQueryable<T> WhereCanEditSignatureSheets<T>(
        this IQueryable<T> query,
        IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return query
            .WhereCanReadSignatureSheets(permissionService)
            .Where(x => x.Municipalities!.Any(y => permissionService.AclBfsLists.BfsMunicipalities.Contains(y.Bfs) && !y.IsLocked));
    }

    public static bool CanEditSignatureSheets(IPermissionService permissionService, CollectionBaseEntity collection)
    {
        return CanReadSignatureSheets(permissionService, collection)
               && collection.Municipalities?.Any(y => permissionService.AclBfsLists.BfsMunicipalities.Contains(y.Bfs) && !y.IsLocked) == true;
    }

    public static IQueryable<T> WhereCanSetCommitteeAddress<T>(
        this IQueryable<T> query,
        IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return query
            .WhereHasRole(permissionService, Roles.Kontrollzeichenerfasser)
            .WhereCanAccessOwnMunicipalityBfsInclParents(permissionService)
            .WhereInPeriodStateInCollectionOrExpired(permissionService.Today);
    }

    public static IQueryable<InitiativeEntity> WhereCanFinishCorrection(this IQueryable<InitiativeEntity> query, IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfs(permissionService)
            .WhereInState(CollectionState.UnderReview)
            .Where(x => x.AdmissibilityDecisionState == AdmissibilityDecisionState.Valid || x.AdmissibilityDecisionState == AdmissibilityDecisionState.ValidButSubjectToConditions)
            .WhereInPeriodStatePublishedOrUnspecified(permissionService.Today);
    }

    private static bool CanFinishCorrection(IPermissionService permissionService, CollectionBaseEntity collection)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stammdatenverwalter)
               && AclPermissions.CanAccessOwnBfs(permissionService, collection)
               && collection is InitiativeEntity
               {
                   State: CollectionState.UnderReview,
                   AdmissibilityDecisionState: AdmissibilityDecisionState.Valid or AdmissibilityDecisionState.ValidButSubjectToConditions,
                   PeriodState: CollectionPeriodState.Published or CollectionPeriodState.Unspecified,
               };
    }

    public static IQueryable<InitiativeEntity> WhereCanSetCollectionPeriod(this IQueryable<InitiativeEntity> query, IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfs(permissionService)
            .WhereInState(CollectionState.PreRecorded)
            .WhereIsPaperSubmission()
            .WhereInPeriodStateUnspecified();
    }

    private static bool CanSetCollectionPeriod(IPermissionService permissionService, CollectionBaseEntity collection)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stammdatenverwalter)
               && AclPermissions.CanAccessOwnBfs(permissionService, collection)
               && collection is InitiativeEntity
               {
                   State: CollectionState.PreRecorded,
                   IsElectronicSubmission: false,
                   PeriodState: CollectionPeriodState.Unspecified,
               };
    }

    public static IQueryable<InitiativeEntity> WhereCanEnable(
        this IQueryable<InitiativeEntity> q,
        IPermissionService permissionService)
    {
        return q
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfs(permissionService)
            .Where(x =>
                x.State == CollectionState.Registered
                || (x.State == CollectionState.UnderReview && x.AdmissibilityDecisionState == AdmissibilityDecisionState.Valid &&
                    x.CollectionStartDate <= permissionService.Today && x.CollectionEndDate >= permissionService.Today));
    }

    private static bool CanEnable(IPermissionService permissionService, CollectionBaseEntity collection)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stammdatenverwalter)
               && AclPermissions.CanAccessOwnBfs(permissionService, collection)
               && (collection.State is CollectionState.Registered
                   || collection is InitiativeEntity
                   {
                       State: CollectionState.UnderReview,
                       AdmissibilityDecisionState: AdmissibilityDecisionState.Valid,
                       PeriodState: CollectionPeriodState.InCollection,
                   });
    }

    public static IQueryable<T> WhereCanFinish<T>(this IQueryable<T> query, IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfs(permissionService)
            .WhereInState(CollectionState.SignatureSheetsSubmitted);
    }

    private static bool CanFinish(IPermissionService permissionService, CollectionBaseEntity collection)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stammdatenverwalter)
               && AclPermissions.CanAccessOwnBfs(permissionService, collection)
               && collection.State == CollectionState.SignatureSheetsSubmitted;
    }

    public static IQueryable<T> WhereCanGenerateDocuments<T>(this IQueryable<T> query, IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfs(permissionService)
            .WhereIsEnded();
    }

    private static bool CanGenerateDocuments(IPermissionService permissionService, CollectionBaseEntity collection)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stammdatenverwalter)
               && AclPermissions.CanAccessOwnBfs(permissionService, collection)
               && collection.State.IsEnded();
    }

    public static IQueryable<InitiativeEntity> WhereCanReadAdmissibilityDecision(
        this IQueryable<InitiativeEntity> query,
        IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfsOrChildren(permissionService);
    }

    public static IQueryable<InitiativeEntity> WhereCanCreateLinkedAdmissibilityDecision(
        this IQueryable<InitiativeEntity> query,
        IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfs(permissionService)
            .WhereInState(CollectionState.Submitted)
            .Where(x => !x.AdmissibilityDecisionState.HasValue ||
                        x.AdmissibilityDecisionState == AdmissibilityDecisionState.Unspecified);
    }

    public static IQueryable<InitiativeEntity> WhereCanEditAdmissibilityDecision(
        this IQueryable<InitiativeEntity> query,
        IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfs(permissionService)
            .Where(x => x.State == CollectionState.PreRecorded
                        || x.AdmissibilityDecisionState == AdmissibilityDecisionState.Open);
    }

    private static bool CanEditAdmissibilityDecision(
        IPermissionService permissionService,
        CollectionBaseEntity collection)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stammdatenverwalter)
               && AclPermissions.CanAccessOwnBfs(permissionService, collection)
               && (collection.State is CollectionState.PreRecorded
                   || collection is InitiativeEntity
                   {
                       AdmissibilityDecisionState: AdmissibilityDecisionState.Open,
                   });
    }

    public static IQueryable<InitiativeEntity> WhereCanDeleteAdmissibilityDecision(
        this IQueryable<InitiativeEntity> query,
        IPermissionService permissionService)
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfs(permissionService)
            .WhereInPeriodStatePublishedOrUnspecified(permissionService.Today)
            .Where(x => x.State == CollectionState.PreRecorded);
    }

    private static bool CanDeleteAdmissibilityDecision(
        IPermissionService permissionService,
        CollectionBaseEntity collection)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stammdatenverwalter)
               && AclPermissions.CanAccessOwnBfs(permissionService, collection)
               && collection is InitiativeEntity
               {
                   PeriodState: CollectionPeriodState.Published or CollectionPeriodState.Unspecified,
                   State: CollectionState.PreRecorded,
               };
    }

    public static IQueryable<T> WhereCanEditGeneralInformation<T>(this IQueryable<T> query, IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return query
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfs(permissionService)
            .Where(x => x.State == CollectionState.PreRecorded);
    }

    private static bool CanEditGeneralInformation(
        IPermissionService permissionService,
        CollectionBaseEntity collection)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stammdatenverwalter)
               && AclPermissions.CanAccessOwnBfs(permissionService, collection)
               && collection.State == CollectionState.PreRecorded;
    }

    private static bool CanReadTotalCount(IPermissionService permissionService, CollectionBaseEntity collection)
    {
        return AclPermissions.CanAccessOwnBfsOrChildren(permissionService, collection)
               && collection.State.IsEnabledForCollectionOrEnded();
    }

    private static bool IsRequestInformalReviewVisible(IPermissionService permissionService, CollectionBaseEntity collection)
        => AclPermissions.CanAccessOwnBfs(permissionService, collection)
           && collection.State is CollectionState.InPreparation;

    // check samples is only allowed for a collection owner
    public static IQueryable<T> WhereCanCheckSamples<T>(
        this IQueryable<T> query,
        IPermissionService permissionService)
        where T : CollectionBaseEntity
    {
        return query
            .WhereHasRole(permissionService, Roles.Stichprobenverwalter)
            .WhereCanAccessOwnBfs(permissionService)
            .WhereIsEnded();
    }

    public static bool CanCheckSamples(IPermissionService permissionService, CollectionBaseEntity collection)
        => AclPermissions.HasRole(permissionService, Roles.Stichprobenverwalter)
           && AclPermissions.CanAccessOwnBfs(permissionService, collection)
           && collection.State.IsEnded();

    public static IQueryable<InitiativeEntity> WhereCanReturnForCorrection(
        this IQueryable<InitiativeEntity> q,
        IPermissionService permissionService)
    {
        return q
            .WhereHasRole(permissionService, Roles.Stammdatenverwalter)
            .WhereCanAccessOwnBfs(permissionService)
            .Where(x => x.State == CollectionState.Submitted || x.State == CollectionState.UnderReview);
    }

    private static bool CanReturnForCorrection(IPermissionService permissionService, CollectionBaseEntity collection)
    {
        return AclPermissions.HasRole(permissionService, Roles.Stammdatenverwalter)
               && AclPermissions.CanAccessOwnBfs(permissionService, collection)
               && collection.State is CollectionState.Submitted or CollectionState.UnderReview;
    }

    public static CollectionUserPermissions Build(IPermissionService permissionService, CollectionBaseEntity collection)
    {
        return new CollectionUserPermissions(
            CanEdit(permissionService, collection),
            IsRequestInformalReviewVisible(permissionService, collection),
            CanCreateMessage(permissionService, collection),
            CanFinishCorrection(permissionService, collection),
            CanSetCollectionPeriod(permissionService, collection),
            CanEnable(permissionService, collection),
            CanDelete(permissionService, collection),
            CanDeleteWithdrawn(permissionService, collection),
            CanReadTotalCount(permissionService, collection),
            CanEditSignatureSheets(permissionService, collection),
            CanReadSignatureSheets(permissionService, collection),
            CanCheckSamples(permissionService, collection),
            CanSubmitSignatureSheets(permissionService, collection),
            CanFinish(permissionService, collection),
            CanGenerateDocuments(permissionService, collection),
            CanEditAdmissibilityDecision(permissionService, collection),
            CanDeleteAdmissibilityDecision(permissionService, collection),
            CanEditGeneralInformation(permissionService, collection),
            CanReturnForCorrection(permissionService, collection));
    }
}
