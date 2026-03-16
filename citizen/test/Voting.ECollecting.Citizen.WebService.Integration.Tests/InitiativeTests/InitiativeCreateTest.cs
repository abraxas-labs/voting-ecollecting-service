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
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.ModelBuilders;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeCreateTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    private readonly InitiativeService.InitiativeServiceClient _client;

    public InitiativeCreateTest(TestApplicationFactory factory)
        : base(factory)
    {
        _client = CreateCitizenClient("default-user-id", acrValue: CitizenAuthMockDefaults.AcrValue100);
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.DomainOfInfluences);
    }

    [Fact]
    public async Task TestChInitiativeShouldWork()
    {
        var response = await _client.CreateAsync(NewValidChRequest());
        var initiative = await RunOnDb(db => db.Initiatives.Include(x => x.Permissions).FirstAsync(x => x.Id == Guid.Parse(response.Id)));
        initiative.SetPeriodState(GetService<TimeProvider>().GetUtcTodayDateOnly());
        await Verify(initiative);
    }

    [Fact]
    public async Task InitiativeMaxElectronicSignaturePercentShouldWork()
    {
        await ModifyDbEntities<DomainOfInfluenceEntity>(
            x => x.Bfs == Bfs.Switzerland,
            x => x.InitiativeMaxElectronicSignaturePercent = 50);

        var response = await _client.CreateAsync(NewValidChRequest());
        var initiative = await RunOnDb(db => db.Initiatives.IgnoreQueryFilters().FirstAsync(x => x.Id == Guid.Parse(response.Id)));
        initiative.MaxElectronicSignatureCount.Should().Be(50_000);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await _client.CreateAsync(NewValidChRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task TestCtInitiativeShouldWork()
    {
        var response = await _client.CreateAsync(NewValidCtRequest());
        var initiative = await RunOnDb(db => db.Initiatives.Include(x => x.Permissions).FirstAsync(x => x.Id == Guid.Parse(response.Id)));
        initiative.SetPeriodState(GetService<TimeProvider>().GetUtcTodayDateOnly());
        await Verify(initiative);
    }

    [Fact]
    public async Task TestMuInitiativeShouldWork()
    {
        var response = await _client.CreateAsync(NewValidMuRequest());
        var initiative = await RunOnDb(db => db.Initiatives.Include(x => x.Permissions).FirstAsync(x => x.Id == Guid.Parse(response.Id)));
        initiative.SetPeriodState(GetService<TimeProvider>().GetUtcTodayDateOnly());
        await Verify(initiative);
    }

    [Fact]
    public async Task TestChInitiativeWithCtSubTypeShouldThrow()
    {
        await AssertStatus(
            async () => await _client.CreateAsync(NewValidChRequest(r => r.SubTypeId = InitiativeModelBuilder.ConstitutionalId.ToString())),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestChInitiativeWithoutSubTypeShouldThrow()
    {
        await AssertStatus(
            async () => await _client.CreateAsync(NewValidChRequest(r => r.SubTypeId = string.Empty)),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task TestCtInitiativeWithChSubTypeShouldThrow()
    {
        await AssertStatus(
            async () => await _client.CreateAsync(NewValidCtRequest(r => r.SubTypeId = InitiativeModelBuilder.FederalId.ToString())),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestCtInitiativeWithoutSubTypeShouldThrow()
    {
        await AssertStatus(
            async () => await _client.CreateAsync(NewValidCtRequest(r => r.SubTypeId = string.Empty)),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task TestDisabledDoiTypeShouldThrow()
    {
        await WithOnlyCtDomainOfInfluenceTypeEnabled(async () => await AssertStatus(
            async () => await _client.CreateAsync(NewValidChRequest()),
            StatusCode.InvalidArgument,
            "Domain of influence type Ch is not enabled."));
    }

    [Fact]
    public async Task InsufficientAcr()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.CreateAsync(NewValidMuRequest()),
            StatusCode.PermissionDenied,
            nameof(InsufficientAcrException));
    }

    [Fact]
    public async Task TestMuInitiativeWithoutMunicipalityIdShouldThrow()
    {
        await AssertStatus(
            async () => await _client.CreateAsync(NewValidMuRequest(r => r.Bfs = string.Empty)),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(async () => await Client.CreateAsync(new CreateInitiativeRequest()), StatusCode.Unauthenticated);
    }

    private CreateInitiativeRequest NewValidChRequest(Action<CreateInitiativeRequest>? customizer = null)
    {
        var request = new CreateInitiativeRequest
        {
            DomainOfInfluenceType = DomainOfInfluenceType.Ch,
            Description = "Abwasser Initiative",
            SubTypeId = InitiativeModelBuilder.FederalId.ToString(),
        };
        customizer?.Invoke(request);
        return request;
    }

    private CreateInitiativeRequest NewValidCtRequest(Action<CreateInitiativeRequest>? customizer = null)
    {
        var request = new CreateInitiativeRequest
        {
            DomainOfInfluenceType = DomainOfInfluenceType.Ct,
            Description = "Abwasser Initiative",
            SubTypeId = InitiativeModelBuilder.ConstitutionalId.ToString(),
        };
        customizer?.Invoke(request);
        return request;
    }

    private CreateInitiativeRequest NewValidMuRequest(Action<CreateInitiativeRequest>? customizer = null)
    {
        var request = new CreateInitiativeRequest
        {
            DomainOfInfluenceType = DomainOfInfluenceType.Mu,
            Description = "Abwasser Initiative",
            Bfs = "3203",
        };
        customizer?.Invoke(request);
        return request;
    }
}
