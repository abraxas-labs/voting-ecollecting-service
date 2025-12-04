// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.DecreeTests;

public class DecreeDeletePublishedTest : BaseGrpcTest<DecreeService.DecreeServiceClient>
{
    public DecreeDeletePublishedTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Referendums);
    }

    [Fact]
    public async Task TestAsCtTenantShouldWork()
    {
        await CtSgStammdatenverwalterClient.DeletePublishedAsync(NewValidRequest());
        var exists = await RunOnDb(db => db.Decrees.AnyAsync(x => x.Id == DecreesCh.GuidFutureNoReferendum));
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task TestAsCtTenantWhenInCollectionShouldThrow()
    {
        var request = NewValidRequest(r => r.Id = DecreesCtStGallen.IdInCollectionWithReferendum);
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.DeletePublishedAsync(request),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestAsCtTenantWhenNoPermissionShouldThrow()
    {
        var request = NewValidRequest(r => r.Id = DecreesMuStGallen.IdFutureNoReferendum);
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.DeletePublishedAsync(request),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestAsCtTenantWhenInvalidIdShouldThrowNotFound()
    {
        var request = NewValidRequest(r => r.Id = "f4590e18-0439-4402-8695-d5696c2a21a7");
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.DeletePublishedAsync(request),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestAsMuTenantShouldWork()
    {
        var request = NewValidRequest(r => r.Id = DecreesMuStGallen.IdFutureNoReferendum);
        await MuSgStammdatenverwalterClient.DeletePublishedAsync(request);
        var decree = await RunOnDb(db => db.Decrees.FirstOrDefaultAsync(x => x.Id == DecreesMuStGallen.GuidFutureNoReferendum));
        decree.Should().BeNull();
    }

    [Fact]
    public async Task TestAsMuTenantWhenInCollectionShouldThrow()
    {
        var request = NewValidRequest(r => r.Id = DecreesMuStGallen.IdInCollectionWithReferendum);
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.DeletePublishedAsync(request),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestAsMuTenantWhenNoPermissionOnOtherMunicipalityShouldThrowNotFound()
    {
        var request = NewValidRequest(r => r.Id = DecreesMuGoldach.IdFutureNoReferendum);
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.DeletePublishedAsync(request),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestAsMuTenantWhenNoPermissionOnCantonShouldThrowNotFound()
    {
        var request = NewValidRequest(r => r.Id = DecreesCtStGallen.IdFutureNoReferendum);
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.DeletePublishedAsync(request),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStammdatenverwalterClient.DeletePublishedAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
        => await new DecreeService.DecreeServiceClient(channel)
            .DeletePublishedAsync(NewValidRequest());

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private DeletePublishedDecreeRequest NewValidRequest(Action<DeletePublishedDecreeRequest>? customizer = null)
    {
        var request = new DeletePublishedDecreeRequest
        {
            Id = DecreesCh.IdFutureNoReferendum,
        };
        customizer?.Invoke(request);
        return request;
    }
}
