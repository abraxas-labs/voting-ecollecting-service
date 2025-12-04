// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Shared.Domain.ModelBuilders;

public class InitiativeModelBuilder : IEntityTypeConfiguration<InitiativeEntity>,
    IEntityTypeConfiguration<InitiativeSubTypeEntity>
{
    public static readonly Guid ConstitutionalId = Guid.Parse("8926f191-6ba3-475f-9db0-d599b3317358");
    public static readonly Guid LegislativeId = Guid.Parse("9bcaba6c-bc1b-43d6-a59e-620ac2f4872a");
    public static readonly Guid UnityId = Guid.Parse("abd22fb4-f5d9-463b-8605-edfc0d93a6a3");
    public static readonly Guid FederalId = Guid.Parse("d0c38ef9-0619-4fdc-a859-237bf6f6d1d3");

    public void Configure(EntityTypeBuilder<InitiativeEntity> builder)
    {
        builder
            .Property(x => x.CollectionStartDate)
            .HasUtcConversion();

        builder
            .Property(x => x.CollectionEndDate)
            .HasUtcConversion();

        builder
            .HasOne(x => x.SubType)
            .WithMany()
            .HasForeignKey(x => x.SubTypeId);

        builder
            .HasMany(x => x.CommitteeLists)
            .WithOne(x => x.CommitteeListOfInitiative)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.CommitteeMembers)
            .WithOne(x => x.Initiative)
            .HasForeignKey(x => x.InitiativeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(x => x.LockedFields);
    }

    public void Configure(EntityTypeBuilder<InitiativeSubTypeEntity> builder)
    {
        builder
            .HasData(
                new InitiativeSubTypeEntity
                {
                    Id = ConstitutionalId,
                    DomainOfInfluenceType = DomainOfInfluenceType.Ct,
                    Bfs = "3200",
                    Description = "Verfassungsinitiative",
                    MinSignatureCount = 8000,
                    MaxElectronicSignatureCount = 4000,
                },
                new InitiativeSubTypeEntity
                {
                    Id = LegislativeId,
                    DomainOfInfluenceType = DomainOfInfluenceType.Ct,
                    Bfs = "3200",
                    Description = "Gesetzesinitiative",
                    MinSignatureCount = 6000,
                    MaxElectronicSignatureCount = 3000,
                },
                new InitiativeSubTypeEntity
                {
                    Id = UnityId,
                    DomainOfInfluenceType = DomainOfInfluenceType.Ct,
                    Bfs = "3200",
                    Description = "Einheitsinitiative",
                    MinSignatureCount = 4000,
                    MaxElectronicSignatureCount = 2000,
                },
                new InitiativeSubTypeEntity
                {
                    Id = FederalId,
                    DomainOfInfluenceType = DomainOfInfluenceType.Ch,
                    Bfs = string.Empty,
                    Description = "Volksinitiative",
                    MinSignatureCount = 100000,
                    MaxElectronicSignatureCount = 50000,
                });
    }
}
