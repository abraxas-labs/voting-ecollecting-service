// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Abstractions.Core.Services;

public interface IAccessControlListDoiService
{
    Task<AclBfsLists> GetBfsNumberAccessControlListsByTenantId(string tenantId);

    Task<List<AccessControlListDoiEntity>> GetMunicipalities(string bfs);
}
