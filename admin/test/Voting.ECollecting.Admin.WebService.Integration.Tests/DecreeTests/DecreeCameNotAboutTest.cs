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
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.Lib.Testing.Mocks;
using CollectionCameNotAboutReason = Voting.ECollecting.Proto.Shared.V1.Enums.CollectionCameNotAboutReason;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.DecreeTests;

public class DecreeCameNotAboutTest : BaseGrpcTest<DecreeService.DecreeServiceClient>
{
    public DecreeCameNotAboutTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Referendums);
    }

    [Fact]
    public async Task ShouldWork()
    {
        await CtSgStammdatenverwalterClient.CameNotAboutAsync(NewValidRequest());

        var decree = await RunOnDb(db => db.Decrees
            .Include(x => x.Collections)
            .FirstAsync(x => x.Id == DecreesCh.GuidInCollection));
        decree.State.Should().Be(DecreeState.EndedCameNotAbout);
        decree.CameNotAboutReason.Should().Be(Shared.Domain.Enums.CollectionCameNotAboutReason.MinSignatureCountNotReached);
        decree.SensitiveDataExpiryDate.Should().Be(MockedClock.NowDateOnly.AddDays(365));
        decree.Collections.Should().AllSatisfy(x => x.State.Should().Be(CollectionState.EndedCameNotAbout));

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == ReferendumsCh.GuidInCollection || x.TemplateBag.CollectionId == ReferendumsCh.GuidSignatureSheetsSubmitted)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessages = await RunOnDb(async db => await db.CollectionMessages.Where(x => x.CollectionId == ReferendumsCh.GuidInCollection || x.CollectionId == ReferendumsCh.GuidSignatureSheetsSubmitted).OrderBy(x => x.CollectionId).ToListAsync());
        await Verify(new { userNotifications, collectionMessages });
    }

    [Fact]
    public async Task ShouldWorkAsMuAdmin()
    {
        await MuSgStammdatenverwalterClient.CameNotAboutAsync(NewValidRequest(x => x.DecreeId = DecreesMuStGallen.IdInCollectionWithReferendum));

        var decree = await RunOnDb(db => db.Decrees
            .Include(x => x.Collections)
            .FirstAsync(x => x.Id == DecreesMuStGallen.GuidInCollectionWithReferendum));
        decree.State.Should().Be(DecreeState.EndedCameNotAbout);
        decree.CameNotAboutReason.Should().Be(Shared.Domain.Enums.CollectionCameNotAboutReason.MinSignatureCountNotReached);
        decree.SensitiveDataExpiryDate.Should().Be(MockedClock.NowDateOnly.AddDays(365));
        decree.Collections.Should().AllSatisfy(x => x.State.Should().Be(CollectionState.EndedCameNotAbout));

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == ReferendumsMuStGallen.GuidInCollectionActive || x.TemplateBag.CollectionId == ReferendumsMuStGallen.GuidSignatureSheetsSubmitted)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessages = await RunOnDb(async db => await db.CollectionMessages.Where(x => x.CollectionId == ReferendumsMuStGallen.GuidInCollectionActive || x.CollectionId == ReferendumsMuStGallen.GuidSignatureSheetsSubmitted).OrderBy(x => x.CollectionId).ToListAsync());
        await Verify(new { userNotifications, collectionMessages });
    }

    [Fact]
    public async Task AsMuOnCtCollectionShouldFail()
    {
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.CameNotAboutAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsMuOnOtherMuCollectionShouldFail()
    {
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.CameNotAboutAsync(NewValidRequest(x => x.DecreeId = DecreesMuStGallen.IdInCollectionWithReferendum)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsCtOnMuCollectionShouldFail()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.CameNotAboutAsync(NewValidRequest(x => x.DecreeId = ReferendumsMuStGallen.IdInCollectionActive)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task WithSensitiveDataExpiryDateInPastShouldFail()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.CameNotAboutAsync(NewValidRequest(x => x.SensitiveDataExpiryDate = MockedClock.GetDate(-1).ToProtoDate())),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task CollectionNotStartedShouldFail()
    {
        await ModifyDbEntities<DecreeEntity>(
            e => e.Id == DecreesCh.GuidInCollection,
            e => e.CollectionStartDate = MockedClock.NowDateOnly.AddDays(2));

        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.CameNotAboutAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStammdatenverwalterClient.CameNotAboutAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
        => await new DecreeService.DecreeServiceClient(channel)
            .CameNotAboutAsync(NewValidRequest());

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private CameNotAboutDecreeRequest NewValidRequest(Action<CameNotAboutDecreeRequest>? customizer = null)
    {
        var request = new CameNotAboutDecreeRequest
        {
            DecreeId = DecreesCh.IdInCollection,
            Reason = CollectionCameNotAboutReason.MinSignatureCountNotReached,
            SensitiveDataExpiryDate = MockedClock.GetDate(365).ToProtoDate(),
        };
        customizer?.Invoke(request);
        return request;
    }
}
