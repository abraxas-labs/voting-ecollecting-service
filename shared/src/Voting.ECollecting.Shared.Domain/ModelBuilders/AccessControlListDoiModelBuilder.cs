// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public class AccessControlListDoiModelBuilder : IEntityTypeConfiguration<AccessControlListDoiEntity>
{
    public void Configure(EntityTypeBuilder<AccessControlListDoiEntity> builder)
    {
        AuditedEntityModelBuilder.Configure(builder);

        builder
            .HasOne(d => d.Parent)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(d => d.ImportStatistic)
            .WithMany(x => x.AccessControlListDois)
            .HasForeignKey(x => x.ImportStatisticId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .Property(d => d.Canton)
            .HasConversion(new EnumToStringConverter<Canton>());

        builder
            .Property(d => d.Type)
            .HasConversion(new EnumToStringConverter<AclDomainOfInfluenceType>());
    }
}
