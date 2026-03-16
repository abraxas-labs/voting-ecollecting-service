// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Voting.ECollecting.Citizen.Abstractions.Adapter.Data;
using Voting.ECollecting.Citizen.Adapter.Data.ModelBuilders;
using Voting.ECollecting.Shared.Adapter.Data.Builders;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Entities.Audit;
using Voting.ECollecting.Shared.Domain.ModelBuilders;
using Voting.Lib.Database.Extensions;

namespace Voting.ECollecting.Citizen.Adapter.Data;

public class DataContext : DbContext, IDataContext
{
    private readonly IAuditTrailEntryBuilder _auditTrailEntryBuilder;

    public DataContext(DbContextOptions<DataContext> options, IAuditTrailEntryBuilder auditTrailEntryBuilder)
    : base(options)
    {
        _auditTrailEntryBuilder = auditTrailEntryBuilder;
    }

    public string? Language { get; set; }

    public DbSet<FileEntity> Files { get; set; } = null!;

    public DbSet<FileContentEntity> FileContents { get; set; } = null!;

    public DbSet<DecreeEntity> Decrees { get; set; } = null!;

    public DbSet<CollectionCountEntity> CollectionCounts { get; set; } = null!;

    public DbSet<InitiativeSubTypeEntity> InitiativeSubTypes { get; set; } = null!;

    public DbSet<InitiativeEntity> Initiatives { get; set; } = null!;

    public DbSet<InitiativeCommitteeMemberEntity> InitiativeCommitteeMembers { get; set; } = null!;

    public DbSet<CollectionBaseEntity> Collections { get; set; } = null!;

    public DbSet<CollectionMessageEntity> CollectionMessages { get; set; } = null!;

    public DbSet<UserNotificationEntity> UserNotifications { get; set; } = null!;

    public DbSet<CollectionPermissionEntity> CollectionPermissions { get; set; } = null!;

    public DbSet<CollectionCitizenEntity> CollectionCitizens { get; set; } = null!;

    public DbSet<CollectionCitizenLogEntity> CollectionCitizenLogs { get; set; } = null!;

    public DbSet<CollectionMunicipalityEntity> CollectionMunicipalities { get; set; } = null!;

    public DbSet<DomainOfInfluenceEntity> DomainOfInfluences { get; set; } = null!;

    public DbSet<AuditTrailEntryEntity> AuditTrailEntries { get; set; } = null!;

    public DbSet<CollectionCitizenLogAuditTrailEntryEntity> CollectionCitizenLogAuditTrailEntries { get; set; }

    public async Task SaveChangesAsync()
    {
        await SaveChangesAsync(default);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddAuditTrailEntries();
        return base.SaveChangesAsync(cancellationToken);
    }

    public Task<IDbContextTransaction> BeginTransaction()
        => Database.BeginTransactionAsync();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        DbContextAccessor.DbContext = this;
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IntegritySignatureEntityModelBuilder).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.ConfigureMarkdownString();
        base.ConfigureConventions(configurationBuilder);
    }

    private void AddAuditTrailEntries()
    {
        var result = _auditTrailEntryBuilder.BuildAuditTrailEntries(this);
        AuditTrailEntries.AddRange(result.AuditTrailEntries);
        CollectionCitizenLogAuditTrailEntries.AddRange(result.CollectionCitizenLogAuditTrailEntries);
    }
}
