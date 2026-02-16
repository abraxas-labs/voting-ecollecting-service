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
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.ReferendumTests;

public class ReferendumUpdateDecreeTest : BaseGrpcTest<ReferendumService.ReferendumServiceClient>
{
    public ReferendumUpdateDecreeTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Referendums);
    }

    [Fact]
    public async Task TestShouldUpdateDecreeAsCreator()
    {
        await AuthenticatedClient.UpdateDecreeAsync(NewValidRequest());
        var referendum = await RunOnDb(db =>
            db.Referendums.FirstAsync(x => x.Id == ReferendumsCtStGallen.GuidInPreparation));
        referendum.SetPeriodState(GetService<TimeProvider>().GetUtcTodayDateOnly());
        await Verify(referendum);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await AuthenticatedClient.UpdateDecreeAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task TestShouldUpdateDecreeAsDeputy()
    {
        await DeputyClient.UpdateDecreeAsync(NewValidRequest());
        var referendum = await RunOnDb(db => db.Referendums.FirstAsync(x => x.Id == ReferendumsCtStGallen.GuidInPreparation));
        referendum.SetPeriodState(GetService<TimeProvider>().GetUtcTodayDateOnly());
        await Verify(referendum);
    }

    [Fact]
    public async Task AsReaderShouldFail()
    {
        await AssertStatus(
            async () => await ReaderClient.UpdateDecreeAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsDeputyNotAcceptedShouldFail()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.UpdateDecreeAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.UpdateDecreeAsync(NewValidRequest(x =>
                x.Id = "096b6b8f-2399-44cc-bf0b-26485a6c8aa8")),
            StatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(async () => await Client.UpdateAsync(new UpdateReferendumRequest()), StatusCode.Unauthenticated);
    }

    [Fact]
    public async Task MoreThanOneReferendumOnDecreePerUserShouldFail()
    {
        await DeputyClient.CreateAsync(new CreateReferendumRequest
        {
            DecreeId = DecreesCtStGallen.IdInCollectionWithoutReferendum,
            Description = "Sammlung gegen das Abwassergesetz",
        });
        await AssertStatus(
            async () => await DeputyClient.UpdateDecreeAsync(NewValidRequest(x => x.DecreeId = DecreesCtStGallen.IdInCollectionWithoutReferendum)),
            StatusCode.AlreadyExists);
    }

    [Fact]
    public async Task MoreThanMaxReferendumOnDecreeShouldFail()
    {
        await ModifyDbEntities<ReferendumEntity>(
            x => x.Id != ReferendumsCtStGallen.GuidInPreparation,
            x =>
            {
                x.DecreeId = DecreesCtStGallen.GuidInCollectionWithReferendum;
                x.AuditInfo.CreatedById = "some-user";
            });

        await AssertStatus(
            async () => await AuthenticatedClient.UpdateDecreeAsync(NewValidRequest(x => x.DecreeId = DecreesCtStGallen.IdInCollectionWithReferendum)),
            StatusCode.InvalidArgument);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInState(CollectionState state)
    {
        // referendum never is in state ReturnForCorrection, so we can skip it
        if (state == CollectionState.ReturnedForCorrection)
        {
            return;
        }

        await ModifyDbEntities<ReferendumEntity>(
            e => e.Id == ReferendumsCtStGallen.GuidInPreparation,
            e => e.State = state);

        if (state == CollectionState.InPreparation)
        {
            await AuthenticatedClient.UpdateDecreeAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await AuthenticatedClient.UpdateDecreeAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    private UpdateReferendumDecreeRequest NewValidRequest(Action<UpdateReferendumDecreeRequest>? customizer = null)
    {
        var request = new UpdateReferendumDecreeRequest
        {
            Id = ReferendumsCtStGallen.IdInPreparation,
            DecreeId = DecreesCtStGallen.IdFutureNoReferendum,
        };
        customizer?.Invoke(request);
        return request;
    }
}
