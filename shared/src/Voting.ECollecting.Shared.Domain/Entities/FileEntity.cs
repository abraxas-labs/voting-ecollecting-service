// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities.Audit;

namespace Voting.ECollecting.Shared.Domain.Entities;

public class FileEntity : AuditedEntity
{
    public string Name { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public FileContentEntity? Content { get; set; }

    public CollectionBaseEntity? ImageOfCollection { get; set; }

    public CollectionBaseEntity? LogoOfCollection { get; set; }

    public CollectionBaseEntity? SignatureSheetOfCollection { get; set; }

    public InitiativeCommitteeMemberEntity? SignatureSheetOfInitiativeCommitteeMember { get; set; }

    public CertificateEntity? ContentOfCertificate { get; set; }

    public InitiativeEntity? CommitteeListOfInitiative { get; set; }

    public Guid? CommitteeListOfInitiativeId { get; set; }

    public DomainOfInfluenceEntity? LogoOfDomainOfInfluence { get; set; }
}
