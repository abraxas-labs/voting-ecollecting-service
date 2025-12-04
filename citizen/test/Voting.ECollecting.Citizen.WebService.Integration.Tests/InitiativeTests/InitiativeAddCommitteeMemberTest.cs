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
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;
using Voting.Lib.Testing.Mocks;
using CollectionPermissionRole = Voting.ECollecting.Proto.Shared.V1.Enums.CollectionPermissionRole;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeAddCommitteeMemberTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeAddCommitteeMemberTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation, InitiativesCtStGallen.GuidLegislativeReturnedForCorrection));
    }

    [Fact]
    public async Task ShouldWork()
    {
        ResetUserNotificationSender();

        var req = NewValidRequest();
        var id = await AuthenticatedClient.AddCommitteeMemberAsync(req);

        var member = await RunOnDb(db => db.InitiativeCommitteeMembers.SingleAsync(x => x.Id == Guid.Parse(id.Id)));
        member.InitiativeId.Should().Be(InitiativesCtStGallen.GuidLegislativeInPreparation);

        var permission = await RunOnDb(db => db.CollectionPermissions.SingleAsync(x => x.Email == req.Email));
        var notifications = await RunOnDb(db => db
            .UserNotifications
            .OrderBy(x => x.Id)
            .ToListAsync());

        var sent = SentUserNotifications;
        await Verify(new { member, sent, notifications, permission }).ScrubUrlTokens();

        // sort indexes should be consecutive
        var sortIndexes = await RunOnDb(db => db.InitiativeCommitteeMembers
            .Where(x => x.InitiativeId == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .OrderBy(x => x.SortIndex)
            .Select(x => x.SortIndex)
            .ToListAsync());

        var i = 0;
        foreach (var si in sortIndexes)
        {
            si.Should().Be(i);
            i++;
        }
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            var req = NewValidRequest();
            await AuthenticatedClient.AddCommitteeMemberAsync(req);
            await Verify(await GetAuditTrailEntries())
                .ScrubMember("Token");
        });
    }

    [Fact]
    public async Task ShouldWorkWithoutRoleWithManualApproval()
    {
        ResetUserNotificationSender();

        var req = NewValidRequest();
        req.Role = CollectionPermissionRole.Unspecified;
        req.RequestMemberSignature = false;
        req.Email = string.Empty;
        var id = await AuthenticatedClient.AddCommitteeMemberAsync(req);

        var member = await RunOnDb(db => db.InitiativeCommitteeMembers.SingleAsync(x => x.Id == Guid.Parse(id.Id)));
        member.InitiativeId.Should().Be(InitiativesCtStGallen.GuidLegislativeInPreparation);

        var hasPermission = await RunOnDb(db => db.CollectionPermissions.AnyAsync(x => x.Email == req.Email));
        hasPermission.Should().BeFalse();

        var hasNotifications = await RunOnDb(db => db
            .UserNotifications
            .AnyAsync());
        hasNotifications.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldThrowWithOwnerRole()
    {
        var req = NewValidRequest();
        req.Role = CollectionPermissionRole.Owner;
        await AssertStatus(
            async () => await AuthenticatedClient.AddCommitteeMemberAsync(req),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task ShouldThrowAsCreatorAsSelfWithRole()
    {
        var req = NewValidRequest();
        req.Email = MockedUserContext.Default.CitizenCreator.EMail;
        await AssertStatus(
            async () => await AuthenticatedClient.AddCommitteeMemberAsync(req),
            StatusCode.AlreadyExists,
            nameof(CannotAddOwnerPermissionException));
    }

    [Fact]
    public async Task ShouldThrowWithUsedEmail()
    {
        var req = NewValidRequest();
        req.Email = "jeanine.mueller@example.com";
        req.Role = CollectionPermissionRole.Deputy;
        await AssertStatus(
            async () => await AuthenticatedClient.AddCommitteeMemberAsync(req),
            StatusCode.AlreadyExists,
            nameof(CollectionPermissionAlreadyExistsException));
    }

    [Fact]
    public async Task ShouldWorkWithManualApprovalOnPaper()
    {
        var req = NewValidRequest();
        req.InitiativeId = InitiativesCtStGallen.IdLegislativeReturnedForCorrection;
        req.RequestMemberSignature = false;
        req.Role = CollectionPermissionRole.Unspecified;
        req.Email = string.Empty;
        var id = await AuthenticatedClient.AddCommitteeMemberAsync(req);

        var member = await RunOnDb(db => db.InitiativeCommitteeMembers.SingleAsync(x => x.Id == Guid.Parse(id.Id)));
        member.InitiativeId.Should().Be(InitiativesCtStGallen.GuidLegislativeReturnedForCorrection);

        var hasPermission = await RunOnDb(db => db.CollectionPermissions.AnyAsync(x => x.Email == req.Email));
        hasPermission.Should().BeFalse();

        var hasNotifications = await RunOnDb(db => db
            .UserNotifications
            .AnyAsync());
        hasNotifications.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldThrowWithApprovalOnPaper()
    {
        var req = NewValidRequest();
        req.InitiativeId = InitiativesCtStGallen.IdLegislativeReturnedForCorrection;
        await AssertStatus(
            async () => await AuthenticatedClient.AddCommitteeMemberAsync(req),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task ShouldThrowWithRoleButNoEmail()
    {
        var req = NewValidRequest();
        req.RequestMemberSignature = false;
        req.Email = string.Empty;
        await AssertStatus(
            async () => await AuthenticatedClient.AddCommitteeMemberAsync(req),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task ShouldThrowInitiativeNotFound()
    {
        var req = NewValidRequest();
        req.InitiativeId = "41923149-db6c-4296-aa72-d24c57037678";
        await AssertStatus(
            async () => await AuthenticatedClient.AddCommitteeMemberAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowWithApprovalButNoEmail()
    {
        var req = NewValidRequest();
        req.Role = CollectionPermissionRole.Unspecified;
        req.Email = string.Empty;
        await AssertStatus(
            async () => await AuthenticatedClient.AddCommitteeMemberAsync(req),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task ShouldWorkAsDeputy()
    {
        var req = NewValidRequest();
        var id = await DeputyClient.AddCommitteeMemberAsync(req);

        var member = await RunOnDb(db => db.InitiativeCommitteeMembers.SingleAsync(x => x.Id == Guid.Parse(id.Id)));
        member.InitiativeId.Should().Be(InitiativesCtStGallen.IdLegislativeInPreparation);

        var hasPermission = await RunOnDb(db => db.CollectionPermissions.AnyAsync(x => x.Email == req.Email));
        hasPermission.Should().BeTrue();

        var hasNotifications = await RunOnDb(db => db
            .UserNotifications
            .AnyAsync());
        hasNotifications.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldThrowAsReader()
    {
        await AssertStatus(
            async () => await ReaderClient.AddCommitteeMemberAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsDeputyNotAccepted()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.AddCommitteeMemberAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowWithLockedFields()
    {
        var req = NewValidRequest(x => x.InitiativeId = InitiativesCtStGallen.IdLegislativeReturnedForCorrection);
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == Guid.Parse(req.InitiativeId),
            x => x.LockedFields = new InitiativeLockedFields
            {
                CommitteeMembers = true,
            });
        await AssertStatus(
            async () => await AuthenticatedClient.AddCommitteeMemberAsync(req),
            StatusCode.InvalidArgument,
            "Cannot edit locked field CommitteeMembers");
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        if (state.InPreparationOrReturnForCorrection())
        {
            await AuthenticatedClient.AddCommitteeMemberAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await DeputyNotAcceptedClient.AddCommitteeMemberAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    private AddCommitteeMemberRequest NewValidRequest(Action<AddCommitteeMemberRequest>? customizer = null)
    {
        var req = new AddCommitteeMemberRequest
        {
            Email = "foo@example.com",
            FirstName = "Foo",
            LastName = "Bar",
            PoliticalFirstName = "Foo (pol)",
            PoliticalLastName = "Bar (pol)",
            DateOfBirth = MockedClock.GetTimestamp(-55 * 365),
            InitiativeId = InitiativesCtStGallen.IdLegislativeInPreparation,
            RequestMemberSignature = true,
            PoliticalDuty = "Protokollführer",
            PoliticalBfs = Bfs.MunicipalityStGallen,
            Bfs = Bfs.MunicipalityStGallen,
            Role = CollectionPermissionRole.Deputy,
        };

        customizer?.Invoke(req);
        return req;
    }
}
