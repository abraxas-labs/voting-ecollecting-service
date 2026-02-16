// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Admin.Abstractions.Adapter.Data;

public interface IDataContext
{
    string? Language { get; set; }

    DbSet<DecreeEntity> Decrees { get; set; }

    DbSet<CollectionCountEntity> CollectionCounts { get; set; }

    DbSet<AccessControlListDoiEntity> AccessControlListDois { get; set; }

    DbSet<ImportStatisticEntity> ImportStatistics { get; set; }

    DbSet<InitiativeCommitteeMemberEntity> InitiativeCommitteeMembers { get; set; }

    DbSet<CollectionPermissionEntity> CollectionPermissions { get; set; }

    DbSet<DomainOfInfluenceEntity> DomainOfInfluences { get; set; }

    Task SaveChangesAsync();

    Task<IDbContextTransaction> BeginTransaction(CancellationToken ct = default);
}
