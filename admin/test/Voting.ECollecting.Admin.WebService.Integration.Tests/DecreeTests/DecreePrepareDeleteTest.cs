// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
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

public class DecreePrepareDeleteTest : BaseGrpcTest<DecreeService.DecreeServiceClient>
{
    public DecreePrepareDeleteTest(TestApplicationFactory factory)
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
    public async Task ShouldWorkAsCt()
    {
        var resp = await CtSgKontrollzeichenloescherClient.PrepareDeleteAsync(NewValidRequest());
        var createdTransactions = GetService<SecondFactorTransactionServiceMock>().CreatedTransactions;
        createdTransactions.Should().HaveCount(1);
        resp.Id.Should().Be(createdTransactions[0].TransactionId.ToString());
        await Verify(new { resp, createdTransactions });
    }

    [Fact]
    public async Task ShouldWorkAsMu()
    {
        var resp = await MuSgKontrollzeichenloescherClient.PrepareDeleteAsync(new PrepareDeleteDecreeRequest
        {
            DecreeId = DecreesMuStGallen.IdPastWithNotPassedReferendum,
        });
        var createdTransactions = GetService<SecondFactorTransactionServiceMock>().CreatedTransactions;
        createdTransactions.Should().HaveCount(1);
        resp.Id.Should().Be(createdTransactions[0].TransactionId.ToString());
        await Verify(new { resp, createdTransactions });
    }

    [Fact]
    public async Task ShouldThrowUnknownId()
    {
        await AssertStatus(
            async () => await MuSgKontrollzeichenloescherClient.PrepareDeleteAsync(
                new PrepareDeleteDecreeRequest { DecreeId = "204dfdfc-8e4e-44b9-8caf-733fcd05cd50", }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsCtOnMu()
    {
        await AssertStatus(
            async () => await CtSgKontrollzeichenloescherClient.PrepareDeleteAsync(new PrepareDeleteDecreeRequest { DecreeId = DecreesMuStGallen.IdPastWithNotPassedReferendum }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsMuOnOtherMu()
    {
        await AssertStatus(
            async () => await MuGoldachKontrollzeichenloescherClient.PrepareDeleteAsync(new PrepareDeleteDecreeRequest { DecreeId = DecreesMuStGallen.IdPastWithNotPassedReferendum }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsMuOnCt()
    {
        await AssertStatus(
            async () => await MuSgKontrollzeichenloescherClient.PrepareDeleteAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowNotEnded()
    {
        var req = NewValidRequest();
        await ModifyDbEntities((DecreeEntity c) => c.Id == Guid.Parse(req.DecreeId), c => c.State = DecreeState.CollectionApplicable);
        await AssertStatus(
            async () => await CtSgKontrollzeichenloescherClient.PrepareDeleteAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsCtWithSensitiveDataExpiryDateInFuture()
    {
        var req = NewValidRequest();
        await ModifyDbEntities((DecreeEntity c) => c.Id == Guid.Parse(req.DecreeId), c => c.SensitiveDataExpiryDate = MockedClock.NowDateOnly.AddDays(1));
        await AssertStatus(
            async () => await CtSgKontrollzeichenloescherClient.PrepareDeleteAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new DecreeService.DecreeServiceClient(channel)
            .PrepareDeleteAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles() => [Roles.Kontrollzeichenloescher];

    private static PrepareDeleteDecreeRequest NewValidRequest()
    {
        return new PrepareDeleteDecreeRequest
        {
            DecreeId = DecreesCtStGallen.IdPastWithPassedReferendum,
        };
    }
}
