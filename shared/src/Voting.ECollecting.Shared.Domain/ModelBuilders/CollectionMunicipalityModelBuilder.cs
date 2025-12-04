// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public class CollectionMunicipalityModelBuilder : IEntityTypeConfiguration<CollectionMunicipalityEntity>
{
    public void Configure(EntityTypeBuilder<CollectionMunicipalityEntity> builder)
    {
        AuditedEntityModelBuilder.Configure(builder);
        builder.OwnsOne(x => x.PhysicalCount);
        builder
            .HasOne(x => x.Collection)
            .WithMany(x => x.Municipalities)
            .HasForeignKey(x => x.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.CollectionId, x.Bfs }).IsUnique();

        builder.Ignore(x => x.SignatureSheetsCount);
    }
}
