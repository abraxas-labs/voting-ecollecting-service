// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.ECollecting.Shared.Domain.Entities.Audit;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public static class AuditedEntityModelBuilder
{
    public static void Configure<T>(EntityTypeBuilder<T> builder)
        where T : class, IAuditedEntity
    {
        builder.OwnsOne(x => x.AuditInfo, Configure);
    }

    public static void Configure<TEntity, TRelatedEntity>(OwnedNavigationBuilder<TEntity, TRelatedEntity> builder)
        where TEntity : class
        where TRelatedEntity : AuditInfo
    {
        builder.Property(x => x.CreatedAt).HasUtcConversion();
        builder.Property(x => x.ModifiedAt).HasUtcConversion();
    }
}
