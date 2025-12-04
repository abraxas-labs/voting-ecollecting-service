// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.ECollecting.Shared.Domain.Entities;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public class UserNotificationModelBuilder : IEntityTypeConfiguration<UserNotificationEntity>
{
    public void Configure(EntityTypeBuilder<UserNotificationEntity> builder)
    {
        builder.OwnsOne(x => x.TemplateBag, x =>
        {
            x.Ignore(y => y.PermissionToken);
            x.Ignore(y => y.InitiativeCommitteeMembershipToken);
            x.Ignore(y => y.AccessibilityMessage);
        });
        builder.Property(x => x.SentTimestamp).HasUtcConversion();

        builder.HasIndex(x => x.State);
    }
}
