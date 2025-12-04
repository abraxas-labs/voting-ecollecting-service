// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Admin.Domain.Models;

public class DomainOfInfluence
{
    public string Name { get; set; } = string.Empty;

    public DomainOfInfluenceType Type { get; set; }

    public DomainOfInfluenceSettings? Settings { get; set; }

    public DomainOfInfluenceAddress? Address { get; set; }

    public string Bfs { get; set; } = string.Empty;

    public string? Webpage { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public FileEntity? Logo { get; set; }
}
