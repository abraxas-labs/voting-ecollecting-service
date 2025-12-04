// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.ECollecting.Shared.Domain.Entities.Audit;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public class CollectionCitizenLogAuditTrailEntryModelBuilder : IEntityTypeConfiguration<CollectionCitizenLogAuditTrailEntryEntity>
{
    public void Configure(EntityTypeBuilder<CollectionCitizenLogAuditTrailEntryEntity> builder)
    {
        AuditedEntityModelBuilder.Configure(builder);

        builder
            .HasOne(x => x.SourceEntity)
            .WithMany(x => x.AuditTrailEntries)
            .HasForeignKey(x => x.SourceEntityId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Property(e => e.RecordBefore)
            .HasColumnType("jsonb");

        builder
            .Property(e => e.RecordAfter)
            .HasColumnType("jsonb");
    }
}
