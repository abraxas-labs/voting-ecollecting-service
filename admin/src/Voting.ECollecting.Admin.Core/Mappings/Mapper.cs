// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Riok.Mapperly.Abstractions;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Models;
using Voting.Lib.Iam.SecondFactor.Models;

namespace Voting.ECollecting.Admin.Core.Mappings;

[Mapper]
internal static partial class Mapper
{
    internal static partial SecondFactorTransaction MapToSecondFactorTransaction(
        SecondFactorTransactionEntity transaction);

    internal static partial SecondFactorTransactionEntity MapToSecondFactorTransaction(
        SecondFactorTransaction transaction);

    internal static partial List<Decree> MapToDecrees(IEnumerable<DecreeEntity> decreeEntities);

    [MapperIgnoreTarget(nameof(Decree.UserPermissions))]
    [MapProperty(nameof(DecreeEntity.Collections), nameof(Decree.Referendums))]
    internal static partial Decree MapToDecree(DecreeEntity decreeEntity);

    [MapperIgnoreTarget(nameof(CollectionSignatureSheet.UserPermissions))]
    internal static partial List<CollectionSignatureSheet> MapToCollectionSignatureSheets(IEnumerable<CollectionSignatureSheetEntity> entities);

    [MapperIgnoreTarget(nameof(CollectionSignatureSheet.UserPermissions))]
    internal static partial CollectionSignatureSheet MapToCollectionSignatureSheet(CollectionSignatureSheetEntity entity);

    internal static partial IEnumerable<DomainOfInfluence> MapToDomainOfInfluences(IEnumerable<AccessControlListDoiEntity> accessControlListDoiEntities);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(DomainOfInfluenceEntity.Id))]
    [MapperIgnoreSource(nameof(DomainOfInfluenceEntity.AuditInfo))]
    [MapperIgnoreSource(nameof(DomainOfInfluenceEntity.LogoId))]
    [MapPropertyFromSource(nameof(DomainOfInfluence.Address))]
    internal static partial void MapToDomainOfInfluence(
        DomainOfInfluenceEntity doiEntity,
        [MappingTarget] DomainOfInfluence domainOfInfluence);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapperIgnoreTarget(nameof(DomainOfInfluence.Email))]
    [MapperIgnoreTarget(nameof(DomainOfInfluence.Phone))]
    [MapperIgnoreTarget(nameof(DomainOfInfluence.Webpage))]
    [MapperIgnoreTarget(nameof(DomainOfInfluence.Address))]
    [MapperIgnoreTarget(nameof(DomainOfInfluence.Logo))]
    [MapPropertyFromSource(nameof(DomainOfInfluence.Settings))]
    internal static partial DomainOfInfluence MapToDomainOfInfluence(AccessControlListDoiEntity accessControlListDoiEntity);

    internal static partial List<Initiative> MapToInitiatives(IEnumerable<InitiativeEntity> initiativeEntity);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapperIgnoreTarget(nameof(Referendum.UserPermissions))]
    [MapperIgnoreTarget(nameof(Referendum.AttestedCollectionCount))]
    internal static partial Referendum MapToReferendum(ReferendumEntity referendumEntity);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapperIgnoreTarget(nameof(Initiative.UserPermissions))]
    [MapperIgnoreTarget(nameof(Initiative.AttestedCollectionCount))]
    internal static partial Initiative MapToInitiative(InitiativeEntity initiativeEntity);

    internal static partial List<CollectionPermission> MapToCollectionPermissions(IEnumerable<CollectionPermissionEntity> collectionPermissionEntities);

    [MapperIgnoreTarget(nameof(InitiativeCommitteeMember.Residence))]
    [MapperIgnoreTarget(nameof(InitiativeCommitteeMember.PoliticalResidence))]
    internal static partial InitiativeCommitteeMember MapToInitiativeCommitteeMember(InitiativeCommitteeMemberEntity initiativeCommitteeMember);

    internal static partial IEnumerable<AclDomainOfInfluenceType> MapToAclDoiTypes(IEnumerable<DomainOfInfluenceType> doiTypes);

    internal static partial IEnumerable<DomainOfInfluenceType> MapToDoiTypes(IEnumerable<AclDomainOfInfluenceType> doiTypes);

    [MapEnum(EnumMappingStrategy.ByName)]
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial DomainOfInfluenceType MapToDoiType(AclDomainOfInfluenceType aclDomainOfInfluenceType);

    [MapEnum(EnumMappingStrategy.ByName)]
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial AclDomainOfInfluenceType MapToAclDomainOfInfluenceType(DomainOfInfluenceType doiType);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void UpdateDomainOfInfluence(
        UpdateDomainOfInfluenceRequest updateRequest,
        [MappingTarget] DomainOfInfluenceEntity domainOfInfluenceEntity);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial CollectionPermission MapToCollectionPermission(CollectionPermissionEntity collectionPermissionEntity);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(AccessControlListDoiEntity.ECollectingInitiativeMaxElectronicSignaturePercent), nameof(DomainOfInfluenceSettings.InitiativeMaxElectronicSignaturePercent))]
    [MapProperty(nameof(AccessControlListDoiEntity.ECollectingInitiativeMinSignatureCount), nameof(DomainOfInfluenceSettings.InitiativeMinSignatureCount))]
    [MapProperty(nameof(AccessControlListDoiEntity.ECollectingInitiativeNumberOfMembersCommittee), nameof(DomainOfInfluenceSettings.InitiativeNumberOfMembersCommittee))]
    [MapProperty(nameof(AccessControlListDoiEntity.ECollectingReferendumMaxElectronicSignaturePercent), nameof(DomainOfInfluenceSettings.ReferendumMaxElectronicSignaturePercent))]
    [MapProperty(nameof(AccessControlListDoiEntity.ECollectingReferendumMinSignatureCount), nameof(DomainOfInfluenceSettings.ReferendumMinSignatureCount))]
    private static partial DomainOfInfluenceSettings MapToDomainOfInfluenceSettings(AccessControlListDoiEntity accessControlListDoiEntity);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial DomainOfInfluenceAddress MapToDomainOfInfluenceSettings(DomainOfInfluenceEntity doi);
}
