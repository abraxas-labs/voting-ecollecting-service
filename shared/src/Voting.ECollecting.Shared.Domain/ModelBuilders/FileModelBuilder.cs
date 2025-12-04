// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public class FileModelBuilder : IEntityTypeConfiguration<FileEntity>
{
    public void Configure(EntityTypeBuilder<FileEntity> builder)
    {
        AuditedEntityModelBuilder.Configure(builder);

        builder.HasOne(x => x.Content)
            .WithOne(x => x.File)
            .HasForeignKey<FileContentEntity>(x => x.FileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
