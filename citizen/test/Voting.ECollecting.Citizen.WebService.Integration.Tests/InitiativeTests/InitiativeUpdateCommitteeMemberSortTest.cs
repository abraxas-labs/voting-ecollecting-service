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

public class InitiativeUpdateCommitteeMemberSortTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    private static readonly Guid _id = InitiativeCommitteeMembers.BuildGuid(
        InitiativesCtStGallen.GuidLegislativeInPreparation,
        "elena.fischer@example.com");

    public InitiativeUpdateCommitteeMemberSortTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation, InitiativesCtStGallen.GuidLegislativeReadyForRegistration));
    }

    [Fact]
    public async Task ShouldWorkMoveDown()
    {
        await AuthenticatedClient.UpdateCommitteeMemberSortAsync(NewValidRequest());

        var sortedMembers = await RunOnDb(db => db.InitiativeCommitteeMembers
            .Where(x => x.InitiativeId == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .OrderBy(x => x.SortIndex)
            .ThenBy(x => x.PoliticalLastName)
            .Select(x => new { x.Email, x.SortIndex })
            .ToListAsync());
        await Verify(sortedMembers);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await AuthenticatedClient.UpdateCommitteeMemberSortAsync(NewValidRequest());
            var result = await GetAuditTrailEntries();
            result.AuditTrailEntries.Count.Should().Be(4);
            result.AuditTrailEntries.Count(e => e.SourceEntityName == "Collections" && e.Action == "Modified")
                .Should().Be(1);
            result.AuditTrailEntries.Count(e => e.SourceEntityName == "InitiativeCommitteeMembers" && e.Action == "Modified")
                .Should().Be(3);
        });
    }

    [Fact]
    public async Task ShouldWorkMoveUp()
    {
        var req = NewValidRequest();
        req.NewIndex = 4;
        await AuthenticatedClient.UpdateCommitteeMemberSortAsync(req);

        var sortedMembers = await RunOnDb(db => db.InitiativeCommitteeMembers
            .Where(x => x.InitiativeId == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .OrderBy(x => x.SortIndex)
            .ThenBy(x => x.PoliticalLastName)
            .Select(x => new { x.Email, x.SortIndex })
            .ToListAsync());
        await Verify(sortedMembers);
    }

    [Fact]
    public async Task ShouldWorkAsDeputy()
    {
        await DeputyClient.UpdateCommitteeMemberSortAsync(NewValidRequest());

        var sortedMembers = await RunOnDb(db => db.InitiativeCommitteeMembers
            .Where(x => x.InitiativeId == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .OrderBy(x => x.SortIndex)
            .ThenBy(x => x.PoliticalLastName)
            .Select(x => new { x.Email, x.SortIndex })
            .ToListAsync());
        await Verify(sortedMembers);
    }

    [Fact]
    public async Task ShouldThrowAsReader()
    {
        await AssertStatus(
            async () => await ReaderClient.UpdateCommitteeMemberSortAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsDeputyNotAccepted()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.UpdateCommitteeMemberSortAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowInitiativeNotFound()
    {
        var req = NewValidRequest();
        req.InitiativeId = "036f7fa8-1639-4047-bb94-585305984a6e";
        await AssertStatus(
            async () => await AuthenticatedClient.UpdateCommitteeMemberSortAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowNotFound()
    {
        var req = NewValidRequest();
        req.Id = "c62e388e-54b1-4c2e-a09e-854ad63374a1";
        await AssertStatus(
            async () => await AuthenticatedClient.UpdateCommitteeMemberSortAsync(req),
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
            async () => await AuthenticatedClient.UpdateCommitteeMemberSortAsync(req),
            StatusCode.InvalidArgument,
            "Cannot edit locked field CommitteeMembers");
    }

    [Fact]
    public async Task ShouldNotAffectOtherInitiatives()
    {
        var otherMembersBeforeSort = await RunOnDb(db => db.InitiativeCommitteeMembers
            .Where(x => x.InitiativeId == InitiativesCtStGallen.GuidLegislativeReadyForRegistration)
            .OrderBy(x => x.SortIndex)
            .ThenBy(x => x.PoliticalLastName)
            .Select(x => new { x.Email, x.SortIndex })
            .ToListAsync());

        await AuthenticatedClient.UpdateCommitteeMemberSortAsync(NewValidRequest());

        var otherMembersAfterSort = await RunOnDb(db => db.InitiativeCommitteeMembers
            .Where(x => x.InitiativeId == InitiativesCtStGallen.GuidLegislativeReadyForRegistration)
            .OrderBy(x => x.SortIndex)
            .ThenBy(x => x.PoliticalLastName)
            .Select(x => new { x.Email, x.SortIndex })
            .ToListAsync());

        otherMembersBeforeSort.Should().BeEquivalentTo(otherMembersAfterSort);
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
            await AuthenticatedClient.UpdateCommitteeMemberSortAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await DeputyNotAcceptedClient.UpdateCommitteeMemberSortAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    private UpdateCommitteeMemberSortRequest NewValidRequest()
    {
        return new UpdateCommitteeMemberSortRequest
        {
            InitiativeId = InitiativesCtStGallen.IdLegislativeInPreparation,
            Id = _id.ToString(),
            NewIndex = 0,
        };
    }
}
