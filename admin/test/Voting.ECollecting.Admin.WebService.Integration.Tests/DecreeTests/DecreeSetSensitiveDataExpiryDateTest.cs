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

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.DecreeTests;

public class DecreeSetSensitiveDataExpiryDateTest : BaseGrpcTest<DecreeService.DecreeServiceClient>
{
    public DecreeSetSensitiveDataExpiryDateTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Decrees
                .WithDecrees(DecreesCtStGallen.GuidPastWithPassedReferendum, DecreesMuStGallen.GuidPastWithNotPassedReferendum)
                .WithReferendums(ReferendumsCtStGallen.GuidPastEndedCameAbout));
    }

    [Fact]
    public async Task ShouldWorkDecreeAsCt()
    {
        var req = NewValidRequest();
        await CtSgKontrollzeichenloescherClient.SetSensitiveDataExpiryDateAsync(req);
        var decree = await RunOnDb(db =>
            db.Decrees.SingleAsync(x => x.Id == DecreesCtStGallen.GuidPastWithPassedReferendum));
        decree.SensitiveDataExpiryDate.Should().NotBeNull();
        decree.SensitiveDataExpiryDate.Should().Be(req.SensitiveDataExpiryDate.ToDate());
    }

    [Fact]
    public async Task ShouldWorkDecreeAsMu()
    {
        var req = NewValidRequest(x => x.DecreeId = DecreesMuStGallen.IdPastWithNotPassedReferendum);
        await MuSgKontrollzeichenloescherClient.SetSensitiveDataExpiryDateAsync(req);
        var decree = await RunOnDb(db =>
            db.Decrees.SingleAsync(x => x.Id == DecreesMuStGallen.GuidPastWithNotPassedReferendum));
        decree.SensitiveDataExpiryDate.Should().NotBeNull();
        decree.SensitiveDataExpiryDate.Should().Be(req.SensitiveDataExpiryDate.ToDate());
    }

    [Fact]
    public async Task ShouldThrowAsCtOnMu()
    {
        var req = NewValidRequest(x => x.DecreeId = DecreesMuStGallen.IdPastWithNotPassedReferendum);
        await AssertStatus(
            async () => await CtSgKontrollzeichenloescherClient.SetSensitiveDataExpiryDateAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsOtherMuOnMu()
    {
        var req = NewValidRequest(x => x.DecreeId = DecreesMuStGallen.IdPastWithNotPassedReferendum);
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
        var req = NewValidRequest(x => x.DecreeId = "1ae5ce67-eb7a-4699-8297-4e07547ac2fe");
        await AssertStatus(
            async () => await CtSgKontrollzeichenloescherClient.SetSensitiveDataExpiryDateAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task NotEndedStateShouldThrow()
    {
        var req = NewValidRequest();
        await ModifyDbEntities(
            (DecreeEntity c) => c.Id == DecreesCtStGallen.GuidPastWithPassedReferendum,
            c => c.State = DecreeState.CollectionApplicable);
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

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgKontrollzeichenloescherClient.SetSensitiveDataExpiryDateAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new DecreeService.DecreeServiceClient(channel).SetSensitiveDataExpiryDateAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles() => [Roles.Kontrollzeichenloescher];

    private static SetDecreeSensitiveDataExpiryDateRequest NewValidRequest(Action<SetDecreeSensitiveDataExpiryDateRequest>? customizer = null)
    {
        var req = new SetDecreeSensitiveDataExpiryDateRequest
        {
            DecreeId = DecreesCtStGallen.IdPastWithPassedReferendum,
            SensitiveDataExpiryDate = MockedClock.UtcNowDate.AddDays(30).ToProtoDate(),
        };
        customizer?.Invoke(req);
        return req;
    }
}
