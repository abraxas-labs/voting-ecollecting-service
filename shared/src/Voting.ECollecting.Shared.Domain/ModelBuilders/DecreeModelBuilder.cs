// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public class DecreeModelBuilder : IEntityTypeConfiguration<DecreeEntity>
{
    private const string DescriptionLowerPropertyName = "DescriptionLower";

    public void Configure(EntityTypeBuilder<DecreeEntity> builder)
    {
        IntegritySignatureEntityModelBuilder.Configure(builder);

        builder.Property<string>(DescriptionLowerPropertyName)
            .HasComputedColumnSql($"lower(\"{nameof(DecreeEntity.Description)}\")", stored: true);

        builder
            .HasIndex(DescriptionLowerPropertyName, nameof(DecreeEntity.Bfs))
            .IsUnique();
    }
}
