// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public class CollectionPermissionModelBuilder : IEntityTypeConfiguration<CollectionPermissionEntity>
{
    public void Configure(EntityTypeBuilder<CollectionPermissionEntity> builder)
    {
        IntegritySignatureEntityModelBuilder.Configure(builder);

        builder
            .HasOne(d => d.Collection)
            .WithMany(x => x.Permissions)
            .HasForeignKey(d => d.CollectionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(x => new { x.CollectionId, x.Email })
            .IsUnique();

        builder.Property(x => x.Token)
            .HasConversion<string?>(x => x, x => x!);
    }
}
