// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public class InitiativeCommitteeMemberModelBuilder : IEntityTypeConfiguration<InitiativeCommitteeMemberEntity>
{
    public void Configure(EntityTypeBuilder<InitiativeCommitteeMemberEntity> builder)
    {
        AuditedEntityModelBuilder.Configure(builder);

        builder.HasIndex(x => new { x.InitiativeId, x.SortIndex });

        builder.HasOne(x => x.Permission)
            .WithOne(x => x.InitiativeCommitteeMember)
            .HasForeignKey<CollectionPermissionEntity>(x => x.InitiativeCommitteeMemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SignatureFile)
            .WithOne(x => x.SignatureSheetOfInitiativeCommitteeMember)
            .HasForeignKey<InitiativeCommitteeMemberEntity>(x => x.SignatureFileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.Token)
            .HasConversion<string?>(x => x, x => x!);

        builder.Property(x => x.TokenExpiry).HasUtcConversion();

        builder.HasIndex(x => x.ApprovalState);
    }
}
