// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Entities.Audit;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.Lib.Common;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.CollectionTests;

public class CollectionPermissionUniqueOwnerTest : BaseRestTest
{
    public CollectionPermissionUniqueOwnerTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidUnityEnabledForCollectionCollecting));
    }

    [Fact]
    public async Task ShouldNotAllowTwoOwnerPermissions()
    {
        // Seeder adds an owner permission. Trying to add another one should fail.
        var action = async () => await RunOnDb(async db =>
        {
            db.CollectionPermissions.Add(new CollectionPermissionEntity
            {
                CollectionId = InitiativesCtStGallen.GuidUnityEnabledForCollectionCollecting,
                Email = "email2@host.com",
                Role = CollectionPermissionRole.Owner,
                State = CollectionPermissionState.Accepted,
                AuditInfo = new AuditInfo
                {
                    CreatedById = "test",
                    CreatedByName = "test",
                    CreatedByEmail = "test@host.com",
                    CreatedAt = DateTime.UtcNow,
                },
                Token = UrlToken.New(),
                TokenExpiry = DateTime.UtcNow.AddDays(1),
            });
            await db.SaveChangesAsync();
        });

        await Assert.ThrowsAsync<DbUpdateException>(action);
    }
}
