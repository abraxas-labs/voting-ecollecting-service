// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.ECollecting.Admin.Core.Services;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.Lib.Scheduler;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class InitiativeCommitteeMemberExpiryJobTest : BaseDbTest
{
    public InitiativeCommitteeMemberExpiryJobTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCh.GuidInPreparation));
    }

    [Fact]
    public async Task ShouldExpireCommitteeMembers()
    {
        var expectedStates =
            new List<(Guid Id, InitiativeCommitteeMemberApprovalState StateBeforeJob, InitiativeCommitteeMemberApprovalState StateAfterJob)>
            {
                (InitiativeCommitteeMembers.BuildGuid(
                        InitiativesCh.GuidInPreparation,
                        "expired@example.com"),
                    InitiativeCommitteeMemberApprovalState.Expired,
                    InitiativeCommitteeMemberApprovalState.Expired),
                (InitiativeCommitteeMembers.BuildGuid(
                        InitiativesCh.GuidInPreparation,
                        "expired-not-updated@example.com"),
                    InitiativeCommitteeMemberApprovalState.Requested,
                    InitiativeCommitteeMemberApprovalState.Expired),
                (InitiativeCommitteeMembers.BuildGuid(
                        InitiativesCh.GuidInPreparation,
                        "sophia.schwarz@example.com"),
                    InitiativeCommitteeMemberApprovalState.Approved,
                    InitiativeCommitteeMemberApprovalState.Approved),
            };

        foreach (var (id, stateBeforeJob, _) in expectedStates)
        {
            var item = await RunOnDb(db => db.InitiativeCommitteeMembers.SingleAsync(x => x.Id == id));
            item.ApprovalState.Should().Be(stateBeforeJob);
        }

        await GetService<JobRunner>().RunJob<InitiativeCommitteeMemberExpiryJob>(CancellationToken.None);

        foreach (var (id, _, stateAfterJob) in expectedStates)
        {
            var item = await RunOnDb(db => db.InitiativeCommitteeMembers.SingleAsync(x => x.Id == id));
            item.ApprovalState.Should().Be(stateAfterJob);
        }
    }

    [Fact]
    public async Task ShouldUpdateSortIndex()
    {
        var initiativeId = InitiativesCh.GuidInPreparation;
        var memberId = InitiativeCommitteeMembers.BuildGuid(
            initiativeId,
            "elena.fischer@example.com");

        await ModifyDbEntities<InitiativeCommitteeMemberEntity>(
            x => x.Id == memberId,
            x => x.TokenExpiry = MockedClock.UtcNowDate.AddDays(-1));

        await GetService<JobRunner>().RunJob<InitiativeCommitteeMemberExpiryJob>(CancellationToken.None);

        var member = await RunOnDb(db => db.InitiativeCommitteeMembers.SingleAsync(x => x.Id == memberId));
        member.SortIndex.Should().BeNull();

        var activeMembers = await RunOnDb(db => db.InitiativeCommitteeMembers
            .Where(x => x.InitiativeId == initiativeId && x.SortIndex != null)
            .Select(x => x.SortIndex!.Value)
            .OrderBy(x => x)
            .ToListAsync());

        activeMembers.Should().Equal(Enumerable.Range(0, activeMembers.Count));
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        var initiativeId = InitiativesCh.GuidInPreparation;
        var memberId = InitiativeCommitteeMembers.BuildGuid(
            initiativeId,
            "elena.fischer@example.com");

        await ModifyDbEntities<InitiativeCommitteeMemberEntity>(
            x => x.Id == memberId,
            x => x.TokenExpiry = MockedClock.UtcNowDate.AddDays(-1));

        await RunInAuditTrailTestScope(async () =>
        {
            await using var scope = GetService<IServiceScopeFactory>().CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<JobRunner>().RunJob<InitiativeCommitteeMemberExpiryJob>(CancellationToken.None);
            await Verify(await GetAuditTrailEntries());
        });
    }
}
