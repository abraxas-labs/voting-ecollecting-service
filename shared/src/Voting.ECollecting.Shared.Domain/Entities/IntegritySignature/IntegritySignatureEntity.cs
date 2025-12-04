// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Riok.Mapperly.Abstractions;
using Voting.ECollecting.Shared.Domain.Entities.Audit;
using Voting.Lib.Database.Models;

namespace Voting.ECollecting.Shared.Domain.Entities.IntegritySignature;

public abstract class IntegritySignatureEntity : BaseEntity, IIntegritySignatureEntity, IAuditedEntity, IAuditTrailTrackedEntity
{
    [MapperIgnore]
    public IntegritySignatureInfo IntegritySignatureInfo { get; set; } = new();

    [MapperIgnore]
    public AuditInfo AuditInfo { get; set; } = new();
}
