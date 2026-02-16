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
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation, InitiativesCtStGallen.GuidLegislativeInPaperSubmission));
    }

    [Fact]
    public async Task TestShouldWork()
    {
        var response = await _client.SetInPreparationAsync(NewValidRequest());
        var initiative = await RunOnDb(db => db.Initiatives.Include(x => x.Permissions).FirstAsync(x => x.Id == Guid.Parse(response.Id)));
        initiative.SetPeriodState(GetService<TimeProvider>().GetUtcTodayDateOnly());
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
    public async Task TestInvalidSecureIdNumberShouldThrow()
    {
        var request = NewValidRequest(r => r.SecureIdNumber = "555555555555");
        await AssertStatus(
            async () => await _client.SetInPreparationAsync(request),
            StatusCode.NotFound,
            $"Initiative with number {request.SecureIdNumber} not found");
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
            $"Initiative with number {request.SecureIdNumber} not found");
    }

    [Fact]
    public async Task TestInitiativeAlreadyInPreparationShouldThrow()
    {
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesCtStGallen.GuidLegislativeInPaperSubmission,
            x => x.State = CollectionState.InPreparation);
        var request = NewValidRequest();
        await AssertStatus(
            async () => await _client.SetInPreparationAsync(request),
            StatusCode.AlreadyExists,
            $"Initiative with number {request.SecureIdNumber} is already in preparation");
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
    public async Task TestDisabledDomainOfInfluenceTypeShouldThrow()
    {
        var request = NewValidRequest();
        await WithEnabledDomainOfInfluenceTypes([], async () => await AssertStatus(
            async () => await _client.SetInPreparationAsync(request),
            StatusCode.NotFound,
            $"Initiative with number {request.SecureIdNumber} not found"));
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
            SecureIdNumber = "AAAAAAAXXAAA",
        };
        customizer?.Invoke(request);
        return request;
    }
}
