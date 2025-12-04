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
using Voting.ECollecting.Shared.Migrations;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeResendCommitteeMemberInvitationTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    private static readonly Guid _id = InitiativeCommitteeMembers.BuildGuid(
        InitiativesCtStGallen.GuidLegislativeInPreparation,
        "margarita@example.com");

    public InitiativeResendCommitteeMemberInvitationTest(TestApplicationFactory factory)
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

        var oldPermission = await GetEntity<InitiativeCommitteeMemberEntity>(_id);

        // ensure new expiry is set.
        GetService<FakeTimeProvider>().Advance(TimeSpan.FromDays(1));
        await AuthenticatedClient.ResendCommitteeMemberInvitationAsync(NewValidRequest());

        // ensure token is rotated
        var member = await GetEntity<InitiativeCommitteeMemberEntity>(_id);
        member.Token.Should().NotBe(oldPermission.Token!.Value);
        member.TokenExpiry.Should().BeAfter(oldPermission.TokenExpiry!.Value);

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
        ResetUserNotificationSender();

        var oldPermission = await GetEntity<InitiativeCommitteeMemberEntity>(_id);

        // ensure new expiry is set.
        GetService<FakeTimeProvider>().Advance(TimeSpan.FromDays(1));

        await RunInAuditTrailTestScope(async () =>
        {
            await AuthenticatedClient.ResendCommitteeMemberInvitationAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries())
                .ScrubMember("Token");
        });
    }

    [Fact]
    public async Task ShouldSendEmailAsDeputy()
    {
        ResetUserNotificationSender();

        var oldPermission = await GetEntity<InitiativeCommitteeMemberEntity>(_id);

        // ensure new expiry is set.
        GetService<FakeTimeProvider>().Advance(TimeSpan.FromDays(1));
        await DeputyClient.ResendCommitteeMemberInvitationAsync(NewValidRequest());

        // ensure token is rotated
        var member = await GetEntity<InitiativeCommitteeMemberEntity>(_id);
        member.Token.Should().NotBe(oldPermission.Token!.Value);
        member.TokenExpiry.Should().BeAfter(oldPermission.TokenExpiry!.Value);

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
            async () => await ReaderClient.ResendCommitteeMemberInvitationAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsDeputyNotAcceptedShouldFail()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.ResendCommitteeMemberInvitationAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldSetStateFailureWhenFailed()
    {
        ResetUserNotificationSender(true);
        await AssertStatus(
            async () => await AuthenticatedClient.ResendCommitteeMemberInvitationAsync(NewValidRequest()),
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
    public async Task UnknownIdShouldThrow()
    {
        var req = NewValidRequest();
        req.Id = "74bc4724-2703-4097-ab1f-b83ab8c4aa69";
        await AssertStatus(
            async () => await AuthenticatedClient.ResendCommitteeMemberInvitationAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task UnknownInitiativeIdShouldThrow()
    {
        var req = NewValidRequest();
        req.InitiativeId = "74bc4724-2703-4097-ab1f-b83ab8c4aa69";
        await AssertStatus(
            async () => await AuthenticatedClient.ResendCommitteeMemberInvitationAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(
            async () => await Client.ResendCommitteeMemberInvitationAsync(NewValidRequest()),
            StatusCode.Unauthenticated);
    }

    [Fact]
    public async Task NotMemberSignatureRequestedShouldThrow()
    {
        await ModifyDbEntities(
            (InitiativeCommitteeMemberEntity e) => e.Id == _id,
            e => e.MemberSignatureRequested = false);
        await AssertStatus(
            async () => await AuthenticatedClient.ResendCommitteeMemberInvitationAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ApprovalStateSignedShouldThrow()
    {
        await ModifyDbEntities(
            (InitiativeCommitteeMemberEntity e) => e.Id == _id,
            e => e.ApprovalState = InitiativeCommitteeMemberApprovalState.Signed);
        await AssertStatus(
            async () => await AuthenticatedClient.ResendCommitteeMemberInvitationAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowWithLockedFields()
    {
        var req = NewValidRequest();
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == Guid.Parse(req.InitiativeId),
            x => x.LockedFields = new InitiativeLockedFields
            {
                CommitteeMembers = true,
            });
        await AssertStatus(
            async () => await AuthenticatedClient.ResendCommitteeMemberInvitationAsync(req),
            StatusCode.InvalidArgument,
            "Cannot edit locked field CommitteeMembers");
    }

    private ResendCommitteeMemberInvitationRequest NewValidRequest()
    {
        return new ResendCommitteeMemberInvitationRequest
        {
            Id = _id.ToString(),
            InitiativeId = InitiativesCtStGallen.IdLegislativeInPreparation,
        };
    }
}
