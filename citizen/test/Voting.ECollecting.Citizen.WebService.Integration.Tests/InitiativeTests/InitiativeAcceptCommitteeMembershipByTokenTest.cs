// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Citizen.Adapter.VotingStimmregister;
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

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeAcceptCommitteeMembershipByTokenTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    private const string Email = "margarita@example.com";

    private static readonly Guid _id =
        InitiativeCommitteeMembers.BuildGuid(
            InitiativesCtStGallen.GuidLegislativeInPreparation,
            Email);

    private static readonly UrlToken _token =
        InitiativeCommitteeMembers.BuildToken(
            InitiativesCtStGallen.GuidLegislativeInPreparation,
            Email);

    public InitiativeAcceptCommitteeMembershipByTokenTest(TestApplicationFactory factory)
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
        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            email: Email,
            ssn: VotingStimmregisterAdapterMock.VotingRightPerson12Ssn);
        var accepted = await client.AcceptCommitteeMembershipByTokenAsync(NewValidRequest());
        accepted.Accepted.Should().BeTrue();

        var membership = await RunOnDb(db => db.InitiativeCommitteeMembers.SingleAsync(x => x.Id == _id));
        await Verify(membership);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            email: Email,
            ssn: VotingStimmregisterAdapterMock.VotingRightPerson12Ssn);
            await client.AcceptCommitteeMembershipByTokenAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldReturnFalseSsnWithoutVotingRight()
    {
        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            email: Email,
            ssn: VotingStimmregisterAdapterMock.NoVotingRightPerson1Ssn);
        var accepted = await client.AcceptCommitteeMembershipByTokenAsync(NewValidRequest());
        accepted.Accepted.Should().BeFalse();

        var membership = await RunOnDb(db => db.InitiativeCommitteeMembers.SingleAsync(x => x.Id == _id));
        await Verify(membership);
    }

    [Fact]
    public async Task InsufficientAcrThrows()
    {
        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue100,
            email: Email,
            ssn: VotingStimmregisterAdapterMock.VotingRightPerson12Ssn);
        await AssertStatus(
            async () => await client.AcceptCommitteeMembershipByTokenAsync(NewValidRequest()),
            StatusCode.PermissionDenied,
            "InsufficientAcrException");
    }

    [Fact]
    public async Task EmailMismatchThrows()
    {
        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            email: "foo" + Email,
            ssn: VotingStimmregisterAdapterMock.VotingRightPerson12Ssn);
        await AssertStatus(
            async () => await client.AcceptCommitteeMembershipByTokenAsync(NewValidRequest()),
            StatusCode.PermissionDenied,
            "EmailDoesNotMatchException");
    }

    [Fact]
    public async Task NotFound()
    {
        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            email: Email,
            ssn: VotingStimmregisterAdapterMock.VotingRightPerson12Ssn);
        var req = new AcceptCommitteeMembershipRequest { Token = UrlToken.New() };
        await AssertStatus(
            async () => await client.AcceptCommitteeMembershipByTokenAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ExpiredShouldThrow()
    {
        await ModifyDbEntities(
            (InitiativeCommitteeMemberEntity e) => e.Token == _token,
            e => e.TokenExpiry = MockedClock.GetDate(-4));

        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            email: Email,
            ssn: VotingStimmregisterAdapterMock.VotingRightPerson12Ssn);
        await AssertStatus(
            async () => await client.AcceptCommitteeMembershipByTokenAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task CollectionState(CollectionState state)
    {
        await ModifyDbEntities<InitiativeEntity>(
            e => e.Id == InitiativesCtStGallen.GuidLegislativeInPreparation,
            e => e.State = state);

        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            email: Email,
            ssn: VotingStimmregisterAdapterMock.VotingRightPerson12Ssn);
        if (state.InPreparationOrReturnForCorrection())
        {
            await client.AcceptCommitteeMembershipByTokenAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await client.AcceptCommitteeMembershipByTokenAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    [Theory]
    [EnumData<InitiativeCommitteeMemberApprovalState>]
    public async Task ApprovalState(InitiativeCommitteeMemberApprovalState state)
    {
        await ModifyDbEntities<InitiativeCommitteeMemberEntity>(
            e => e.Id == _id,
            e => e.ApprovalState = state);

        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            email: Email,
            ssn: VotingStimmregisterAdapterMock.VotingRightPerson12Ssn);
        if (state == InitiativeCommitteeMemberApprovalState.Requested)
        {
            await client.AcceptCommitteeMembershipByTokenAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await client.AcceptCommitteeMembershipByTokenAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    private AcceptCommitteeMembershipRequest NewValidRequest()
    {
        return new AcceptCommitteeMembershipRequest { Token = _token.ToString() };
    }
}
