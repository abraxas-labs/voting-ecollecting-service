// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Ecollecting.Shared.V1.Models;
using Google.Protobuf.WellKnownTypes;
using Riok.Mapperly.Abstractions;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Proto.Citizen.Services.V1.Responses;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Database.Models;
using DomainModels = Voting.ECollecting.Citizen.Domain.Models;
using MarkdownString = Voting.Lib.Database.Models.MarkdownString;
using ProtoCitizenModels = Voting.ECollecting.Proto.Citizen.Services.V1.Models;
using ProtoSharedEnums = Voting.ECollecting.Proto.Shared.V1.Enums;
using SharedDomainModels = Voting.ECollecting.Shared.Domain.Models;
using SharedProtoModels = Abraxas.Voting.Ecollecting.Shared.V1.Models;

namespace Voting.ECollecting.Citizen.Api.Grpc.Mappings;

// don't explicitly assign null to protos,
// no need since all are new objects which don't have the fields initialized anyway
[Mapper(AllowNullPropertyAssignment = false, EnumMappingStrategy = EnumMappingStrategy.ByValueCheckDefined)]
internal static partial class Mapper
{
    internal static ListInitiativesResponse MapToListInitiativesResponse(IEnumerable<DomainModels.Initiative> initiatives)
        => new ListInitiativesResponse { Initiatives = { MapToInitiatives(initiatives) } };

    internal static ListMyReferendumsResponse MapToListMyReferendumsResponse(IEnumerable<DomainModels.Decree> decrees, IEnumerable<DomainModels.Referendum> referendums)
        => new ListMyReferendumsResponse { Decrees = { MapToDecrees(decrees) }, WithoutDecreeReferendums = { MapToReferendums(referendums) } };

    internal static ListDecreesEligibleForReferendumResponse MapToListDecreesEligibleForReferendumResponse(
        Dictionary<DomainOfInfluenceType, List<DomainModels.Decree>> decreeGroups)
        => new ListDecreesEligibleForReferendumResponse { Groups = { MapToDecreeGroups(decreeGroups) } };

    internal static ListCollectionsResponse MapToListCollectionResponse(
        Dictionary<DomainOfInfluenceType, DomainModels.CollectionsGroup> collectionGroups)
        => new ListCollectionsResponse { Groups = { MapToCollectionsGroup(collectionGroups) } };

    internal static ListDomainOfInfluencesResponse MapToListDomainOfInfluencesResponse(
        IEnumerable<DomainModels.DomainOfInfluence> domainOfInfluences)
        => new ListDomainOfInfluencesResponse { DomainOfInfluences = { MapToDomainOfInfluences(domainOfInfluences) } };

    internal static ListInitiativeSubTypesResponse MapToListInitiativeSubTypesResponse(
        IEnumerable<InitiativeSubTypeEntity> subTypes)
        => new ListInitiativeSubTypesResponse { SubTypes = { MapInitiativeSubTypes(subTypes) } };

    internal static ListCollectionPermissionsResponse MapToListCollectionPermissionsResponse(
        IEnumerable<DomainModels.CollectionPermission> permissions)
        => new ListCollectionPermissionsResponse { Permissions = { MapCollectionPermissions(permissions) } };

    internal static partial GetPendingCollectionPermissionResponse MapToGetCollectionPermissionResponse(
        DomainModels.PendingCollectionPermission permission);

    internal static ListCollectionMessagesResponse MapToCollectionMessagesResponse(List<CollectionMessageEntity> messages, bool informalReviewRequested)
        => new ListCollectionMessagesResponse { Messages = { MapCollectionMessages(messages) }, InformalReviewRequested = informalReviewRequested };

    internal static partial DomainOfInfluenceType MapDomainOfInfluenceType(ProtoSharedEnums.DomainOfInfluenceType domainOfInfluenceType);

    internal static partial IReadOnlySet<ProtoSharedEnums.DomainOfInfluenceType> MapDomainOfInfluenceTypes(IEnumerable<DomainOfInfluenceType> domainOfInfluenceType);

    internal static partial CollectionPermissionRole MapCollectionPermissionRole(ProtoSharedEnums.CollectionPermissionRole role);

    internal static partial CollectionType MapCollectionType(ProtoSharedEnums.CollectionType collectionType);

    [MapPropertyFromSource(nameof(ProtoCitizenModels.Initiative.Collection))]
    internal static partial ProtoCitizenModels.Initiative MapToInitiative(DomainModels.Initiative initiative);

    [MapPropertyFromSource(nameof(ProtoCitizenModels.Referendum.Collection))]
    internal static partial ProtoCitizenModels.Referendum MapToReferendum(DomainModels.Referendum referendum);

    internal static partial List<ProtoCitizenModels.Referendum> MapToReferendums(IEnumerable<DomainModels.Referendum> referendums);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial IdValue MapToIdValue(BaseEntity entity);

    internal static partial CollectionAddress MapToCollectionAddress(ProtoCitizenModels.CollectionAddress address);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial ProtoCitizenModels.InitiativeCommittee MapToInitiativeCommittee(SharedDomainModels.InitiativeCommittee committee);

