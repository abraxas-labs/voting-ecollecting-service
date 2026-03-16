// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public class DomainOfInfluenceModelBuilder : IEntityTypeConfiguration<DomainOfInfluenceEntity>
{
    public void Configure(EntityTypeBuilder<DomainOfInfluenceEntity> builder)
    {
        AuditedEntityModelBuilder.Configure(builder);
        builder.Ignore(x => x.NameForProtocol);
        builder.HasOne(x => x.Logo)
            .WithOne(x => x.LogoOfDomainOfInfluence)
            .HasForeignKey<DomainOfInfluenceEntity>(x => x.LogoId)
            .OnDelete(DeleteBehavior.SetNull);
        builder
            .HasIndex(x => x.Bfs)
            .IsUnique();
    }
}
