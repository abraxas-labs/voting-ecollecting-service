// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Domain.Models;

public class DomainOfInfluence : DomainOfInfluenceEntity
{
    public DomainOfInfluenceUserPermissions? UserPermissions { get; set; }
}