    internal static partial CollectionPermissionRole MapToRole(ProtoSharedEnums.CollectionPermissionRole role);

    internal static partial GetPendingCommitteeMemberResponse MapToPendingCommitteeMembership(
        DomainModels.PendingCommitteeMembership data);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(request.Role))]
    [MapProperty(nameof(request.RequestMemberSignature), nameof(InitiativeCommitteeMemberEntity.MemberSignatureRequested))]
    internal static partial InitiativeCommitteeMemberEntity MapToInitiativeCommitteeMember(AddCommitteeMemberRequest request);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(request.Role))]
    [MapProperty(nameof(request.RequestMemberSignature), nameof(InitiativeCommitteeMemberEntity.MemberSignatureRequested))]
    internal static partial InitiativeCommitteeMemberEntity MapToInitiativeCommitteeMember(UpdateCommitteeMemberRequest request);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapNestedProperties(nameof(CollectionMessageEntity.AuditInfo))]
    internal static partial CollectionMessage MapToCollectionMessage(CollectionMessageEntity message);

    internal static partial ProtoCitizenModels.ValidationSummary MapToValidationSummary(DomainModels.ValidationSummary validationSummary);

    internal static partial IEnumerable<DomainOfInfluenceType> MapToDomainOfInfluenceTypes(IEnumerable<ProtoSharedEnums.DomainOfInfluenceType> domainOfInfluenceTypes);

    internal static partial CollectionPeriodState MapCollectionPeriodState(ProtoSharedEnums.CollectionPeriodState src);

    internal static partial SharedDomainModels.AccessibilityMessage MapToAccessibilityMessage(SendAccessibilityMessageRequest request);

    internal static GenerateSignatureSheetTemplatePreviewResponse MapToGenerateSignatureSheetTemplatePreviewResponse(FileEntity file)
        => new GenerateSignatureSheetTemplatePreviewResponse { GeneratedSignatureSheetTemplate = MapToFile(file) };

    internal static SetSignatureSheetTemplateGeneratedResponse MapToSetSignatureSheetTemplateGeneratedResponse(FileEntity file)
        => new SetSignatureSheetTemplateGeneratedResponse { GeneratedSignatureSheetTemplate = MapToFile(file) };

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(@InitiativeCommitteeMemberEntity.Permission.Role), nameof(ProtoCitizenModels.InitiativeCommitteeMember.Role))]
    private static partial ProtoCitizenModels.InitiativeCommitteeMember MapToInitiativeCommitteeMember(SharedDomainModels.InitiativeCommitteeMember member);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ProtoCitizenModels.CollectionAddress MapToCollectionAddress(CollectionAddress address);

    private static partial IEnumerable<ProtoCitizenModels.Initiative> MapToInitiatives(IEnumerable<DomainModels.Initiative> initiatives);

    private static partial IEnumerable<ProtoCitizenModels.Decree> MapToDecrees(IEnumerable<DomainModels.Decree> decrees);

    private static partial IEnumerable<CollectionMessage> MapCollectionMessages(List<CollectionMessageEntity> messages);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ProtoCitizenModels.CollectionCount MapToCollectionCount(CollectionCountEntity collectionCountEntity);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapperIgnoreTarget(nameof(ProtoCitizenModels.Collection.UserPermissions))]
    [MapperIgnoreTarget(nameof(ProtoCitizenModels.Collection.IsSigned))]
    [MapperIgnoreTarget(nameof(ProtoCitizenModels.Collection.SignatureType))]
    [MapperIgnoreTarget(nameof(ProtoCitizenModels.Collection.AttestedCollectionCount))]
    private static partial ProtoCitizenModels.Collection MapToReferendumCollection(ReferendumEntity referendum);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ProtoCitizenModels.Collection MapToReferendumCollection(DomainModels.Referendum referendum);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial SharedDomainModels.NullableCollectionCount MapToNullableCollectionCount(CollectionCountEntity source);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial SharedProtoModels.File MapToFile(FileEntity file);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapperIgnoreTarget(nameof(ProtoCitizenModels.Collection.UserPermissions))]
    [MapperIgnoreTarget(nameof(ProtoCitizenModels.Collection.IsSigned))]
    [MapperIgnoreTarget(nameof(ProtoCitizenModels.Collection.SignatureType))]
    [MapperIgnoreTarget(nameof(ProtoCitizenModels.Collection.AttestedCollectionCount))]
    private static partial ProtoCitizenModels.Collection MapToInitiativeCollection(InitiativeEntity initiative);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ProtoCitizenModels.Collection MapToInitiativeCollection(DomainModels.Initiative initiative);

    private static partial IEnumerable<ProtoCitizenModels.DomainOfInfluence> MapToDomainOfInfluences(
        IEnumerable<DomainModels.DomainOfInfluence> domainOfInfluences);

    private static partial ProtoCitizenModels.DomainOfInfluence MapToDomainOfInfluence(DomainModels.DomainOfInfluence domainOfInfluence);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapperIgnoreTarget(nameof(ProtoCitizenModels.Decree.UserPermissions))]
    [MapperIgnoreTarget(nameof(ProtoCitizenModels.Decree.AttestedCollectionCount))]
    private static partial ProtoCitizenModels.Decree MapDecree(DecreeEntity decree);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(DomainModels.Decree.Referendums), nameof(ProtoCitizenModels.Decree.Collections))]
    private static partial ProtoCitizenModels.Decree MapToDecree(DomainModels.Decree decree);

    private static partial IEnumerable<ProtoCitizenModels.CollectionsGroup> MapToCollectionsGroup(
        Dictionary<DomainOfInfluenceType, DomainModels.CollectionsGroup> groups);

    [MapProperty(
        nameof(KeyValuePair<DomainOfInfluenceType, DomainModels.CollectionsGroup>.Key),
        nameof(ProtoCitizenModels.CollectionsGroup.DomainOfInfluenceType))]
    [MapNestedProperties(nameof(KeyValuePair<DomainOfInfluenceType, DomainModels.CollectionsGroup>.Value))]
    private static partial ProtoCitizenModels.CollectionsGroup MapToCollectionsGroup(
        KeyValuePair<DomainOfInfluenceType, DomainModels.CollectionsGroup> groups);

    private static partial IEnumerable<ProtoCitizenModels.DecreeGroup> MapToDecreeGroups(
        Dictionary<DomainOfInfluenceType, List<DomainModels.Decree>> groups);

    [MapProperty(
        nameof(KeyValuePair<DomainOfInfluenceType, List<DomainModels.Decree>>.Key),
        nameof(ProtoCitizenModels.DecreeGroup.DomainOfInfluenceType))]
    [MapProperty(
        nameof(KeyValuePair<DomainOfInfluenceType, List<DomainModels.Decree>>.Value),
        nameof(ProtoCitizenModels.DecreeGroup.Decrees))]
    private static partial ProtoCitizenModels.DecreeGroup MapToDecreeGroup(KeyValuePair<DomainOfInfluenceType, List<DomainModels.Decree>> groups);

    private static partial IEnumerable<ProtoCitizenModels.InitiativeSubType> MapInitiativeSubTypes(IEnumerable<InitiativeSubTypeEntity> subTypes);

    [MapperIgnoreSource(nameof(InitiativeSubTypeEntity.Bfs))]
    private static partial ProtoCitizenModels.InitiativeSubType MapInitiativeSubType(InitiativeSubTypeEntity subType);

    private static partial IEnumerable<ProtoCitizenModels.CollectionPermission> MapCollectionPermissions(IEnumerable<DomainModels.CollectionPermission> permissions);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ProtoCitizenModels.CollectionPermission MapCollectionPermission(DomainModels.CollectionPermission permission);

    [MapPropertyFromSource(nameof(ProtoCitizenModels.Initiative.Collection))]
    [MapperIgnoreTarget(nameof(ProtoCitizenModels.Initiative.CommitteeDescription))]
    private static partial ProtoCitizenModels.Initiative MapToInitiative(InitiativeEntity initiative);

    [MapPropertyFromSource(nameof(ProtoCitizenModels.Referendum.Collection))]
    [MapperIgnoreTarget(nameof(ProtoCitizenModels.Referendum.IsOtherReferendumOfSameDecreeSigned))]
    private static partial ProtoCitizenModels.Referendum MapToReferendum(ReferendumEntity referendum);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapperIgnoreTarget(nameof(ProtoCitizenModels.Collection.UserPermissions))]
    [MapperIgnoreTarget(nameof(ProtoCitizenModels.Collection.IsSigned))]
    [MapperIgnoreTarget(nameof(ProtoCitizenModels.Collection.SignatureType))]
    [MapperIgnoreTarget(nameof(ProtoCitizenModels.Collection.AttestedCollectionCount))]
    private static partial ProtoCitizenModels.Collection MapCollection(CollectionBaseEntity collection);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ProtoCitizenModels.CollectionUserPermissions MapUserPermissions(DomainModels.CollectionUserPermissions source);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ProtoCitizenModels.SimpleDecree MapToSimpleDecree(DecreeEntity decree);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ProtoCitizenModels.InitiativeCommitteeMemberUserPermissions MapInitiativeCommitteeMemberUserPermissions(SharedDomainModels.InitiativeCommitteeMemberUserPermissions userPermissions);

    private static Timestamp MapToDateTime(DateTime dateTime)
    {
        return dateTime.ToTimestamp();
    }

    private static DateOnly MapToDateOnlyFromTimestamp(Timestamp timestamp)
        => DateOnly.FromDateTime(timestamp.ToDateTime());

    private static Timestamp MapToTimestamp(DateOnly date)
        => date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).ToTimestamp();

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial SharedProtoModels.Date MapToDate(DateOnly date);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial SharedProtoModels.MarkdownString MapMarkdownString(MarkdownString mdStr);
}
