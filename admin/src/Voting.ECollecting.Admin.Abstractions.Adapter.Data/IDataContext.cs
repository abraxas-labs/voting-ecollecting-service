// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Abstractions.Adapter.Data;

public interface IDataContext
{
    DbSet<InitiativeCommitteeMemberEntity> InitiativeCommitteeMembers { get; set; }

    DbSet<CollectionPermissionEntity> CollectionPermissions { get; set; }

    Task SaveChangesAsync();

    Task<IDbContextTransaction> BeginTransaction(CancellationToken ct = default);
}
