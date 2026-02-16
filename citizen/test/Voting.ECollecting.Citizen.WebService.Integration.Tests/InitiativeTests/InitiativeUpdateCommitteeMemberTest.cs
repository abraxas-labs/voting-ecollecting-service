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

public class InitiativeUpdateCommitteeMemberTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    private static readonly Guid _id = InitiativeCommitteeMembers.BuildGuid(
        InitiativesCtStGallen.GuidLegislativeInPreparation,
        "margarita@example.com");

    public InitiativeUpdateCommitteeMemberTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation, InitiativesCtStGallen.GuidLegislativeReturnedForCorrection));
    }

    [Fact]
    public async Task ShouldWorkWithoutUpdatingEmail()
    {
        ResetUserNotificationSender();

        await AuthenticatedClient.UpdateCommitteeMemberAsync(NewValidRequest());

        var member = await RunOnDb(db => db.InitiativeCommitteeMembers
            .Include(x => x.Permission)
            .SingleAsync(x => x.Id == _id));

        await Verify(member);

        // should not send update email
        SentUserNotifications.Should().BeEmpty();
    }

    [Fact]
    public async Task AddRoleAndUpdateRole()
    {
        ResetUserNotificationSender();

        // add role
        var req1 = NewValidRequest();
        req1.Role = CollectionPermissionRole.Deputy;
        await AuthenticatedClient.UpdateCommitteeMemberAsync(req1);
        var permissionToken1 = await RunOnDb(db => db.InitiativeCommitteeMembers
            .Where(x => x.Id == _id)
            .Select(x => x.Permission!.Token!.Value)
            .SingleAsync());

        // update role without updating email
        var req2 = NewValidRequest();
        req2.Role = CollectionPermissionRole.Reader;
        await AuthenticatedClient.UpdateCommitteeMemberAsync(req2);
        var permissionToken2 = await RunOnDb(db => db.InitiativeCommitteeMembers
            .Where(x => x.Id == _id)
            .Select(x => x.Permission!.Token!.Value)
            .SingleAsync());
        permissionToken1.Should().Be(permissionToken2);

        var member = await RunOnDb(db => db.InitiativeCommitteeMembers
            .Include(x => x.Permission)
            .SingleAsync(x => x.Id == _id));

        var sent = SentUserNotifications;
        await Verify(new { member, sent }).ScrubUrlTokens();
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await AuthenticatedClient.UpdateCommitteeMemberAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task AddRoleAndUpdateEmailShouldResend()
    {
        ResetUserNotificationSender();

        // add role
        var req1 = NewValidRequest();
        req1.Role = CollectionPermissionRole.Deputy;
        await AuthenticatedClient.UpdateCommitteeMemberAsync(req1);
        var permissionToken1 = await RunOnDb(db => db.InitiativeCommitteeMembers
            .Where(x => x.Id == _id)
            .Select(x => x.Permission!.Token!.Value)
            .SingleAsync());

        // update email
        var req2 = NewValidRequest();
        req2.Role = CollectionPermissionRole.Deputy;
        req2.Email = "updated@example.com";
        await AuthenticatedClient.UpdateCommitteeMemberAsync(req2);
        var permissionToken2 = await RunOnDb(db => db.InitiativeCommitteeMembers
            .Where(x => x.Id == _id)
            .Select(x => x.Permission!.Token!.Value)
            .SingleAsync());
        permissionToken1.Should().NotBe(permissionToken2);

        var member = await RunOnDb(db => db.InitiativeCommitteeMembers
            .Include(x => x.Permission)
            .SingleAsync(x => x.Id == _id));

        var sent = SentUserNotifications;
        await Verify(new { member, sent }).ScrubUrlTokens();
    }

    [Fact]
    public async Task DeleteRoleAndEmailShouldWork()
    {
        ResetUserNotificationSender();

        var req = NewValidRequest();
        req.RequestMemberSignature = false;
        req.Email = string.Empty;
        req.Role = CollectionPermissionRole.Unspecified;
        await AuthenticatedClient.UpdateCommitteeMemberAsync(req);

        // should not send update email
        SentUserNotifications.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldThrowWithApprovalOnPaper()
    {
        var req = NewValidRequest();
        req.InitiativeId = InitiativesCtStGallen.IdLegislativeReturnedForCorrection;
        req.Id = InitiativeCommitteeMembers.BuildGuid(
            InitiativesCtStGallen.GuidLegislativeReturnedForCorrection,
            "margarita@example.com").ToString();
        req.RequestMemberSignature = true;
        req.Role = CollectionPermissionRole.Unspecified;
        req.Email = string.Empty;
        await AssertStatus(
            async () => await AuthenticatedClient.UpdateCommitteeMemberAsync(req),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task ShouldThrowWithOwnerRole()
    {
        var req = NewValidRequest();
        req.Role = CollectionPermissionRole.Owner;
        await AssertStatus(
            async () => await AuthenticatedClient.UpdateCommitteeMemberAsync(req),
            StatusCode.AlreadyExists);
    }

    [Fact]
    public async Task ShouldThrowWithUsedEmail()
    {
        var req = NewValidRequest();
        req.Email = "jeanine.mueller@example.com";
        req.Role = CollectionPermissionRole.Deputy;
        await AssertStatus(
            async () => await AuthenticatedClient.UpdateCommitteeMemberAsync(req),
            StatusCode.AlreadyExists,
            nameof(CollectionPermissionAlreadyExistsException));
    }

    [Fact]
    public async Task ShouldWorkWithManualApprovalOnPaper()
    {
        ResetUserNotificationSender();

        var req = NewValidRequest();
        req.InitiativeId = InitiativesCtStGallen.IdLegislativeReturnedForCorrection;
        req.Id = InitiativeCommitteeMembers.BuildGuid(
            InitiativesCtStGallen.GuidLegislativeReturnedForCorrection,
            "margarita@example.com").ToString();
        req.RequestMemberSignature = false;
        req.Role = CollectionPermissionRole.Unspecified;
        req.Email = string.Empty;
        await AuthenticatedClient.UpdateCommitteeMemberAsync(req);

        // should not send update email
        SentUserNotifications.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldWorkAsDeputy()
    {
        ResetUserNotificationSender();

        await DeputyClient.UpdateCommitteeMemberAsync(NewValidRequest());

        var member = await RunOnDb(db => db.InitiativeCommitteeMembers
            .Include(x => x.Permission)
            .SingleAsync(x => x.Id == _id));

        await Verify(member);

        // should not send update email
        SentUserNotifications.Should().BeEmpty();
    }

    [Fact]
    public async Task ShouldThrowReader()
    {
        await AssertStatus(
            async () => await ReaderClient.UpdateCommitteeMemberAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowDeputyNotAccepted()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.UpdateCommitteeMemberAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowUnauthenticated()
    {
        await AssertStatus(
            async () => await Client.UpdateCommitteeMemberAsync(NewValidRequest()),
            StatusCode.Unauthenticated);
    }

    [Theory]
    [EnumData<InitiativeCommitteeMemberApprovalState>]
    public async Task WorksInApprovalState(InitiativeCommitteeMemberApprovalState state)
    {
        await ModifyDbEntities(
            (InitiativeCommitteeMemberEntity e) => e.Id == _id,
            e => e.ApprovalState = state);

        if (state == InitiativeCommitteeMemberApprovalState.Requested)
        {
            await AuthenticatedClient.UpdateCommitteeMemberAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await AuthenticatedClient.UpdateCommitteeMemberAsync(NewValidRequest()),
                StatusCode.InvalidArgument);
        }
    }

    [Fact]
    public async Task ShouldWorkInApprovalStateSelfSigned()
    {
        await ModifyDbEntities(
            (InitiativeCommitteeMemberEntity e) => e.Id == _id,
            e =>
            {
                e.ApprovalState = InitiativeCommitteeMemberApprovalState.Signed;
                e.MemberSignatureRequested = false;
                e.SignatureType = InitiativeCommitteeMemberSignatureType.UploadedSignature;
            });

        await AuthenticatedClient.UpdateCommitteeMemberAsync(NewValidRequest());
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
            await AuthenticatedClient.UpdateCommitteeMemberAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await DeputyNotAcceptedClient.UpdateCommitteeMemberAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    [Fact]
    public async Task ShouldThrowUnknownInitiativeId()
    {
        var req = NewValidRequest();
        req.InitiativeId = "48a5b8f1-663b-4108-acba-7b601e440964";
        await AssertStatus(
            async () => await AuthenticatedClient.UpdateCommitteeMemberAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowUnknownId()
    {
        var req = NewValidRequest();
        req.Id = "c9242e65-91bc-4deb-8115-9ce56f107a19";
        await AssertStatus(
            async () => await AuthenticatedClient.UpdateCommitteeMemberAsync(req),
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
            async () => await AuthenticatedClient.UpdateCommitteeMemberAsync(req),
            StatusCode.InvalidArgument,
            "Cannot edit locked field CommitteeMembers");
    }

    private UpdateCommitteeMemberRequest NewValidRequest()
    {
        return new UpdateCommitteeMemberRequest
        {
            Id = _id.ToString(),
            Email = "margarita@example.com",
            FirstName = "Foo (updated)",
            LastName = "Bar (updated)",
            PoliticalFirstName = "Foo (pol) (updated)",
            PoliticalLastName = "Bar (pol) (updated)",
            DateOfBirth = MockedClock.GetTimestamp(-55 * 365),
            InitiativeId = InitiativesCtStGallen.IdLegislativeInPreparation,
            RequestMemberSignature = true,
            PoliticalDuty = "Protokollführer (updated)",
            PoliticalResidence = Bfs.GetName(Bfs.MunicipalityGoldach),
            Bfs = Bfs.MunicipalityGoldach,
            Street = "Bahnhofstrasse (updated)",
            HouseNumber = "2a",
            ZipCode = "9001",
            Role = CollectionPermissionRole.Unspecified,
        };
    }
}
