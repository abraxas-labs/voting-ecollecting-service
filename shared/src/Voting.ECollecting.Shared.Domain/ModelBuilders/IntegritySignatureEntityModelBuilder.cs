// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.ECollecting.Shared.Domain.Entities.IntegritySignature;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public static class IntegritySignatureEntityModelBuilder
{
    public static void Configure<T>(EntityTypeBuilder<T> builder)
        where T : IntegritySignatureEntity
    {
        AuditedEntityModelBuilder.Configure(builder);
        builder.OwnsOne(x => x.IntegritySignatureInfo);
    }
}
