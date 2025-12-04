// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;
using CollectionService = Voting.ECollecting.Proto.Admin.Services.V1.CollectionService;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionDeleteWithdrawnTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionDeleteWithdrawnTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Collections
                .WithInitiatives(InitiativesCtStGallen.GuidLegislativeWithdrawn, InitiativesMuStGallen.GuidWithdrawn)
                .WithReferendums(ReferendumsCtStGallen.GuidWithdrawn));
    }

    [Fact]
    public async Task ShouldDeleteInitiative()
    {
        await CtSgStammdatenverwalterClient.DeleteWithdrawnAsync(new DeleteWithdrawnCollectionRequest
        {
            CollectionId = InitiativesCtStGallen.IdLegislativeWithdrawn,
        });

        var exists = await RunOnDb(db => db.Collections.AnyAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeWithdrawn));
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStammdatenverwalterClient.DeleteWithdrawnAsync(new DeleteWithdrawnCollectionRequest
            {
                CollectionId = InitiativesCtStGallen.IdLegislativeWithdrawn,
            });

            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldDeleteReferendum()
    {
        await CtSgStammdatenverwalterClient.DeleteWithdrawnAsync(new DeleteWithdrawnCollectionRequest
        {
            CollectionId = ReferendumsCtStGallen.IdWithdrawn,
        });

        var exists = await RunOnDb(db => db.Collections.AnyAsync(x => x.Id == ReferendumsCtStGallen.GuidWithdrawn));
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldDeleteMu()
    {
        await MuSgStammdatenverwalterClient.DeleteWithdrawnAsync(new DeleteWithdrawnCollectionRequest
        {
            CollectionId = InitiativesMuStGallen.IdWithdrawn,
        });

        var exists = await RunOnDb(db => db.Collections.AnyAsync(x => x.Id == InitiativesMuStGallen.GuidWithdrawn));
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldDeleteMuAsCt()
    {
        await CtSgStammdatenverwalterClient.DeleteWithdrawnAsync(new DeleteWithdrawnCollectionRequest
        {
            CollectionId = InitiativesMuStGallen.IdWithdrawn,
        });

        var exists = await RunOnDb(db => db.Collections.AnyAsync(x => x.Id == InitiativesMuStGallen.GuidWithdrawn));
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteCtAsMuShouldThrow()
    {
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.DeleteWithdrawnAsync(new DeleteWithdrawnCollectionRequest
            {
                CollectionId = InitiativesCtStGallen.IdLegislativeWithdrawn,
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteOtherMuAsMuShouldThrow()
    {
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.DeleteWithdrawnAsync(new DeleteWithdrawnCollectionRequest
            {
                CollectionId = InitiativesMuStGallen.IdWithdrawn,
            }),
            StatusCode.NotFound);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInState(CollectionState state)
    {
        await ModifyDbEntities<InitiativeEntity>(
            e => e.Id == InitiativesCtStGallen.GuidLegislativeWithdrawn,
            e => e.State = state);

        var req = new DeleteWithdrawnCollectionRequest
        {
            CollectionId = InitiativesCtStGallen.IdLegislativeWithdrawn,
        };
        if (state == CollectionState.Withdrawn)
        {
            await CtSgStammdatenverwalterClient.DeleteWithdrawnAsync(req);
        }
        else
        {
            await AssertStatus(
                async () => await CtSgStammdatenverwalterClient.DeleteWithdrawnAsync(req),
                StatusCode.NotFound);
        }
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionService.CollectionServiceClient(channel).DeleteWithdrawnAsync(new DeleteWithdrawnCollectionRequest
        {
            CollectionId = InitiativesCtStGallen.IdLegislativeWithdrawn,
        });
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }
}
