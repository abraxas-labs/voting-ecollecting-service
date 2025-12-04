// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Entities.Audit;
using Voting.ECollecting.Shared.Domain.ModelBuilders;
using Voting.ECollecting.Shared.Migrations.ModelBuilders;

namespace Voting.ECollecting.Shared.Migrations;

public class MigrationDataContext(DbContextOptions<MigrationDataContext> options)
    : DbContext(options)
{
    public string? Language { get; set; }

    public DbSet<CertificateEntity> Certificates { get; set; } = null!;

    public DbSet<FileEntity> Files { get; set; } = null!;

    public DbSet<FileContentEntity> FileContents { get; set; } = null!;

    public DbSet<DecreeEntity> Decrees { get; set; } = null!;

    public DbSet<CollectionBaseEntity> Collections { get; set; } = null!;

    public DbSet<CollectionSignatureSheetEntity> CollectionSignatureSheets { get; set; } = null!;

    public DbSet<CollectionCountEntity> CollectionCounts { get; set; } = null!;

    public DbSet<AccessControlListDoiEntity> AccessControlListDois { get; set; } = null!;

    public DbSet<ImportStatisticEntity> ImportStatistics { get; set; } = null!;

    public DbSet<InitiativeEntity> Initiatives { get; set; } = null!;

    public DbSet<InitiativeCommitteeMemberEntity> InitiativeCommitteeMembers { get; set; } = null!;

    public DbSet<InitiativeSubTypeEntity> InitiativeSubTypes { get; set; } = null!;

    public DbSet<ReferendumEntity> Referendums { get; set; } = null!;

    public DbSet<CollectionPermissionEntity> CollectionPermissions { get; set; } = null!;

    public DbSet<CollectionMessageEntity> CollectionMessages { get; set; } = null!;

    public DbSet<UserNotificationEntity> UserNotifications { get; set; } = null!;

    public DbSet<CollectionCitizenEntity> CollectionCitizens { get; set; } = null!;

    public DbSet<CollectionCitizenLogEntity> CollectionCitizenLogs { get; set; } = null!;

    public DbSet<CollectionMunicipalityEntity> CollectionMunicipalities { get; set; } = null!;

    public DbSet<SecondFactorTransactionEntity> SecondFactorTransactions { get; set; } = null!;

    public DbSet<DomainOfInfluenceEntity> DomainOfInfluences { get; set; } = null!;

    public DbSet<AuditTrailEntryEntity> AuditTrailEntries { get; set; } = null!;

    public DbSet<CollectionCitizenLogAuditTrailEntryEntity> CollectionCitizenLogAuditTrailEntries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        DbContextAccessor.DbContext = this;
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IntegritySignatureEntityModelBuilder).Assembly);
    }
}
