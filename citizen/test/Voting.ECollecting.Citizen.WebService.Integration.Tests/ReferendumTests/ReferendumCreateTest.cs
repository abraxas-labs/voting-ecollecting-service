// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Citizen.WebService.Exceptions;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.ReferendumTests;

public class ReferendumCreateTest : BaseGrpcTest<ReferendumService.ReferendumServiceClient>
{
    private readonly ReferendumService.ReferendumServiceClient _client;

    public ReferendumCreateTest(TestApplicationFactory factory)
        : base(factory)
    {
        _client = CreateCitizenClient("default-user-id", acrValue: CitizenAuthMockDefaults.AcrValue100);
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Referendums);
    }

    [Fact]
    public async Task TestShouldWork()
    {
        var response = await _client.CreateAsync(NewValidRequest());
        var referendum = await RunOnDb(db => db.Referendums.Include(x => x.Permissions).FirstAsync(x => x.Id == Guid.Parse(response.Id)));
        referendum.SetPeriodState(GetService<TimeProvider>().GetUtcTodayDateOnly());
        await Verify(referendum);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await _client.CreateAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task TestShouldWorkWithDecreeId()
    {
        var response = await _client.CreateAsync(NewValidRequest(x => x.DecreeId = DecreesCtStGallen.IdFutureNoReferendum));
        var referendum = await RunOnDb(db => db.Referendums.Include(x => x.Permissions).FirstAsync(x => x.Id == Guid.Parse(response.Id)));
        referendum.DecreeId.Should().Be(DecreesCtStGallen.IdFutureNoReferendum);
        referendum.SetPeriodState(GetService<TimeProvider>().GetUtcTodayDateOnly());
        await Verify(referendum);
    }

    [Fact]
    public async Task InsufficientAcr()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.CreateAsync(NewValidRequest()),
            StatusCode.PermissionDenied,
            nameof(InsufficientAcrException));
    }

    [Fact]
    public async Task ReferendumShouldInheritMaxElectronicSignatureCountFromDecree()
    {
        var decreeId = DecreesCtStGallen.IdFutureNoReferendum;
        await ModifyDbEntities<DecreeEntity>(
            x => x.Id == Guid.Parse(decreeId),
            x => x.MaxElectronicSignatureCount = 9999);

        var response = await _client.CreateAsync(NewValidRequest(x => x.DecreeId = decreeId));
        var referendum = await RunOnDb(db => db.Referendums.FirstAsync(x => x.Id == Guid.Parse(response.Id)));
        referendum.MaxElectronicSignatureCount.Should().Be(9999);
    }

    [Fact]
    public async Task MoreThanOneReferendumOnDecreePerUserShouldThrow()
    {
        await _client.CreateAsync(NewValidRequest(x => x.DecreeId = DecreesCtStGallen.IdInCollectionWithoutReferendum));
        await AssertStatus(
            async () => await _client.CreateAsync(NewValidRequest(x => x.DecreeId = DecreesCtStGallen.IdInCollectionWithoutReferendum)),
            StatusCode.AlreadyExists);
    }

    [Fact]
    public async Task MoreThanMaxReferendumOnDecreeShouldThrow()
    {
        await ModifyDbEntities<ReferendumEntity>(
            _ => true,
            x =>
            {
                x.DecreeId = DecreesCtStGallen.GuidInCollectionWithReferendum;
                x.AuditInfo.CreatedById = "some-user";
            });

        await AssertStatus(
            async () => await _client.CreateAsync(NewValidRequest(x => x.DecreeId = DecreesCtStGallen.IdInCollectionWithReferendum)),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(async () => await Client.CreateAsync(new CreateReferendumRequest()), StatusCode.Unauthenticated);
    }

    private CreateReferendumRequest NewValidRequest(Action<CreateReferendumRequest>? customizer = null)
    {
        var request = new CreateReferendumRequest
        {
            Description = "Sammlung gegen das Abwassergesetz",
        };
        customizer?.Invoke(request);
        return request;
    }
}
