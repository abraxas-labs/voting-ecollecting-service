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

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeSetInPreparationTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    private readonly InitiativeService.InitiativeServiceClient _client;

    public InitiativeSetInPreparationTest(TestApplicationFactory factory)
        : base(factory)
    {
        _client = CreateCitizenClient("default-user-id", acrValue: CitizenAuthMockDefaults.AcrValue100);
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation, InitiativesCtStGallen.GuidLegislativeInPaperSubmission, InitiativesCtStGallen.GuidLegislativeInPaperSubmissionReader));
    }

    [Fact]
    public async Task TestShouldWork()
    {
        var response = await _client.SetInPreparationAsync(NewValidRequest());
        var initiative = await RunOnDb(db => db.Initiatives.FirstAsync(x => x.Id == Guid.Parse(response.Id)));
        initiative.SetPeriodState(GetService<TimeProvider>().GetUtcNowDateTime());
        await Verify(initiative);
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
    public async Task TestInvalidGovernmentDecisionNumberShouldThrow()
    {
        var request = NewValidRequest(r => r.GovernmentDecisionNumber = "123");
        await AssertStatus(
            async () => await _client.SetInPreparationAsync(request),
            StatusCode.NotFound,
            $"Initiative with government decision number {request.GovernmentDecisionNumber} not found");
    }

    [Fact]
    public async Task TestWithoutAdmissibilityDecisionShouldThrow()
    {
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesCtStGallen.GuidLegislativeInPaperSubmission,
            x => x.AdmissibilityDecisionState = null);
        var request = NewValidRequest();
        await AssertStatus(
            async () => await _client.SetInPreparationAsync(request),
            StatusCode.NotFound,
            $"Initiative with government decision number {request.GovernmentDecisionNumber} not found");
    }

    [Fact]
    public async Task TestInitiativeAlreadyInPreparationShouldThrow()
    {
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesCtStGallen.GuidLegislativeInPaperSubmission,
            x => x.State = CollectionState.InPreparation);
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesCtStGallen.GuidLegislativeInPaperSubmissionReader,
            x => x.State = CollectionState.InPreparation);
        var request = NewValidRequest();
        await AssertStatus(
            async () => await _client.SetInPreparationAsync(request),
            StatusCode.AlreadyExists,
            $"Initiative with government decision number {request.GovernmentDecisionNumber} is already in preparation");
    }

    [Fact]
    public async Task TestInitiativeAdmissibilityDecisionRejectedShouldThrow()
    {
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesCtStGallen.GuidLegislativeInPaperSubmission,
            x => x.AdmissibilityDecisionState = AdmissibilityDecisionState.Rejected);
        var request = NewValidRequest();
        await AssertStatus(
            async () => await _client.SetInPreparationAsync(request),
            StatusCode.InvalidArgument,
            "Initiative admissibility decision state cannot be rejected");
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

    private SetInitiativeInPreparationRequest NewValidRequest(Action<SetInitiativeInPreparationRequest>? customizer = null)
    {
        var request = new SetInitiativeInPreparationRequest
        {
            GovernmentDecisionNumber = "CH-123.4567",
        };
        customizer?.Invoke(request);
        return request;
    }
}
