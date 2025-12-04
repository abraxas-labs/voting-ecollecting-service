// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public class CollectionCountModelBuilder : IEntityTypeConfiguration<CollectionCountEntity>
{
    public void Configure(EntityTypeBuilder<CollectionCountEntity> builder)
    {
        IntegritySignatureEntityModelBuilder.Configure(builder);

        builder
            .HasOne(d => d.Collection)
            .WithOne(x => x.CollectionCount)
            .HasForeignKey<CollectionCountEntity>(d => d.CollectionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
