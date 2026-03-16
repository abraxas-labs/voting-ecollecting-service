// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Domain.Models;

namespace Voting.ECollecting.Admin.Abstractions.Core.Services;

public interface IAccessControlListService
{
    Task EnsureIsCtOrChTenant();

    Task<AclBfsLists> GetBfsNumberAccessControlListsByTenantId(string tenantId);
}
