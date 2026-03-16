// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories;

public interface IDomainOfInfluenceRepository : Shared.Abstractions.Adapter.Data.Repositories.IDomainOfInfluenceRepository
{
    Task<DomainOfInfluenceEntity> GetSingleByType(DomainOfInfluenceType type);

    Task<string> GetSingleBfsByType(DomainOfInfluenceType type);
}
