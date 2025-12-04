// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public class ReferendumModelBuilder : IEntityTypeConfiguration<ReferendumEntity>
{
    public void Configure(EntityTypeBuilder<ReferendumEntity> builder)
    {
        builder
            .HasOne(d => d.Decree)
            .WithMany(x => x.Collections)
            .HasForeignKey(d => d.DecreeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
