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

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class InitiativeDeleteTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeDeleteTest(TestApplicationFactory factory)
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
        var exists = await RunOnDb(db => db.Initiatives.AnyAsync(x => x.Id == InitiativesCtStGallen.GuidUnityEndedCameAbout));
        exists.Should().BeFalse();
        await Verify(GetService<UserNotificationSenderMock>().Sent);
    }

    [Fact]
    public async Task ShouldWorkAsMuOnMu()
    {
        var req = new DeleteInitiativeRequest
        {
            InitiativeId = InitiativesMuStGallen.IdUnityEndedCameAbout,
            SecondFactorTransactionId = CreateVerifiedTransaction(InitiativesMuStGallen.GuidUnityEndedCameAbout).ToString(),
        };
        await MuSgKontrollzeichenloescherClient.DeleteAsync(req);
        var exists = await RunOnDb(db => db.Initiatives.AnyAsync(x => x.Id == InitiativesMuStGallen.GuidUnityEndedCameAbout));
        exists.Should().BeFalse();
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

    [Fact]
    public async Task ShouldThrowUnknownId()
    {
        var req = new DeleteInitiativeRequest
        {
            InitiativeId = "bd54ab16-0111-4c49-961a-802d48da82b5",
            SecondFactorTransactionId = CreateVerifiedTransaction(InitiativesMuStGallen.GuidUnityEndedCameAbout).ToString(),
        };
        await AssertStatus(
            async () => await MuSgKontrollzeichenloescherClient.DeleteAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsMuOnOtherMu()
    {
        var req = new DeleteInitiativeRequest
        {
            InitiativeId = InitiativesMuStGallen.IdUnityEndedCameAbout,
            SecondFactorTransactionId = CreateVerifiedTransaction(InitiativesMuStGallen.GuidUnityEndedCameAbout).ToString(),
        };
        await AssertStatus(
            async () => await MuGoldachKontrollzeichenloescherClient.DeleteAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsCtOnMu()
    {
        var req = new DeleteInitiativeRequest
        {
            InitiativeId = InitiativesMuStGallen.IdUnityEndedCameAbout,
            SecondFactorTransactionId = CreateVerifiedTransaction(InitiativesMuStGallen.GuidUnityEndedCameAbout).ToString(),
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
        await ModifyDbEntities((InitiativeEntity c) => c.Id == Guid.Parse(req.InitiativeId), c => c.SensitiveDataExpiryDate = MockedClock.NowDateOnly.AddDays(1));
        await AssertStatus(
            async () => await CtSgKontrollzeichenloescherClient.DeleteAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowUnknownTransactionId()
    {
        var req = new DeleteInitiativeRequest
        {
            InitiativeId = InitiativesMuStGallen.IdUnityEndedCameAbout,
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
        await ModifyDbEntities((InitiativeEntity c) => c.Id == Guid.Parse(req.InitiativeId), c => c.State = CollectionState.EnabledForCollection);
        await AssertStatus(
            async () => await CtSgKontrollzeichenloescherClient.DeleteAsync(req),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new InitiativeService.InitiativeServiceClient(channel).DeleteAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Kontrollzeichenloescher;
    }

    private DeleteInitiativeRequest NewValidRequest()
    {
        var req = new DeleteInitiativeRequest
        {
            InitiativeId = InitiativesCtStGallen.IdUnityEndedCameAbout,
            SecondFactorTransactionId = CreateVerifiedTransaction(InitiativesCtStGallen.GuidUnityEndedCameAbout).ToString(),
        };
        return req;
    }

    private Guid CreateVerifiedTransaction(Guid initiativeId)
    {
        var actionId = SecondFactorTransactionActionId.Create(
            SecondFactorTransactionActionTypes.DeleteInitiative,
            initiativeId);
        return GetService<SecondFactorTransactionServiceMock>().AddVerifiedActionId(actionId);
    }
}
