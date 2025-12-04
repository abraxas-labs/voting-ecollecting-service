// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Database.Models;

namespace Voting.ECollecting.Shared.Domain.Entities.Audit;

/// <summary>
/// An entity with an <see cref="AuditInfo"/>.
/// </summary>
public abstract class AuditedEntity : BaseEntity, IAuditedEntity
{
    /// <summary>
    /// Gets or sets the audit info of this entity.
    /// </summary>
    public AuditInfo AuditInfo { get; set; } = new();
}
