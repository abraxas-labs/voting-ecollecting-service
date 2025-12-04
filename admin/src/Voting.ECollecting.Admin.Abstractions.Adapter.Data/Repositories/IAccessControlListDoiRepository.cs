// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Admin.Abstractions.Adapter.Data.Repositories;

/// <summary>
/// Repository for domain of influences based access control list.
/// </summary>
public interface IAccessControlListDoiRepository : Shared.Abstractions.Adapter.Data.Repositories.IAccessControlListDoiRepository
{
    Task<string> GetMunicipalityNameByBfs(AclDomainOfInfluenceType doiType, string bfs);

    Task<string> GetSingleBfsForDoiType(AclBfsLists aclBfsLists, AclDomainOfInfluenceType doiType);

    Task<AccessControlListDoiEntity> GetSingleForDoiType(AclBfsLists aclBfsLists, AclDomainOfInfluenceType doiType);
}
