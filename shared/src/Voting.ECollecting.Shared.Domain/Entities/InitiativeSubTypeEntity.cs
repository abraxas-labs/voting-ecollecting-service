// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Lib.Database.Models;

namespace Voting.ECollecting.Shared.Domain.Entities;

public class InitiativeSubTypeEntity : BaseEntity
{
    public string Bfs { get; set; } = string.Empty;

    public DomainOfInfluenceType DomainOfInfluenceType { get; set; }

    public string Description { get; set; } = string.Empty;

    public int MinSignatureCount { get; set; }

    public int MaxElectronicSignatureCount { get; set; }
}
