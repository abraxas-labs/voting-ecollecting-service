// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities.Audit;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Shared.Domain.Entities;

public class AccessControlListDoiEntity : AuditedEntity, IHasBfs
{
    public string Name { get; set; } = string.Empty;

    public string? Bfs { get; set; }

    public string TenantName { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public AclDomainOfInfluenceType Type { get; set; }

    public Canton Canton { get; set; }

    public bool IsValid { get; set; }

    public string? ValidationErrors { get; set; }

    /// <summary>
    /// Gets or sets the parent identifier of the current DOI within the hierarchical tree.
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Gets or sets the parent DOI within the hierarchical tree. Is null if the current DOI is a root node.
    /// </summary>
    public AccessControlListDoiEntity? Parent { get; set; }

    public Guid? ImportStatisticId { get; set; }

    public ImportStatisticEntity? ImportStatistic { get; set; }

    /// <summary>
    /// Gets or sets the children DOI within the hierarchical tree.
    /// Empty if the current DOI is a leaf node, otherwise it may have one to many children.
    /// </summary>
    public ICollection<AccessControlListDoiEntity> Children { get; set; } = new HashSet<AccessControlListDoiEntity>();

    public bool ECollectingEnabled { get; set; }

    public int? ECollectingInitiativeMinSignatureCount { get; set; }

    public int? ECollectingInitiativeMaxElectronicSignaturePercent { get; set; }

    public int? ECollectingInitiativeNumberOfMembersCommittee { get; set; }

    public int? ECollectingReferendumMinSignatureCount { get; set; }

    public int? ECollectingReferendumMaxElectronicSignaturePercent { get; set; }

    public string ECollectingEmail { get; set; } = string.Empty;

    public int SortNumber { get; set; }

    public string NameForProtocol { get; set; } = string.Empty;
}
