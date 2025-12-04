// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public class CertificateModelBuilder : IEntityTypeConfiguration<CertificateEntity>
{
    public void Configure(EntityTypeBuilder<CertificateEntity> builder)
    {
        AuditedEntityModelBuilder.Configure(builder);
        builder.OwnsOne(x => x.CAInfo);
        builder.OwnsOne(x => x.Info);
        builder.HasOne(x => x.Content)
            .WithOne(x => x.ContentOfCertificate)
            .HasForeignKey<CertificateEntity>(x => x.ContentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
