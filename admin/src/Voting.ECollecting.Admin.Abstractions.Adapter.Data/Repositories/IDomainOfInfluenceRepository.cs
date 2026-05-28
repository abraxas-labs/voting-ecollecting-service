// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;

public interface IDomainOfInfluenceRepository
    : Shared.Abstractions.Adapter.Data.Repositories.IDomainOfInfluenceRepository
{
    Task<string> GetSingleBfsByType(
        AclBfsLists aclBfsLists,
        DomainOfInfluenceType type);

    Task<DomainOfInfluenceEntity> GetSingleByBfs(string bfs, DomainOfInfluenceType type);

    Task<DomainOfInfluenceEntity> GetSingleByType(
        AclBfsLists aclBfsLists,
        DomainOfInfluenceType type);

    Task<DomainOfInfluenceEntity> GetSingleWithLogoContentsByType(AclBfsLists aclBfsLists, DomainOfInfluenceType type);

    Task<DomainOfInfluenceEntity> GetCanton();

    Task<IReadOnlyDictionary<(DomainOfInfluenceType Type, string Bfs), DomainOfInfluenceEntity>> GetByTypeAndBfs(IReadOnlySet<string> bfs);
}
