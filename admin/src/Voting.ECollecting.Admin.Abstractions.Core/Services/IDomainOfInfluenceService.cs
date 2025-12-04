// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Admin.Abstractions.Core.Services;

public interface IDomainOfInfluenceService
{
    Task<List<DomainOfInfluence>> List(
        bool? eCollectingEnabled,
        IReadOnlySet<DomainOfInfluenceType>? doiTypes,
        bool includeChildren);

    Task<List<DomainOfInfluenceType>> ListOwnTypes();

    Task<DomainOfInfluence> Get(string bfs);

    Task Update(string bfs, UpdateDomainOfInfluenceRequest updateRequest);
}
