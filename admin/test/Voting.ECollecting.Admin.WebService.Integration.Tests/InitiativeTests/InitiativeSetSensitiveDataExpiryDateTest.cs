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

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class InitiativeSetSensitiveDataExpiryDateTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeSetSensitiveDataExpiryDateTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Initiatives
                .WithInitiatives(InitiativesCtStGallen.GuidConstitutionalEndedCameNotAbout, InitiativesMuStGallen.GuidUnityEndedCameAbout)
                .WithReferendums(ReferendumsCtStGallen.GuidPastEndedCameAbout));
    }

    [Fact]
    public async Task ShouldWorkInitiativeAsCt()
    {
        var req = NewValidRequest();
        await CtSgKontrollzeichenloescherClient.SetSensitiveDataExpiryDateAsync(req);
        var initiative = await RunOnDb(db =>
            db.Initiatives.SingleAsync(x => x.Id == InitiativesCtStGallen.GuidConstitutionalEndedCameNotAbout));
        initiative.SensitiveDataExpiryDate.Should().NotBeNull();
        initiative.SensitiveDataExpiryDate.Should().Be(req.SensitiveDataExpiryDate.ToDate());
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgKontrollzeichenloescherClient.SetSensitiveDataExpiryDateAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldWorkInitiativeAsMu()
    {
        var req = NewValidRequest(x => x.InitiativeId = InitiativesMuStGallen.IdUnityEndedCameAbout);
        await MuSgKontrollzeichenloescherClient.SetSensitiveDataExpiryDateAsync(req);
        var initiative = await RunOnDb(db =>
            db.Initiatives.SingleAsync(x => x.Id == InitiativesMuStGallen.GuidUnityEndedCameAbout));
        initiative.SensitiveDataExpiryDate.Should().NotBeNull();
        initiative.SensitiveDataExpiryDate.Should().Be(req.SensitiveDataExpiryDate.ToDate());
    }

    [Fact]
    public async Task ShouldThrowAsCtOnMu()
    {
        var req = NewValidRequest(x => x.InitiativeId = InitiativesMuStGallen.IdUnityEndedCameAbout);
        await AssertStatus(
            async () => await CtSgKontrollzeichenloescherClient.SetSensitiveDataExpiryDateAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsOtherMuOnMu()
    {
        var req = NewValidRequest(x => x.InitiativeId = InitiativesMuStGallen.IdUnityEndedCameAbout);
        await AssertStatus(
            async () => await MuGoldachKontrollzeichenloescherClient.SetSensitiveDataExpiryDateAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsMuOnCt()
    {
        var req = NewValidRequest();
        await AssertStatus(
            async () => await MuSgKontrollzeichenloescherClient.SetSensitiveDataExpiryDateAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task UnknownIdShouldThrow()
    {
        var req = NewValidRequest(x => x.InitiativeId = "1ae5ce67-eb7a-4699-8297-4e07547ac2fe");
        await AssertStatus(
            async () => await CtSgKontrollzeichenloescherClient.SetSensitiveDataExpiryDateAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task NotEndedStateShouldThrow()
    {
        var req = NewValidRequest();
        await ModifyDbEntities(
            (InitiativeEntity c) => c.Id == InitiativesCtStGallen.GuidConstitutionalEndedCameNotAbout,
            c => c.State = CollectionState.EnabledForCollection);
        await AssertStatus(
            async () => await CtSgKontrollzeichenloescherClient.SetSensitiveDataExpiryDateAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task DateNotInTheFutureShouldThrow()
    {
        var req = NewValidRequest();
        req.SensitiveDataExpiryDate = MockedClock.UtcNowDate.Date.ToProtoDate();
        await AssertStatus(
            async () => await CtSgKontrollzeichenloescherClient.SetSensitiveDataExpiryDateAsync(req),
            StatusCode.InvalidArgument,
            "Sensitive data expiry date must be in the future.");
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new InitiativeService.InitiativeServiceClient(channel).SetSensitiveDataExpiryDateAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles() => [Roles.Kontrollzeichenloescher];

    private static SetInitiativeSensitiveDataExpiryDateRequest NewValidRequest(Action<SetInitiativeSensitiveDataExpiryDateRequest>? customizer = null)
    {
        var req = new SetInitiativeSensitiveDataExpiryDateRequest
        {
            InitiativeId = InitiativesCtStGallen.IdConstitutionalEndedCameNotAbout,
            SensitiveDataExpiryDate = MockedClock.UtcNowDate.AddDays(30).ToProtoDate(),
        };
        customizer?.Invoke(req);
        return req;
    }
}
