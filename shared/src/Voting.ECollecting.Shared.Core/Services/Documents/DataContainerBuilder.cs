// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Core.Services.Documents.TemplateBag;
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
        var totalCitizenCount = data.Collections.Sum(x => x.CollectionCount!.TotalCitizenCount);
        var electronicCitizenCount = data.Collections.Sum(x => x.CollectionCount!.ElectronicCitizenCount);
        var totalValidPhysicalSignatureCount = totalCitizenCount - electronicCitizenCount;
        var totalInvalidPhysicalSignatureCount = data.Collections.SelectMany(x => x.Municipalities!).Sum(x => x.PhysicalCount.Invalid);
        var collectionTypeName = data.IsDecree
            ? DefaultReferendumCollectionTypeName
            : data.SubType?.Description ?? DefaultInitiativeCollectionTypeName;
        var domainOfInfluenceDataContainers = BuildDomainOfInfluenceDataContainers(data);
        return new ECollectingProtocolDataContainer(
            TemplateBagMapper.MapToString(data.DomainOfInfluenceType),
            collectionTypeName,
            data.Description,
            electronicCitizenCount,
            totalValidPhysicalSignatureCount,
            totalInvalidPhysicalSignatureCount,
            totalCitizenCount,
            domainOfInfluenceDataContainers);
    }

    public static CommitteeListTemplateBag BuildCommitteeListTemplateBag(CommitteeListTemplateData data)
    {
        return new CommitteeListTemplateBag(
            TemplateBagMapper.MapToString(data.Initiative.DomainOfInfluenceType!.Value),
            data.SubType?.Description ?? DefaultInitiativeCollectionTypeName,
            data.Initiative.Description,
            data.RequiredApprovedMembersCount,
            TemplateBagMapper.MapToCommitteeMembers(data.CommitteeMembers),
            new CollectionPermissionDataContainer(data.Initiative.AuditInfo.CreatedByName, data.Initiative.AuditInfo.CreatedByEmail ?? string.Empty),
            TemplateBagMapper.MapToCollectionPermissionDataContainers(data.CollectionDeputies));
    }

    private static List<DomainOfInfluenceDataContainer> BuildDomainOfInfluenceDataContainers(
        ECollectingProtocolTemplateData data)
    {
        var collectionMunicipalitiesByBfs = data.Collections
            .SelectMany(x => x.Municipalities!)
            .GroupBy(x => x.Bfs)
            .ToDictionary(x => x.Key, x => x.ToList());
        var doiMunicipalitiesByParent = data.AccessControlListDoi.GetFlattenChildren()
            .Where(x => x is { Type: AclDomainOfInfluenceType.Mu, Parent: not null })
            .GroupBy(x => x.Parent!)
            .OrderBy(x => x.Key.SortNumber)
            .ToDictionary(x => x.Key, x => x.ToList());

        var domainOfInfluenceDataContainers = new List<DomainOfInfluenceDataContainer>();
        foreach (var (parent, doiMunicipalities) in doiMunicipalitiesByParent)
        {
            var totalValidElectronicSignatureCount = 0;
            var totalValidPhysicalSignatureCount = 0;
            var totalInvalidPhysicalSignatureCount = 0;
            var muDomainOfInfluenceDataContainers = new List<DomainOfInfluenceDataContainer>();
            foreach (var bfs in doiMunicipalities.OrderBy(x => x.SortNumber).Select(x => x.Bfs))
            {
                if (string.IsNullOrEmpty(bfs) || !collectionMunicipalitiesByBfs.TryGetValue(bfs, out var municipalities) || municipalities.Count == 0)
                {
                    continue;
                }

                var validElectronicSignatureCount = municipalities.Sum(x => x.ElectronicCitizenCount);
                var validPhysicalSignatureCount = municipalities.Sum(x => x.PhysicalCount.Valid);
                var invalidPhysicalSignatureCount = municipalities.Sum(x => x.PhysicalCount.Invalid);

                totalValidElectronicSignatureCount += validElectronicSignatureCount;
                totalValidPhysicalSignatureCount += validPhysicalSignatureCount;
                totalInvalidPhysicalSignatureCount += invalidPhysicalSignatureCount;

                muDomainOfInfluenceDataContainers.Add(
                    new DomainOfInfluenceDataContainer(
                    municipalities[0].MunicipalityName,
                    validElectronicSignatureCount,
                    validPhysicalSignatureCount,
                    invalidPhysicalSignatureCount));
            }

            if (muDomainOfInfluenceDataContainers.Count == 0)
            {
                // empty parent doi should not be added
                continue;
            }

            var domainOfInfluenceDataContainer = new DomainOfInfluenceDataContainer(
                parent.Name,
                totalValidElectronicSignatureCount,
                totalValidPhysicalSignatureCount,
                totalInvalidPhysicalSignatureCount)
            { DomainOfInfluences = muDomainOfInfluenceDataContainers, };

            domainOfInfluenceDataContainers.Add(domainOfInfluenceDataContainer);
        }

        return domainOfInfluenceDataContainers;
    }
}
