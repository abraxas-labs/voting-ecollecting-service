// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public class CollectionCitizenLogEntityModelBuilder : IEntityTypeConfiguration<CollectionCitizenLogEntity>
{
    public void Configure(EntityTypeBuilder<CollectionCitizenLogEntity> builder)
    {
        IntegritySignatureEntityModelBuilder.Configure(builder);

        builder
            .HasOne(x => x.Collection)
            .WithMany(x => x.CitizenLogs)
            .HasForeignKey(x => x.CollectionId);

        builder
            .HasIndex(x => new { x.CollectionId, x.VotingStimmregisterIdMac })
            .IsUnique();
    }
}
