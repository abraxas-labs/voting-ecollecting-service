// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Domain.Models;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Abstractions.Core.Services;

public interface IDomainOfInfluenceService
{
    Task<List<DomainOfInfluence>> List(bool? eCollectingEnabled, IReadOnlySet<DomainOfInfluenceType>? doiTypes);

    IEnumerable<DomainOfInfluenceType> ListDomainOfInfluenceTypes();
}
