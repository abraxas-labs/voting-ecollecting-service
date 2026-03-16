// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Diagnostics.CodeAnalysis;
using Voting.ECollecting.Shared.Domain.Entities.Audit;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Shared.Domain.Entities;

public class DomainOfInfluenceEntity : AuditedEntity, IHasBfs
{
    /// <summary>
    /// Gets or sets the name of the DOI.
    /// Imported from Basis.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the BFS code of the DOI.
    /// Imported from Basis.
    /// </summary>
    public string? Bfs { get; set; }

    /// <summary>
    /// Gets or sets the name of the assigned tenant.
    /// Imported from Basis.
    /// </summary>
    public string TenantName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant id of the assigned tenant.
    /// Imported from Basis.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the basis domain of influence type.
    /// This includes much more types than supported by eCollecting (<seealso cref="DomainOfInfluenceType"/>).
    /// Imported from Basis.
    /// </summary>
    public BasisDomainOfInfluenceType BasisType { get; set; }

    /// <summary>
    /// Gets or sets the domain of influence type.
    /// Imported from Basis and mapped from basis.
    /// For all unknown types, <seealso cref="DomainOfInfluenceType.Unspecified"/> is set.
    /// </summary>
    public DomainOfInfluenceType Type { get; set; }

    /// <summary>
    /// Gets or sets the canton of the DOI.
    /// Imported from Basis.
    /// </summary>
    public Canton Canton { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether whether all import validation rules succeeded.
    /// Set during the import.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Bfs))]
    public bool IsValid { get; set; }

    /// <summary>
    /// Gets or sets the validation errors. Only set if <see cref="IsValid"/> is false.
    /// Set during the import.
    /// </summary>
    public string? ValidationErrors { get; set; }

    /// <summary>
    /// Gets or sets the parent identifier of the current DOI within the hierarchical tree.
    /// Imported from Basis.
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Gets or sets the parent DOI within the hierarchical tree. Is null if the current DOI is a root node.
    /// Imported from Basis.
    /// </summary>
    public DomainOfInfluenceEntity? Parent { get; set; }

    /// <summary>
    /// Gets or sets the id of the last modifying import statistic.
    /// </summary>
    public Guid? ImportStatisticId { get; set; }

    /// <summary>
    /// Gets or sets the last modifying import statistic.
    /// </summary>
    public ImportStatisticEntity? ImportStatistic { get; set; }

    /// <summary>
    /// Gets or sets the children DOI within the hierarchical tree.
    /// Empty if the current DOI is a leaf node, otherwise it may have one to many children.
    /// Imported from Basis.
    /// </summary>
    public ICollection<DomainOfInfluenceEntity> Children { get; set; } = new HashSet<DomainOfInfluenceEntity>();

    /// <summary>
    /// Gets or sets a value indicating whether whether eCollecting is enabled for this DOI.
    /// If set to true, electronic citizen signatures are allowed.
    /// If set to false eCollecting is only used for physical signatures.
    /// Imported from Basis.
    /// </summary>
    public bool ECollectingEnabled { get; set; }

    /// <summary>
    /// Gets or sets the sort number.
    /// Imported from Basis.
    /// </summary>
    public int SortNumber { get; set; }

    /// <summary>
    /// Gets or sets the name for protocol.
    /// Imported from Basis.
    /// </summary>
    public string NameForProtocol { get; set; } = string.Empty;

    public Guid? LogoId { get; set; }

    public FileEntity? Logo { get; set; }

    public string AddressName { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;

    public string ZipCode { get; set; } = string.Empty;

    public string Locality { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public List<string> NotificationEmails { get; set; } = [];

    public string? Webpage { get; set; }

    public int? InitiativeMinSignatureCount { get; set; }

    public int? InitiativeMaxElectronicSignaturePercent { get; set; }

    public int? InitiativeNumberOfMembersCommittee { get; set; }

    public int? ReferendumMinSignatureCount { get; set; }

    public int? ReferendumMaxElectronicSignaturePercent { get; set; }

    public int GetMaxInitiativeElectronicSignatureCount(int minSignatureCount)
    {
        if (Type == DomainOfInfluenceType.Mu)
        {
            throw new InvalidOperationException("Cannot get max signature count for municipality. Need to use the quorum of the canton.");
        }

        return (int)Math.Round(minSignatureCount * (InitiativeMaxElectronicSignaturePercent.GetValueOrDefault() / 100.0));
    }

    public int GetMaxReferendumElectronicSignatureCount(int minSignatureCount)
    {
        if (Type == DomainOfInfluenceType.Mu)
        {
            throw new InvalidOperationException("Cannot get max signature count for municipality. Need to use the quorum of the canton.");
        }

        return (int)Math.Round(minSignatureCount * (ReferendumMaxElectronicSignaturePercent.GetValueOrDefault() / 100.0));
    }
}
