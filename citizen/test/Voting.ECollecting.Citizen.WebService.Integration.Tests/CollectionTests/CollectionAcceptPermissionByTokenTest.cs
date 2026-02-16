// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Citizen.Core.Exceptions;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;
using Voting.Lib.Common;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.CollectionTests;

public class CollectionAcceptPermissionByTokenTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    private static readonly Guid _id =
        CollectionPermissions.BuildGuid(
            InitiativesCtStGallen.GuidLegislativeInPreparation,
            false,
            CollectionPermissionRole.Deputy);

    private static readonly UrlToken _token =
        CollectionPermissions.BuildToken(
            InitiativesCtStGallen.GuidLegislativeInPreparation,
            CollectionPermissionRole.Deputy);

    public CollectionAcceptPermissionByTokenTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation));
    }

    [Fact]
    public async Task ShouldWork()
    {
        var client = CreateCitizenClient(acrValue: CitizenAuthMockDefaults.AcrValue100, email: "peter.huenkeler@example.com");
        await client.AcceptPermissionByTokenAsync(NewValidRequest());

        var permission = await RunOnDb(db => db.CollectionPermissions.SingleAsync(x => x.Id == _id));
        await Verify(permission);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            var client = CreateCitizenClient(acrValue: CitizenAuthMockDefaults.AcrValue100, email: "peter.huenkeler@example.com");
            await client.AcceptPermissionByTokenAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldWorkWithAcr400()
    {
        var client = CreateCitizenClient(acrValue: CitizenAuthMockDefaults.AcrValue400, email: "peter.huenkeler@example.com");
        await client.AcceptPermissionByTokenAsync(NewValidRequest());

        var permission = await RunOnDb(db => db.CollectionPermissions.SingleAsync(x => x.Id == _id));
        await Verify(permission);
    }

    [Fact]
    public async Task InsufficientAcrThrows()
    {
        var client = CreateCitizenClient(acrValue: string.Empty, email: "peter.huenkeler@example.com");
        await AssertStatus(
            async () => await client.AcceptPermissionByTokenAsync(NewValidRequest()),
            StatusCode.PermissionDenied,
            "InsufficientAcrException");
    }

    [Fact]
    public async Task EmailMismatchThrows()
    {
        var client = CreateCitizenClient(acrValue: CitizenAuthMockDefaults.AcrValue100, email: "peter.huenkeler-1@example.com");
        await AssertStatus(
            async () => await client.AcceptPermissionByTokenAsync(NewValidRequest()),
            StatusCode.PermissionDenied,
            "EmailDoesNotMatchException");
    }

    [Fact]
    public async Task NotFound()
    {
        var client = CreateCitizenClient(acrValue: CitizenAuthMockDefaults.AcrValue100, email: "peter.huenkeler@example.com");
        var req = new AcceptCollectionPermissionRequest { Token = UrlToken.New() };
        await AssertStatus(
            async () => await client.AcceptPermissionByTokenAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ExpiredShouldThrow()
    {
        await ModifyDbEntities(
            (CollectionPermissionEntity e) => e.Token == _token,
            e => e.TokenExpiry = MockedClock.GetDate(-4));

        var client = CreateCitizenClient(acrValue: CitizenAuthMockDefaults.AcrValue100, email: "peter.huenkeler@example.com");
        await AssertStatus(
            async () => await client.AcceptPermissionByTokenAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task CollectionState(CollectionState state)
    {
        await ModifyDbEntities<InitiativeEntity>(
            e => e.Id == InitiativesCtStGallen.GuidLegislativeInPreparation,
            e => e.State = state);

        var client = CreateCitizenClient(acrValue: CitizenAuthMockDefaults.AcrValue100, email: "peter.huenkeler@example.com");
        if (state.IsEndedOrAborted())
        {
            await AssertStatus(
                async () => await client.AcceptPermissionByTokenAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
        else
        {
            await client.AcceptPermissionByTokenAsync(NewValidRequest());
        }
    }

    [Theory]
    [EnumData<CollectionPermissionState>]
    public async Task PermissionState(CollectionPermissionState state)
    {
        await ModifyDbEntities<CollectionPermissionEntity>(
            e => e.Id == _id,
            e => e.State = state);

        var client = CreateCitizenClient(acrValue: CitizenAuthMockDefaults.AcrValue100, email: "peter.huenkeler@example.com");
        if (state == CollectionPermissionState.Pending)
        {
            await client.AcceptPermissionByTokenAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await client.AcceptPermissionByTokenAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    [Fact]
    public async Task AcceptInvitationWithExistingPermissionThrows()
    {
        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue100,
            email: "peter.huenkeler@example.com");

        // Accept the first permission
        await client.AcceptPermissionByTokenAsync(NewValidRequest());

        // Create another pending permission (invitation) for the same user on the same collection
        var newToken = UrlToken.New();
        var collectionId = InitiativesCtStGallen.GuidLegislativeInPreparation;

        await RunOnDb(async db =>
        {
            await db.CollectionPermissions.AddAsync(new CollectionPermissionEntity
            {
                Id = Guid.NewGuid(),
                CollectionId = collectionId,
                Email = "peter.huenkeler@example.com",
                FirstName = "Peter",
                LastName = "Huenkeler",
                Role = CollectionPermissionRole.Deputy,
                State = CollectionPermissionState.Pending,
                Token = newToken,
                TokenExpiry = DateTime.UtcNow.AddDays(1),
                AuditInfo = new Voting.ECollecting.Shared.Domain.Entities.Audit.AuditInfo
                {
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = "test",
                    CreatedByName = "Test User",
                },
            });
            await db.SaveChangesAsync();
        });

        // Try to accept the second invitation
        await AssertStatus(
            async () => await client.AcceptPermissionByTokenAsync(
                new AcceptCollectionPermissionRequest { Token = newToken }),
            StatusCode.InvalidArgument,
            nameof(UserHasAlreadyAPermissionException));
    }

    private AcceptCollectionPermissionRequest NewValidRequest()
    {
        return new AcceptCollectionPermissionRequest { Token = _token.ToString() };
    }
}
