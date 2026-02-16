// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Citizen.WebService.Exceptions;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Test.MockedData;
using CollectionState = Voting.ECollecting.Shared.Domain.Enums.CollectionState;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.ReferendumTests;

public class ReferendumSetInPreparationTest : BaseGrpcTest<ReferendumService.ReferendumServiceClient>
{
    private readonly ReferendumService.ReferendumServiceClient _client;

    public ReferendumSetInPreparationTest(TestApplicationFactory factory)
        : base(factory)
    {
        _client = CreateCitizenClient("default-user-id", acrValue: CitizenAuthMockDefaults.AcrValue100);
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Referendums.WithReferendums(ReferendumsCtStGallen.GuidInPaperSubmission));
    }

    [Fact]
    public async Task TestShouldWork()
    {
        var response = await _client.SetInPreparationAsync(NewValidRequest());
        var referendum = await RunOnDb(db => db.Referendums.Include(x => x.Permissions).FirstAsync(x => x.Id == Guid.Parse(response.Id)));
        referendum.SetPeriodState(GetService<TimeProvider>().GetUtcTodayDateOnly());
        await Verify(referendum);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await _client.SetInPreparationAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task TestInvalidReferendumNumberShouldThrow()
    {
        var request = NewValidRequest(r => r.SecureIdNumber = "555555555555");
        await AssertStatus(
            async () => await _client.SetInPreparationAsync(request),
            StatusCode.NotFound,
            $"Referendum with number {request.SecureIdNumber} not found");
    }

    [Fact]
    public async Task TestElectronicSubmissionShouldThrow()
    {
        await ModifyDbEntities<ReferendumEntity>(
            x => x.Id == ReferendumsCtStGallen.GuidInPaperSubmission,
            x => x.IsElectronicSubmission = true);
        var request = NewValidRequest();
        await AssertStatus(
            async () => await _client.SetInPreparationAsync(request),
            StatusCode.NotFound,
            $"Referendum with number {request.SecureIdNumber} not found");
    }

    [Fact]
    public async Task TestReferendumAlreadyInPreparationShouldThrow()
    {
        await ModifyDbEntities<ReferendumEntity>(
            x => x.Id == ReferendumsCtStGallen.GuidInPaperSubmission,
            x => x.State = CollectionState.InPreparation);
        var request = NewValidRequest();
        await AssertStatus(
            async () => await _client.SetInPreparationAsync(request),
            StatusCode.AlreadyExists,
            $"Referendum with number {request.SecureIdNumber} is already in preparation");
    }

    [Fact]
    public async Task TestDisabledDomainOfInfluenceTypeShouldThrow()
    {
        var request = NewValidRequest(x => x.SecureIdNumber = "AAAAAAAAAAAB");
        await WithEnabledDomainOfInfluenceTypes([], async () => await AssertStatus(
            async () => await _client.SetInPreparationAsync(request),
            StatusCode.NotFound,
            $"Referendum with number {request.SecureIdNumber} not found"));
    }

    [Fact]
    public Task InsufficientAcr()
    {
        var client = CreateCitizenClient("default-user-id", acrValue: "unknown");
        return AssertStatus(
            async () => await client.SetInPreparationAsync(NewValidRequest()),
            StatusCode.PermissionDenied,
            nameof(InsufficientAcrException));
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(async () => await Client.SetInPreparationAsync(NewValidRequest()), StatusCode.Unauthenticated);
    }

    private SetReferendumInPreparationRequest NewValidRequest(Action<SetReferendumInPreparationRequest>? customizer = null)
    {
        var request = new SetReferendumInPreparationRequest
        {
            SecureIdNumber = "AAAAAAAAAAAA",
        };
        customizer?.Invoke(request);
        return request;
    }
}
