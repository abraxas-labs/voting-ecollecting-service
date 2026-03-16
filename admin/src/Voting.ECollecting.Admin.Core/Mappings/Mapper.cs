// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Riok.Mapperly.Abstractions;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
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
    [MapperIgnoreTarget(nameof(Initiative.DomainOfInfluenceName))]
    internal static partial Decree MapToDecree(DecreeEntity decreeEntity);

    [MapperIgnoreTarget(nameof(CollectionSignatureSheet.UserPermissions))]
    internal static partial List<CollectionSignatureSheet> MapToCollectionSignatureSheets(IEnumerable<CollectionSignatureSheetEntity> entities);

    [MapperIgnoreTarget(nameof(CollectionSignatureSheet.UserPermissions))]
    internal static partial CollectionSignatureSheet MapToCollectionSignatureSheet(CollectionSignatureSheetEntity entity);

    internal static partial IEnumerable<DomainOfInfluence> MapToDomainOfInfluences(IEnumerable<DomainOfInfluenceEntity> accessControlListDoiEntities);

    internal static void MapToDomainOfInfluence(
        DomainOfInfluenceEntity doiEntity,
        DomainOfInfluence domainOfInfluence)
    {
        var nameForProtocol = domainOfInfluence.NameForProtocol;
        MapToDomainOfInfluenceInternal(doiEntity, domainOfInfluence);

        if (string.IsNullOrEmpty(nameForProtocol))
        {
            domainOfInfluence.NameForProtocol = nameForProtocol;
        }
    }

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapperIgnoreTarget(nameof(DomainOfInfluence.UserPermissions))]
    internal static partial DomainOfInfluence MapToDomainOfInfluence(DomainOfInfluenceEntity domainOfInfluenceEntity);

    internal static partial List<Initiative> MapToInitiatives(IEnumerable<InitiativeEntity> initiativeEntity);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapperIgnoreTarget(nameof(Referendum.UserPermissions))]
    [MapperIgnoreTarget(nameof(Referendum.AttestedCollectionCount))]
    internal static partial Referendum MapToReferendum(ReferendumEntity referendumEntity);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapperIgnoreTarget(nameof(Initiative.UserPermissions))]
    [MapperIgnoreTarget(nameof(Initiative.AttestedCollectionCount))]
    [MapperIgnoreTarget(nameof(Initiative.DomainOfInfluenceName))]
    internal static partial Initiative MapToInitiative(InitiativeEntity initiativeEntity);

    internal static partial List<CollectionPermission> MapToCollectionPermissions(IEnumerable<CollectionPermissionEntity> collectionPermissionEntities);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapNestedProperties(nameof(UpdateDomainOfInfluenceRequest.Settings))]
    internal static partial void UpdateDomainOfInfluence(
        UpdateDomainOfInfluenceRequest updateRequest,
        [MappingTarget] DomainOfInfluenceEntity domainOfInfluenceEntity);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(DomainOfInfluenceEntity.Id))]
    [MapperIgnoreSource(nameof(DomainOfInfluenceEntity.AuditInfo))]
    [MapperIgnoreSource(nameof(DomainOfInfluenceEntity.LogoId))]
    private static partial void MapToDomainOfInfluenceInternal(
        DomainOfInfluenceEntity doiEntity,
        [MappingTarget] DomainOfInfluence domainOfInfluence);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial CollectionPermission MapToCollectionPermission(CollectionPermissionEntity collectionPermissionEntity);
}
