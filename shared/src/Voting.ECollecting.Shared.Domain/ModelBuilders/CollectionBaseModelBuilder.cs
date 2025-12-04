// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public class CollectionBaseModelBuilder : IEntityTypeConfiguration<CollectionBaseEntity>
{
    public void Configure(EntityTypeBuilder<CollectionBaseEntity> builder)
    {
        IntegritySignatureEntityModelBuilder.Configure(builder);

        builder.HasDiscriminator(x => x.Type)
            .HasValue<InitiativeEntity>(CollectionType.Initiative)
            .HasValue<ReferendumEntity>(CollectionType.Referendum)
            .HasValue<CollectionBaseEntity>(CollectionType.Unspecified);

        builder.HasOne(x => x.Image)
            .WithOne(x => x.ImageOfCollection)
            .HasForeignKey<CollectionBaseEntity>(x => x.ImageId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Logo)
            .WithOne(x => x.LogoOfCollection)
            .HasForeignKey<CollectionBaseEntity>(x => x.LogoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.SignatureSheetTemplate)
            .WithOne(x => x.SignatureSheetOfCollection)
            .HasForeignKey<CollectionBaseEntity>(x => x.SignatureSheetTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.OwnsOne(x => x.Address);
        builder.Navigation(x => x.Address).IsRequired();
    }
}
