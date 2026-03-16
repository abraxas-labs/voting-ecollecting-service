// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Abstractions.Core.Services;

public interface IDomainOfInfluenceService
{
    Task<DomainOfInfluenceEntity> GetWithChildren(string bfs);

    Task<List<DomainOfInfluenceEntity>> GetTree();
}
