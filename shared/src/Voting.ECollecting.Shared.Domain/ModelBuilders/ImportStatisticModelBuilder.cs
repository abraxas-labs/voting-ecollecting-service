// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public class ImportStatisticModelBuilder : IEntityTypeConfiguration<ImportStatisticEntity>
{
    public void Configure(EntityTypeBuilder<ImportStatisticEntity> builder)
    {
        AuditedEntityModelBuilder.Configure(builder);

        builder
            .Property(p => p.ImportStatus)
            .HasConversion(new EnumToStringConverter<ImportStatus>());

        builder
            .Property(p => p.ImportType)
            .HasConversion(new EnumToStringConverter<ImportType>());

        builder
            .Property(p => p.SourceSystem)
            .HasConversion(new EnumToStringConverter<ImportSourceSystem>());

        builder
            .Property(d => d.StartedDate)
            .HasUtcConversion();

        builder
            .Property(d => d.FinishedDate)
            .HasUtcConversion();
    }
}
