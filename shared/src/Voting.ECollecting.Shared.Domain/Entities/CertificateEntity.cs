// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities.Audit;

namespace Voting.ECollecting.Shared.Domain.Entities;

public class CertificateEntity : AuditedEntity, IAuditTrailTrackedEntity
{
    public string? Label { get; set; }

    public bool Active { get; set; }

    public FileEntity? Content { get; set; }

    public Guid ContentId { get; set; }

    public CertificateInfo? Info { get; set; }

    public CertificateInfo? CAInfo { get; set; }
}
