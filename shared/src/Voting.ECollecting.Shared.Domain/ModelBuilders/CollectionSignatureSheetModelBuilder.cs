// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public class CollectionSignatureSheetModelBuilder : IEntityTypeConfiguration<CollectionSignatureSheetEntity>
{
    public void Configure(EntityTypeBuilder<CollectionSignatureSheetEntity> builder)
    {
        AuditedEntityModelBuilder.Configure(builder);
        builder
            .Property(x => x.AttestedAt)
            .HasPrecision(3); // no need to store microseconds, these are a hassle in the FE (no direct js support) and we don't need them anyway.
        builder.OwnsOne(x => x.Count);
        builder
            .HasOne(x => x.CollectionMunicipality)
            .WithMany(x => x.SignatureSheets)
            .HasForeignKey(x => x.CollectionMunicipalityId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.CollectionMunicipalityId, x.Number }).IsUnique();
    }
}
