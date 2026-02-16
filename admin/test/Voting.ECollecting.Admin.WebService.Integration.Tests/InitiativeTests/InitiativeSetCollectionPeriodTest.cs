// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.Admin.WebService.Integration.Tests.Mocks;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;
using Voting.Lib.Testing.Mocks;
using CollectionState = Voting.ECollecting.Shared.Domain.Enums.CollectionState;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class InitiativeSetCollectionPeriodTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeSetCollectionPeriodTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPaperSubmission, InitiativesMuStGallen.GuidPreRecorded));
    }

    [Fact]
    public async Task ShouldSetCollectionPeriod()
    {
        var req = NewValidRequest();
        await CtSgStammdatenverwalterClient.SetCollectionPeriodAsync(req);

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.Municipalities!.OrderBy(y => y.Bfs))
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPaperSubmission));
        initiative.State.Should().Be(CollectionState.PreRecorded);
        initiative.CollectionStartDate.Should().Be(req.CollectionStartDate.ToDate());
        initiative.CollectionEndDate.Should().Be(req.CollectionEndDate.ToDate());
        initiative.MacKeyId.Should().NotBeNullOrEmpty();
        initiative.EncryptionKeyId.Should().NotBeNullOrEmpty();

        initiative.SetPeriodState(GetService<TimeProvider>().GetUtcTodayDateOnly());
        await Verify(initiative);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStammdatenverwalterClient.SetCollectionPeriodAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task AsMuOnOwnCollectionShouldWork()
    {
        var req = NewValidRequest(x => x.Id = InitiativesMuStGallen.IdPreRecorded);
        await MuSgStammdatenverwalterClient.SetCollectionPeriodAsync(req);

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.Municipalities!.OrderBy(y => y.Bfs))
            .FirstAsync(x => x.Id == InitiativesMuStGallen.GuidPreRecorded));
        initiative.State.Should().Be(CollectionState.PreRecorded);
        initiative.CollectionStartDate.Should().Be(req.CollectionStartDate.ToDate());
        initiative.CollectionEndDate.Should().Be(req.CollectionEndDate.ToDate());
        initiative.MacKeyId.Should().NotBeNullOrEmpty();
        initiative.EncryptionKeyId.Should().NotBeNullOrEmpty();

        initiative.SetPeriodState(GetService<TimeProvider>().GetUtcTodayDateOnly());
        await Verify(initiative);
    }

    [Fact]
    public async Task CollectionStartDateInPastShouldFail()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.SetCollectionPeriodAsync(NewValidRequest(x => x.CollectionStartDate = GetService<TimeProvider>().GetUtcTodayDateOnly().AddDays(-1).ToProtoDate())),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task CollectionEndDateBeforeStartDateShouldFail()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.SetCollectionPeriodAsync(NewValidRequest(x =>
            {
                x.CollectionStartDate = MockedClock.GetDate().ToProtoDate();
                x.CollectionEndDate = MockedClock.GetDate(-1).ToProtoDate();
            })),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task AsMuOnCtCollectionShouldFail()
    {
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.SetCollectionPeriodAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsMuOnOtherMuCollectionShouldFail()
    {
        var req = NewValidRequest(x => x.Id = InitiativesMuStGallen.IdPreRecorded);
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.SetCollectionPeriodAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsCtOnMuCollectionShouldFail()
    {
        var req = NewValidRequest(x => x.Id = InitiativesMuStGallen.IdPreRecorded);
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.SetCollectionPeriodAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ElectronicSubmissionShouldFail()
    {
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesCtStGallen.GuidLegislativeInPaperSubmission,
            x => x.IsElectronicSubmission = true);

        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.SetCollectionPeriodAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task CollectionPeriodAlreadySetShouldFail()
    {
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesCtStGallen.GuidLegislativeInPaperSubmission,
            x =>
            {
                x.CollectionStartDate = GetService<TimeProvider>().GetUtcTodayDateOnly().AddDays(1);
                x.CollectionEndDate = GetService<TimeProvider>().GetUtcTodayDateOnly().AddDays(31);
            });

        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.SetCollectionPeriodAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.SetCollectionPeriodAsync(NewValidRequest(x => x.Id = "2a5db5e2-99f2-4d0d-b874-ecfdb7e63d83")),
            StatusCode.NotFound);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPaperSubmission)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        if (state is CollectionState.PreRecorded)
        {
            await CtSgStammdatenverwalterClient.SetCollectionPeriodAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await CtSgStammdatenverwalterClient.SetCollectionPeriodAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new InitiativeService.InitiativeServiceClient(channel)
            .SetCollectionPeriodAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private SetCollectionPeriodInitiativeRequest NewValidRequest(Action<SetCollectionPeriodInitiativeRequest>? customizer = null)
    {
        var request = new SetCollectionPeriodInitiativeRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeInPaperSubmission,
            CollectionStartDate = MockedClock.GetDate(10).ToProtoDate(),
            CollectionEndDate = MockedClock.GetDate(50).ToProtoDate(),
        };
        customizer?.Invoke(request);
        return request;
    }
}
