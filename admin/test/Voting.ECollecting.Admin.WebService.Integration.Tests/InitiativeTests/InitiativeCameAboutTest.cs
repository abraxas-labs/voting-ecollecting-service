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
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class InitiativeCameAboutTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeCameAboutTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCh.GuidSignatureSheetsSubmitted, InitiativesMuStGallen.GuidSignatureSheetsSubmitted));
    }

    [Fact]
    public async Task ShouldWork()
    {
        await CtSgStammdatenverwalterClient.CameAboutAsync(NewValidRequest());

        var initiative = await RunOnDb(db => db.Initiatives
            .FirstAsync(x => x.Id == InitiativesCh.GuidSignatureSheetsSubmitted));
        initiative.State.Should().Be(CollectionState.EndedCameAbout);
        initiative.SensitiveDataExpiryDate.Should().Be(DateOnly.FromDateTime(MockedClock.GetDate(365)));

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCh.GuidSignatureSheetsSubmitted)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesCh.GuidSignatureSheetsSubmitted));
        await Verify(new { userNotifications, collectionMessage });
    }

    [Fact]
    public async Task ShouldWorkAsMuAdmin()
    {
        await MuSgStammdatenverwalterClient.CameAboutAsync(NewValidRequest(x => x.InitiativeId = InitiativesMuStGallen.IdSignatureSheetsSubmitted));

        var initiative = await RunOnDb(db => db.Initiatives
            .FirstAsync(x => x.Id == InitiativesMuStGallen.GuidSignatureSheetsSubmitted));
        initiative.State.Should().Be(CollectionState.EndedCameAbout);
        initiative.SensitiveDataExpiryDate.Should().Be(DateOnly.FromDateTime(MockedClock.GetDate(365)));

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesMuStGallen.GuidSignatureSheetsSubmitted)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesMuStGallen.GuidSignatureSheetsSubmitted));
        await Verify(new { userNotifications, collectionMessage });
    }

    [Fact]
    public async Task AsMuOnCtCollectionShouldFail()
    {
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.CameAboutAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsMuOnOtherMuCollectionShouldFail()
    {
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.CameAboutAsync(NewValidRequest(x => x.InitiativeId = InitiativesMuStGallen.IdSignatureSheetsSubmitted)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsCtOnMuCollectionShouldFail()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.CameAboutAsync(NewValidRequest(x => x.InitiativeId = InitiativesMuStGallen.IdSignatureSheetsSubmitted)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task WithSensitiveDataExpiryDateInPastShouldFail()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.CameAboutAsync(NewValidRequest(x => x.SensitiveDataExpiryDate = MockedClock.GetDate(-1).ToProtoDate())),
            StatusCode.InvalidArgument);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCh.GuidSignatureSheetsSubmitted)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        if (state is CollectionState.SignatureSheetsSubmitted)
        {
            await CtSgStammdatenverwalterClient.CameAboutAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await CtSgStammdatenverwalterClient.CameAboutAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStammdatenverwalterClient.CameAboutAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
        => await new InitiativeService.InitiativeServiceClient(channel)
            .CameAboutAsync(NewValidRequest());

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private CameAboutInitiativeRequest NewValidRequest(Action<CameAboutInitiativeRequest>? customizer = null)
    {
        var request = new CameAboutInitiativeRequest
        {
            InitiativeId = InitiativesCh.IdSignatureSheetsSubmitted,
            SensitiveDataExpiryDate = MockedClock.GetDate(365).ToProtoDate(),
        };
        customizer?.Invoke(request);
        return request;
    }
}
