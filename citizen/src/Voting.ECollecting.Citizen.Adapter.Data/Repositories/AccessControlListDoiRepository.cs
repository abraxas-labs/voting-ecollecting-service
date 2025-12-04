// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data.Repositories;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Exceptions;
using Voting.Lib.Database.Repositories;

namespace Voting.ECollecting.Citizen.Adapter.Data.Repositories;

public class AccessControlListDoiRepository(DataContext context)
    : DbRepository<DataContext, AccessControlListDoiEntity>(context), IAccessControlListDoiRepository
{
    public async Task<string> GetSingleBfsForDoiType(AclDomainOfInfluenceType doiType)
    {
        return await Query()
                   .Where(x => x.Type == doiType && !string.IsNullOrEmpty(x.Bfs))
                   .Select(x => x.Bfs)
                   .Distinct()
                   .SingleAsync()
               ?? throw new EntityNotFoundException(nameof(AclDomainOfInfluenceType), new { Type = doiType });
    }
}
