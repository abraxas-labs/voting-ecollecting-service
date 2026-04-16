// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.ECollecting.Admin.WebService.Integration.Tests.Mocks;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.Lib.Iam.SecondFactor.Exceptions;
using Voting.Lib.Iam.SecondFactor.Models;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.DecreeTests;

public class DecreeDeleteTest : BaseGrpcTest<DecreeService.DecreeServiceClient>
{
    public DecreeDeleteTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Default); // seed all to ensure cascade deletion works correctly
    }

    [Fact]
    public async Task ShouldWorkAsCt()
    {
        ResetUserNotificationSender();
        await CtSgKontrollzeichenloescherClient.DeleteAsync(NewValidRequest());
        var exists = await RunOnDb(db => db.Decrees.AnyAsync(x => x.Id == DecreesCtStGallen.GuidPastWithPassedReferendum));
        exists.Should().BeFalse();
        await Verify(GetService<UserNotificationSenderMock>().Sent);
    }

    [Fact]
    public async Task ShouldCascadeDeleteReferendumsWhenDeleting()
    {
        var decreeId = DecreesCtStGallen.GuidPastWithPassedReferendum;
        var referendumIds = await RunOnDb(db => db.Referendums
            .Where(x => x.DecreeId == decreeId)
            .Select(x => x.Id)
            .ToListAsync());

        referendumIds.Should().NotBeEmpty();

        await CtSgKontrollzeichenloescherClient.DeleteAsync(NewValidRequest());

        var decreeExists = await RunOnDb(db => db.Decrees.AnyAsync(x => x.Id == decreeId));
        decreeExists.Should().BeFalse();

        var remainingReferendumCount = await RunOnDb(db => db.Referendums
            .CountAsync(x => referendumIds.Contains(x.Id)));
        remainingReferendumCount.Should().Be(0);
    }

    [Fact]
    public async Task ShouldWorkAsMuOnMu()
    {
        var req = new DeleteDecreeRequest
        {
            DecreeId = DecreesMuStGallen.IdPastWithNotPassedReferendum,
            SecondFactorTransactionId = CreateVerifiedTransaction(DecreesMuStGallen.GuidPastWithNotPassedReferendum).ToString(),
        };
        await MuSgKontrollzeichenloescherClient.DeleteAsync(req);
        var exists = await RunOnDb(db => db.Decrees.AnyAsync(x => x.Id == DecreesMuStGallen.GuidPastWithNotPassedReferendum));
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldThrowUnknownId()
    {
        var req = new DeleteDecreeRequest
        {
            DecreeId = "bd54ab16-0111-4c49-961a-802d48da82b5",
            SecondFactorTransactionId = CreateVerifiedTransaction(DecreesMuStGallen.GuidPastWithNotPassedReferendum).ToString(),
        };
        await AssertStatus(
            async () => await MuSgKontrollzeichenloescherClient.DeleteAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsMuOnOtherMu()
    {
        var req = new DeleteDecreeRequest
        {
            DecreeId = DecreesMuStGallen.IdPastWithNotPassedReferendum,
            SecondFactorTransactionId = CreateVerifiedTransaction(DecreesMuStGallen.GuidPastWithNotPassedReferendum).ToString(),
        };
        await AssertStatus(
            async () => await MuGoldachKontrollzeichenloescherClient.DeleteAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsCtOnMu()
    {
        var req = new DeleteDecreeRequest
        {
            DecreeId = DecreesMuStGallen.IdPastWithNotPassedReferendum,
            SecondFactorTransactionId = CreateVerifiedTransaction(DecreesMuStGallen.GuidPastWithNotPassedReferendum).ToString(),
        };
        await AssertStatus(
            async () => await CtSgKontrollzeichenloescherClient.DeleteAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsMuOnCt()
    {
        await AssertStatus(
            async () => await MuGoldachKontrollzeichenloescherClient.DeleteAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsCtWithSensitiveDataExpiryDateInFuture()
    {
        var req = NewValidRequest();
        await ModifyDbEntities((DecreeEntity c) => c.Id == Guid.Parse(req.DecreeId), c => c.SensitiveDataExpiryDate = MockedClock.NowDateOnly.AddDays(1));
        await AssertStatus(
            async () => await CtSgKontrollzeichenloescherClient.DeleteAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowUnknownTransactionId()
    {
        var req = new DeleteDecreeRequest
        {
            DecreeId = DecreesMuStGallen.IdPastWithNotPassedReferendum,
            SecondFactorTransactionId = "d720c4ad-6949-48ad-aea2-54134cc6e740",
        };
        await AssertStatus(
            async () => await MuSgKontrollzeichenloescherClient.DeleteAsync(req),
            StatusCode.PermissionDenied,
            nameof(SecondFactorTransactionNotVerifiedException));
    }

    [Fact]
    public async Task ShouldThrowNotEnded()
    {
        var req = NewValidRequest();
        await ModifyDbEntities((DecreeEntity c) => c.Id == Guid.Parse(req.DecreeId), c => c.State = DecreeState.CollectionApplicable);
        await AssertStatus(
            async () => await CtSgKontrollzeichenloescherClient.DeleteAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgKontrollzeichenloescherClient.DeleteAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new DecreeService.DecreeServiceClient(channel).DeleteAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Kontrollzeichenloescher;
    }

    private DeleteDecreeRequest NewValidRequest()
    {
        var req = new DeleteDecreeRequest
        {
            DecreeId = DecreesCtStGallen.IdPastWithPassedReferendum,
            SecondFactorTransactionId = CreateVerifiedTransaction(DecreesCtStGallen.GuidPastWithPassedReferendum).ToString(),
        };
        return req;
    }

    private Guid CreateVerifiedTransaction(Guid decreeId)
    {
        var actionId = SecondFactorTransactionActionId.Create(
            SecondFactorTransactionActionTypes.DeleteDecree,
            decreeId);
        return GetService<SecondFactorTransactionServiceMock>().AddVerifiedActionId(actionId);
    }
}
