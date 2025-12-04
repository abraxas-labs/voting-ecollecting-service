// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Migrations;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.CollectionTests;

public class CollectionResendPermissionTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    private static readonly Guid _permissionId = CollectionPermissions.BuildGuid(
            InitiativesCtStGallen.GuidLegislativeInPreparation,
            false,
            CollectionPermissionRole.Deputy);

    public CollectionResendPermissionTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation));
    }

    [Fact]
    public async Task ShouldSendEmail()
    {
        ResetUserNotificationSender();

        var oldPermission = await GetEntity<CollectionPermissionEntity>(_permissionId);

        // ensure new expiry is set.
        GetService<FakeTimeProvider>().Advance(TimeSpan.FromDays(1));
        await AuthenticatedClient.ResendPermissionAsync(NewValidRequest());

        // ensure token is rotated.
        var permission = await GetEntity<CollectionPermissionEntity>(_permissionId);
        permission.Token.Should().NotBe(oldPermission.Token!.Value);
        permission.TokenExpiry.Should().BeAfter(oldPermission.TokenExpiry!.Value);

        var notifications = await RunScoped((MigrationDataContext db) => db
            .UserNotifications
            .OrderBy(x => x.Id)
            .ToListAsync());

        var sent = SentUserNotifications;
        await Verify(new { sent, notifications }).ScrubUrlTokens();
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            // ensure new expiry is set.
            GetService<FakeTimeProvider>().Advance(TimeSpan.FromDays(1));
            await AuthenticatedClient.ResendPermissionAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries())
                .ScrubMember("Token");
        });
    }

    [Fact]
    public async Task ShouldSendEmailAsDeputy()
    {
        ResetUserNotificationSender();

        await DeputyClient.ResendPermissionAsync(NewValidRequest());

        var notifications = await RunScoped((MigrationDataContext db) => db
            .UserNotifications
            .OrderBy(x => x.Id)
            .ToListAsync());

        var sent = SentUserNotifications;
        await Verify(new { sent, notifications }).ScrubUrlTokens();
    }

    [Fact]
    public async Task AsReaderShouldFail()
    {
        await AssertStatus(
            async () => await ReaderClient.ResendPermissionAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsDeputyNotAcceptedShouldFail()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.ResendPermissionAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldSetStateFailureWhenFailed()
    {
        ResetUserNotificationSender(true);

        await AssertStatus(
            async () => await AuthenticatedClient.ResendPermissionAsync(NewValidRequest()),
            StatusCode.Internal);

        var notifications = await RunScoped((MigrationDataContext db) => db
            .UserNotifications
            .OrderBy(x => x.Id)
            .ToListAsync());

        SentUserNotifications.Should().BeEmpty();
        notifications.Count.Should().Be(1);
        notifications[0].State.Should().Be(UserNotificationState.Failed);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.ResendPermissionAsync(NewValidRequest(x => x.Id = "4c62b23c-6bc7-48c1-8bbd-8e5a6e12f714")),
            StatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(async () => await Client.ResendPermissionAsync(NewValidRequest()), StatusCode.Unauthenticated);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInState(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        if (state.IsEndedOrAborted())
        {
            await AssertStatus(
                async () => await AuthenticatedClient.ResendPermissionAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
        else
        {
            await AuthenticatedClient.ResendPermissionAsync(NewValidRequest());
        }
    }

    [Theory]
    [EnumData<CollectionPermissionState>]
    public async Task PermissionState(CollectionPermissionState state)
    {
        await ModifyDbEntities<CollectionPermissionEntity>(
            e => e.Id == _permissionId,
            e => e.State = state);

        if (state == CollectionPermissionState.Pending)
        {
            await AuthenticatedClient.ResendPermissionAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await AuthenticatedClient.ResendPermissionAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    private ResendCollectionPermissionRequest NewValidRequest(Action<ResendCollectionPermissionRequest>? customizer = null)
    {
        var request = new ResendCollectionPermissionRequest
        {
            Id = _permissionId.ToString(),
        };
        customizer?.Invoke(request);
        return request;
    }
}
