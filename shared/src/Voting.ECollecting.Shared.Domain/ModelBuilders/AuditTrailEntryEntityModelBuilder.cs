// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.ECollecting.Shared.Domain.Entities.Audit;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public class AuditTrailEntryEntityModelBuilder : IEntityTypeConfiguration<AuditTrailEntryEntity>
{
    public void Configure(EntityTypeBuilder<AuditTrailEntryEntity> builder)
    {
        AuditedEntityModelBuilder.Configure(builder);

        builder
            .Property(e => e.RecordBefore)
            .HasColumnType("jsonb");

        builder
            .Property(e => e.RecordAfter)
            .HasColumnType("jsonb");
    }
}
