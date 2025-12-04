// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories;

public interface IAccessControlListDoiRepository : Shared.Abstractions.Adapter.Data.Repositories.IAccessControlListDoiRepository
{
    Task<string> GetSingleBfsForDoiType(AclDomainOfInfluenceType doiType);
}
