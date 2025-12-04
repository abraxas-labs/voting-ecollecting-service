// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities.Audit;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Shared.Domain.Entities;

public class DomainOfInfluenceEntity : AuditedEntity, IHasBfs
{
    public string Bfs { get; set; } = string.Empty;

    public DomainOfInfluenceType Type { get; set; }

    public Guid? LogoId { get; set; }

    public FileEntity? Logo { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;

    public string ZipCode { get; set; } = string.Empty;

    public string Locality { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Webpage { get; set; }
}
