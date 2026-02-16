// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

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
            .HasIndex(x => new { x.CollectionId, x.IamUserId })
            .HasFilter($"\"{nameof(CollectionPermissionEntity.IamUserId)}\" <> ''")
            .IsUnique();

        builder
            .HasIndex(x => x.CollectionId)
            .IsUnique()
            .HasFilter($"\"{nameof(CollectionPermissionEntity.Role)}\" = {(int)CollectionPermissionRole.Owner}")
            .HasDatabaseName("IX_CollectionPermissions_Owner");

        builder.Property(x => x.Token)
            .HasConversion<string?>(x => x, x => x!);

        builder.Property(x => x.TokenExpiry).HasUtcConversion();

        builder.HasIndex(x => x.State);
    }
}
