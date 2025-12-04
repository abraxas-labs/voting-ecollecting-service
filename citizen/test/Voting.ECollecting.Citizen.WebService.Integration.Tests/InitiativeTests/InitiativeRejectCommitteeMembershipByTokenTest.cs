// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Microsoft.EntityFrameworkCore;
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

public class InitiativeRejectCommitteeMembershipByTokenTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    private static readonly Guid _id =
        InitiativeCommitteeMembers.BuildGuid(
            InitiativesCtStGallen.GuidLegislativeInPreparation,
            "margarita@example.com");

    private static readonly UrlToken _token =
        InitiativeCommitteeMembers.BuildToken(
            InitiativesCtStGallen.GuidLegislativeInPreparation,
            "margarita@example.com");

    public InitiativeRejectCommitteeMembershipByTokenTest(TestApplicationFactory factory)
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
        await Client.RejectCommitteeMembershipByTokenAsync(NewValidRequest());
        var membership = await RunOnDb(db => db.InitiativeCommitteeMembers.SingleAsync(x => x.Id == _id));
        await Verify(membership);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await Client.RejectCommitteeMembershipByTokenAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task NotFound()
    {
        var req = new RejectCommitteeMembershipRequest { Token = UrlToken.New() };
        await AssertStatus(
            async () => await Client.RejectCommitteeMembershipByTokenAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ExpiredShouldThrow()
    {
        await ModifyDbEntities(
            (InitiativeCommitteeMemberEntity e) => e.Token == _token,
            e => e.TokenExpiry = MockedClock.GetDate(-4));

        await AssertStatus(
            async () => await Client.RejectCommitteeMembershipByTokenAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task CollectionState(CollectionState state)
    {
        await ModifyDbEntities<InitiativeEntity>(
            e => e.Id == InitiativesCtStGallen.GuidLegislativeInPreparation,
            e => e.State = state);

        if (state.InPreparationOrReturnForCorrection())
        {
            await Client.RejectCommitteeMembershipByTokenAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await Client.RejectCommitteeMembershipByTokenAsync(NewValidRequest()),
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

        if (state == InitiativeCommitteeMemberApprovalState.Requested)
        {
            await Client.RejectCommitteeMembershipByTokenAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await Client.RejectCommitteeMembershipByTokenAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    private RejectCommitteeMembershipRequest NewValidRequest()
    {
        return new RejectCommitteeMembershipRequest { Token = _token.ToString() };
    }
}
