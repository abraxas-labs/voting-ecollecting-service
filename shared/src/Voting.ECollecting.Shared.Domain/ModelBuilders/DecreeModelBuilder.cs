// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public class DecreeModelBuilder : IEntityTypeConfiguration<DecreeEntity>
{
    public void Configure(EntityTypeBuilder<DecreeEntity> builder)
    {
        IntegritySignatureEntityModelBuilder.Configure(builder);

        builder
            .Property(d => d.CollectionStartDate)
            .HasUtcConversion();

        builder
            .Property(d => d.CollectionEndDate)
            .HasUtcConversion();
    }
}
