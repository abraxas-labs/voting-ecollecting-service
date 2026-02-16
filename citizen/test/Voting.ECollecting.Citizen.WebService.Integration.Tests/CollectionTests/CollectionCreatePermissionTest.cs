// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
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
using Voting.ECollecting.Shared.Migrations;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;
using CollectionPermissionRole = Voting.ECollecting.Proto.Shared.V1.Enums.CollectionPermissionRole;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.CollectionTests;

public class CollectionCreatePermissionTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionCreatePermissionTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation));
    }

    [Fact]
    public async Task ShouldWorkAsCreator()
    {
        ResetUserNotificationSender();

        var response = await AuthenticatedClient.CreatePermissionAsync(NewValidRequest());
        var permission = await RunOnDb(db => db.CollectionPermissions.FirstAsync(x => x.Id == Guid.Parse(response.Id)));
        var notifications = await RunScoped((MigrationDataContext db) => db
            .UserNotifications
            .OrderBy(x => x.Id)
            .ToListAsync());

        var sent = SentUserNotifications;
        await Verify(new { permission, sent, notifications }).ScrubUrlTokens();
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await AuthenticatedClient.CreatePermissionAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries())
                .ScrubMember("Token");
        });
    }

    [Fact]
    public async Task ShouldThrowAsCreatorSelf()
    {
        var req = NewValidRequest(x => x.Email = MockedUserContext.Default.CitizenCreator.EMail);
        await AssertStatus(
            async () => await AuthenticatedClient.CreatePermissionAsync(req),
            StatusCode.AlreadyExists,
            nameof(CannotAddOwnerPermissionException));
    }

    [Fact]
    public async Task ShouldWorkAsDeputy()
    {
        ResetUserNotificationSender();

        var response = await DeputyClient.CreatePermissionAsync(NewValidRequest());
        var permission = await RunOnDb(db => db.CollectionPermissions.FirstOrDefaultAsync(x => x.Id == Guid.Parse(response.Id)));
        await Verify(permission).ScrubUrlTokens();
    }

    [Fact]
    public async Task AsReaderShouldFail()
    {
        await AssertStatus(
            async () => await ReaderClient.CreatePermissionAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsDeputyNotAcceptedShouldFail()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.CreatePermissionAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task OwnerRoleShouldFail()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.CreatePermissionAsync(NewValidRequest(r => r.Role = CollectionPermissionRole.Owner)),
            StatusCode.AlreadyExists);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.CreatePermissionAsync(NewValidRequest(x => x.CollectionId = "6cb8abec-07e3-484b-b5e9-d9ca528188d9")),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldSetStateFailureWhenFailed()
    {
        ResetUserNotificationSender(true);

        await AssertStatus(
            async () => await AuthenticatedClient.CreatePermissionAsync(NewValidRequest()),
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
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(async () => await Client.CreatePermissionAsync(new CreateCollectionPermissionRequest()), StatusCode.Unauthenticated);
    }

    [Fact]
    public async Task DuplicateEmailShouldFail()
    {
        await AuthenticatedClient.CreatePermissionAsync(NewValidRequest());
        await AssertStatus(
            async () => await AuthenticatedClient.CreatePermissionAsync(NewValidRequest()),
            StatusCode.AlreadyExists);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInState(CollectionState state)
    {
        await ModifyDbEntities<InitiativeEntity>(
            e => e.Id == InitiativesCtStGallen.GuidLegislativeInPreparation,
            e => e.State = state);

        if (state.IsEndedOrAborted())
        {
            await AssertStatus(
                async () => await AuthenticatedClient.CreatePermissionAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
        else
        {
            await AuthenticatedClient.CreatePermissionAsync(NewValidRequest());
        }
    }

    private CreateCollectionPermissionRequest NewValidRequest(Action<CreateCollectionPermissionRequest>? customizer = null)
    {
        var request = new CreateCollectionPermissionRequest
        {
            CollectionId = InitiativesCtStGallen.IdLegislativeInPreparation,
            LastName = "Muster",
            FirstName = "Hans",
            Email = "hans.muster@example.com",
            Role = CollectionPermissionRole.Reader,
        };
        customizer?.Invoke(request);
        return request;
    }
}
