// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Riok.Mapperly.Abstractions;
using Voting.ECollecting.Citizen.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Models;
using DomainOfInfluence = Voting.ECollecting.Citizen.Domain.Models.DomainOfInfluence;

namespace Voting.ECollecting.Citizen.Core.Mappings;

[Mapper]
internal static partial class Mapper
{
    internal static partial IEnumerable<DomainOfInfluence> MapToDomainOfInfluences(IEnumerable<AccessControlListDoiEntity> accessControlListDoiEntities);

    internal static partial List<Initiative> MapToInitiatives(IEnumerable<InitiativeEntity> initiativeEntities);

    [MapperIgnoreTarget(nameof(Initiative.UserPermissions))]
    [MapperIgnoreTarget(nameof(Initiative.IsSigned))]
    [MapperIgnoreTarget(nameof(Initiative.SignAcceptedAcrs))]
    [MapperIgnoreTarget(nameof(Initiative.CommitteeDescription))]
    [MapperIgnoreTarget(nameof(Initiative.AttestedCollectionCount))]
    internal static partial Initiative MapToInitiative(InitiativeEntity initiativeEntity);

    internal static partial List<Decree> MapToDecrees(IEnumerable<DecreeEntity> decreeEntities);

    [UserMapping(Default = true)]
    internal static Decree MapToDecree(DecreeEntity decreeEntity)
    {
        var decree = MapToDecreeInternal(decreeEntity);

        // set correct decree references
        foreach (var referendum in decree.Referendums)
        {
            referendum.Decree = decree;
        }

        return decree;
    }

    internal static partial List<Referendum> MapToReferendums(IEnumerable<ReferendumEntity> referendumEntities);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapperIgnoreTarget(nameof(Referendum.UserPermissions))]
    [MapperIgnoreTarget(nameof(Referendum.IsSigned))]
    [MapperIgnoreTarget(nameof(Referendum.SignAcceptedAcrs))]
    [MapperIgnoreTarget(nameof(Referendum.IsDecreeSigned))]
    [MapperIgnoreTarget(nameof(Referendum.AttestedCollectionCount))]
    internal static partial Referendum MapToReferendum(ReferendumEntity referendumEntity);

    [MapperIgnoreTarget(nameof(InitiativeCommitteeMemberEntity.AuditInfo))]
    [MapperIgnoreTarget(nameof(InitiativeCommitteeMemberEntity.InitiativeId))]
    [MapperIgnoreTarget(nameof(InitiativeCommitteeMemberEntity.Initiative))]
    [MapperIgnoreTarget(nameof(InitiativeCommitteeMemberEntity.Id))]
    [MapperIgnoreTarget(nameof(InitiativeCommitteeMemberEntity.Permission))]
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial void ApplyUpdate(
        InitiativeCommitteeMemberEntity member,
        InitiativeCommitteeMemberEntity existingMember);

    [MapValue(nameof(CollectionPermissionEntity.IamFirstName), "")]
    [MapValue(nameof(CollectionPermissionEntity.IamLastName), "")]
    [MapValue(nameof(CollectionPermissionEntity.IamUserId), "")]
    [MapperIgnoreTarget(nameof(CollectionPermissionEntity.Token))]
    [MapperIgnoreTarget(nameof(CollectionPermissionEntity.State))]
    [MapperIgnoreTarget(nameof(CollectionPermissionEntity.IntegritySignatureInfo))]
    [MapperIgnoreTarget(nameof(CollectionPermissionEntity.Id))]
    [MapperIgnoreTarget(nameof(CollectionPermissionEntity.Role))]
    [MapperIgnoreTarget(nameof(CollectionPermissionEntity.CollectionId))]
    [MapperIgnoreTarget(nameof(CollectionPermissionEntity.Collection))]
    [MapperIgnoreTarget(nameof(CollectionPermissionEntity.InitiativeCommitteeMember))]
    [MapperIgnoreTarget(nameof(CollectionPermissionEntity.InitiativeCommitteeMemberId))]
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    internal static partial void ApplyUpdate(
        InitiativeCommitteeMemberEntity member,
        CollectionPermissionEntity? permission);

    internal static partial IEnumerable<AclDomainOfInfluenceType> MapToAclDoiTypes(IEnumerable<DomainOfInfluenceType> doiTypes);

    [MapperIgnoreTarget(nameof(InitiativeCommitteeMember.Residence))]
    [MapperIgnoreTarget(nameof(InitiativeCommitteeMember.PoliticalResidence))]
    internal static partial InitiativeCommitteeMember MapToInitiativeCommitteeMember(InitiativeCommitteeMemberEntity initiativeCommitteeMember);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(AccessControlListDoiEntity.ECollectingInitiativeMinSignatureCount), nameof(DomainOfInfluence.InitiativeMinSignatureCount))]
    [MapProperty(nameof(AccessControlListDoiEntity.ECollectingInitiativeMaxElectronicSignaturePercent), nameof(DomainOfInfluence.InitiativeMaxElectronicSignaturePercent))]
    private static partial DomainOfInfluence MapToDomainOfInfluence(AccessControlListDoiEntity accessControlListDoiEntity);

    [MapEnum(EnumMappingStrategy.ByName)]
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    private static partial AclDomainOfInfluenceType MapToDomainOfInfluenceType(DomainOfInfluenceType aclDomainOfInfluenceType);

    [MapEnum(EnumMappingStrategy.ByName)]
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial DomainOfInfluenceType MapToDomainOfInfluenceType(AclDomainOfInfluenceType aclDomainOfInfluenceType);

    [MapProperty(nameof(DecreeEntity.Collections), nameof(Decree.Referendums))]
    [MapperIgnoreTarget(nameof(Initiative.UserPermissions))]
    private static partial Decree MapToDecreeInternal(DecreeEntity decreeEntity);
}
