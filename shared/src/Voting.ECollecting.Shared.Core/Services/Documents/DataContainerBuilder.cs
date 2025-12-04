// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Core.Services.Documents.TemplateBag;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Domain.Models;

namespace Voting.ECollecting.Shared.Core.Services.Documents;

public static class DataContainerBuilder
{
    private const string DefaultInitiativeCollectionTypeName = "Volksinitiative";
    private const string DefaultReferendumCollectionTypeName = "Referendum";

    public static ECollectingProtocolDataContainer BuildProtocolDataContainer(ECollectingProtocolTemplateData data)
    {
        var totalValidPhysicalSignatureCount = data.Collection.CollectionCount!.TotalCitizenCount -
                                               data.Collection.CollectionCount.ElectronicCitizenCount;
        var totalInvalidPhysicalSignatureCount = data.Collection.Municipalities!.Sum(x => x.PhysicalCount.Invalid);
        var collectionTypeName = GetCollectionTypeName(data.Collection, data.SubType);
        var domainOfInfluenceDataContainers = BuildDomainOfInfluenceDataContainers(data);
        return new ECollectingProtocolDataContainer(
            TemplateBagMapper.MapToString(data.Collection.DomainOfInfluenceType!.Value),
            collectionTypeName,
            data.Collection.Description,
            data.Collection.CollectionCount.ElectronicCitizenCount,
            totalValidPhysicalSignatureCount,
            totalInvalidPhysicalSignatureCount,
            data.Collection.CollectionCount.TotalCitizenCount,
            domainOfInfluenceDataContainers);
    }

    public static CommitteeListTemplateBag BuildCommitteeListTemplateBag(CommitteeListTemplateData data)
    {
        return new CommitteeListTemplateBag(
            TemplateBagMapper.MapToString(data.Initiative.DomainOfInfluenceType!.Value),
            GetCollectionTypeName(data.Initiative, data.SubType),
            data.Initiative.Description,
            data.RequiredApprovedMembersCount,
            TemplateBagMapper.MapToCommitteeMembers(data.CommitteeMembers),
            new CollectionPermissionDataContainer(data.Initiative.AuditInfo.CreatedByName, data.Initiative.AuditInfo.CreatedByEmail ?? string.Empty),
            TemplateBagMapper.MapToCollectionPermissionDataContainers(data.CollectionDeputies));
    }

    private static List<DomainOfInfluenceDataContainer> BuildDomainOfInfluenceDataContainers(
        ECollectingProtocolTemplateData data)
    {
        var collectionMunicipalityByBfs = data.Collection.Municipalities!.ToDictionary(x => x.Bfs);
        var doiMunicipalitiesByParent = data.AccessControlListDoi.GetFlattenChildren()
            .Where(x => x is { Type: AclDomainOfInfluenceType.Mu, Parent: not null })
            .GroupBy(x => x.Parent!)
            .OrderBy(x => x.Key.SortNumber)
            .ToDictionary(x => x.Key, x => x.ToList());

        var domainOfInfluenceDataContainers = new List<DomainOfInfluenceDataContainer>();
        foreach (var (parent, doiMunicipalities) in doiMunicipalitiesByParent)
        {
            var validElectronicSignatureCount = 0;
            var validPhysicalSignatureCount = 0;
            var invalidPhysicalSignatureCount = 0;
            var muDomainOfInfluenceDataContainers = new List<DomainOfInfluenceDataContainer>();
            foreach (var bfs in doiMunicipalities.OrderBy(x => x.SortNumber).Select(x => x.Bfs))
            {
                if (string.IsNullOrEmpty(bfs) || !collectionMunicipalityByBfs.TryGetValue(bfs, out var municipality))
                {
                    continue;
                }

                validElectronicSignatureCount += municipality.ElectronicCitizenCount;
                validPhysicalSignatureCount += municipality.PhysicalCount.Valid;
                invalidPhysicalSignatureCount += municipality.PhysicalCount.Invalid;
                muDomainOfInfluenceDataContainers.Add(
                    TemplateBagMapper.MapToDomainOfInfluenceDataContainer(municipality));
            }

            if (muDomainOfInfluenceDataContainers.Count == 0)
            {
                // empty parent doi should not be added
                continue;
            }

            var domainOfInfluenceDataContainer = new DomainOfInfluenceDataContainer(
                parent.Name,
                validElectronicSignatureCount,
                validPhysicalSignatureCount,
                invalidPhysicalSignatureCount)
            { DomainOfInfluences = muDomainOfInfluenceDataContainers, };

            domainOfInfluenceDataContainers.Add(domainOfInfluenceDataContainer);
        }

        return domainOfInfluenceDataContainers;
    }

    private static string GetCollectionTypeName(CollectionBaseEntity collection, InitiativeSubTypeEntity? subType)
    {
        return collection.Type switch
        {
            CollectionType.Initiative => subType?.Description ?? DefaultInitiativeCollectionTypeName,
            CollectionType.Referendum => DefaultReferendumCollectionTypeName,
            _ => throw new InvalidOperationException($"Unexpected collection type: {collection.Type}"),
        };
    }
}
