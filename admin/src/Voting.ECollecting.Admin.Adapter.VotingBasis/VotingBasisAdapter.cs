// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Basis.Services.V1;
using Abraxas.Voting.Basis.Services.V1.Models;
using Abraxas.Voting.Basis.Shared.V1;
using Voting.ECollecting.Admin.Abstractions.Adapter.VotingBasis;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Admin.Adapter.VotingBasis;

/// <summary>
/// Domain of influence access control list service from VOTING Basis.
/// </summary>
public class VotingBasisAdapter : IVotingBasisAdapter
{
    private readonly AdminManagementService.AdminManagementServiceClient _votingBasisServiceClient;

    public VotingBasisAdapter(AdminManagementService.AdminManagementServiceClient votingBasisServiceClient)
    {
        _votingBasisServiceClient = votingBasisServiceClient;
    }

    public async Task<IEnumerable<DomainOfInfluenceEntity>> GetAccessControlList(Guid? importStatisticId)
    {
        return await GetFlattenDomainOfInfluenceListFromVotingBasis(importStatisticId);
    }

    private static DomainOfInfluenceEntity MapToEntity(
        PoliticalDomainOfInfluence serviceModel,
        DomainOfInfluenceEntity entity,
        Guid? importStatisticId)
    {
        entity.Id = Guid.Parse(serviceModel.Id);
        entity.Name = serviceModel.Name;
        entity.Bfs = string.IsNullOrWhiteSpace(serviceModel.Bfs) ? null : serviceModel.Bfs;
        entity.TenantName = serviceModel.TenantName;
        entity.TenantId = serviceModel.TenantId;
        entity.BasisType = Mapper.MapToBasisDomainOfInfluenceType(serviceModel.Type);
        entity.Type = Mapper.MapToDomainOfInfluenceType(serviceModel.Type);
        entity.Canton = serviceModel.Canton == DomainOfInfluenceCanton.Unspecified ?
            Canton.Unknown :
            Enum.Parse<Canton>(serviceModel.Canton.ToString(), ignoreCase: true);
        entity.ParentId = string.IsNullOrWhiteSpace(serviceModel.ParentId) ? null : Guid.Parse(serviceModel.ParentId);
        entity.ImportStatisticId = importStatisticId;
        entity.ECollectingEnabled = serviceModel.ECollectingEnabled;
        entity.SortNumber = serviceModel.SortNumber;
        entity.NameForProtocol = serviceModel.NameForProtocol;

        return entity;
    }

    private async Task<IEnumerable<DomainOfInfluenceEntity>> GetFlattenDomainOfInfluenceListFromVotingBasis(Guid? importStatisticId)
    {
        var result = await _votingBasisServiceClient.GetPoliticalDomainOfInfluenceHierarchyAsync(new());

        return result.PoliticalDomainOfInfluences
            .SelectMany(GetFlattenChildrenInclSelf)
            .Select(doi => MapToEntity(doi, new(), importStatisticId));
    }

    private IEnumerable<PoliticalDomainOfInfluence> GetFlattenChildrenInclSelf(PoliticalDomainOfInfluence doi)
    {
        yield return doi;
        foreach (var childDoi in doi.Children.SelectMany(GetFlattenChildrenInclSelf))
        {
            yield return childDoi;
        }
    }
}
