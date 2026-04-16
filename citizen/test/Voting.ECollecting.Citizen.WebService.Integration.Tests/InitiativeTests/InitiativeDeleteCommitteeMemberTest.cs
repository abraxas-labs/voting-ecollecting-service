// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
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

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeDeleteCommitteeMemberTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    private static readonly Guid _id = InitiativeCommitteeMembers.BuildGuid(
        InitiativesCtStGallen.GuidLegislativeInPreparation,
        "margarita@example.com");

    public InitiativeDeleteCommitteeMemberTest(TestApplicationFactory factory)
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
        var exists = await RunOnDb(db => db.InitiativeCommitteeMembers.AnyAsync(x => x.Id == _id));
        exists.Should().BeTrue();

        await AuthenticatedClient.DeleteCommitteeMemberAsync(NewValidRequest());

        exists = await RunOnDb(db => db.InitiativeCommitteeMembers.AnyAsync(x => x.Id == _id));
        exists.Should().BeFalse();

        // sort indexes should be consecutive
        var sortIndexes = await RunOnDb(db => db.InitiativeCommitteeMembers
            .Where(x => x.InitiativeId == InitiativesCtStGallen.GuidLegislativeInPreparation && x.SortIndex != null)
            .OrderBy(x => x.SortIndex)
            .Select(x => x.SortIndex!.Value)
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
            await AuthenticatedClient.DeleteCommitteeMemberAsync(NewValidRequest());
            var result = await GetAuditTrailEntries();
            result.AuditTrailEntries.Count.Should().Be(19);
            result.AuditTrailEntries.Count(e => e.SourceEntityName == "Collections" && e.Action == "Modified")
                .Should().Be(1);
            result.AuditTrailEntries.Count(e => e.SourceEntityName == "InitiativeCommitteeMembers" && e.Action == "Deleted")
                .Should().Be(1);
            result.AuditTrailEntries.Count(e => e.SourceEntityName == "InitiativeCommitteeMembers" && e.Action == "Modified")
                .Should().Be(17);
        });
    }

    [Fact]
    public async Task ShouldWorkAsDeputy()
    {
        var exists = await RunOnDb(db => db.InitiativeCommitteeMembers.AnyAsync(x => x.Id == _id));
        exists.Should().BeTrue();

        await DeputyClient.DeleteCommitteeMemberAsync(NewValidRequest());

        exists = await RunOnDb(db => db.InitiativeCommitteeMembers.AnyAsync(x => x.Id == _id));
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldThrowAsReader()
    {
        await AssertStatus(
            async () => await ReaderClient.DeleteCommitteeMemberAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsDeputyNotAccepted()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.DeleteCommitteeMemberAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowInitiativeNotFound()
    {
        var req = NewValidRequest();
        req.InitiativeId = "036f7fa8-1639-4047-bb94-585305984a6e";
        await AssertStatus(
            async () => await AuthenticatedClient.DeleteCommitteeMemberAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowNotFound()
    {
        var req = NewValidRequest();
        req.Id = "c62e388e-54b1-4c2e-a09e-854ad63374a1";
        await AssertStatus(
            async () => await AuthenticatedClient.DeleteCommitteeMemberAsync(req),
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
            async () => await AuthenticatedClient.DeleteCommitteeMemberAsync(req),
            StatusCode.InvalidArgument,
            "Cannot edit locked field CommitteeMembers");
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInCollectionStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        if (state.InPreparationOrReturnForCorrection())
        {
            await AuthenticatedClient.DeleteCommitteeMemberAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await AuthenticatedClient.DeleteCommitteeMemberAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    [Theory]
    [EnumData<InitiativeCommitteeMemberApprovalState>]
    public async Task WorksInMemberApprovalStates(InitiativeCommitteeMemberApprovalState state)
    {
        await RunOnDb(db => db.InitiativeCommitteeMembers
            .Where(x => x.Id == _id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.ApprovalState, state)));

        if (state is InitiativeCommitteeMemberApprovalState.SignatureRejected or InitiativeCommitteeMemberApprovalState.Rejected or InitiativeCommitteeMemberApprovalState.Expired)
        {
            await RunOnDb(db => db.InitiativeCommitteeMembers
                .Where(x => x.Id == _id)
                .ExecuteUpdateAsync(x => x.SetProperty(y => y.SortIndex, (int?)null)));
        }

        await AuthenticatedClient.DeleteCommitteeMemberAsync(NewValidRequest());
    }

    private DeleteCommitteeMemberRequest NewValidRequest()
    {
        return new DeleteCommitteeMemberRequest
        {
            InitiativeId = InitiativesCtStGallen.IdLegislativeInPreparation,
            Id = _id.ToString(),
        };
    }
}
