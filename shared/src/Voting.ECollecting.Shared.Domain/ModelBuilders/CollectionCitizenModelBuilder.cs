// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public class CollectionCitizenModelBuilder : IEntityTypeConfiguration<CollectionCitizenEntity>
{
    public void Configure(EntityTypeBuilder<CollectionCitizenEntity> builder)
    {
        IntegritySignatureEntityModelBuilder.Configure(builder);

        builder
            .HasOne(x => x.CollectionMunicipality)
            .WithMany(x => x.Citizens)
            .HasForeignKey(x => x.CollectionMunicipalityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.SignatureSheet)
            .WithMany(x => x.Citizens)
            .HasForeignKey(x => x.SignatureSheetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Log)
            .WithOne(x => x.CollectionCitizen)
            .HasForeignKey<CollectionCitizenLogEntity>(x => x.CollectionCitizenId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.SignatureSheet)
            .WithMany(x => x.Citizens)
            .HasForeignKey(x => x.SignatureSheetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
