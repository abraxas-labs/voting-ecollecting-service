// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Citizen.Adapter.VotingStimmregister;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Models;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeGetTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeGetTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation, InitiativesCtStGallen.GuidUnityEnabledForCollectionCollecting));
    }

    [Fact]
    public async Task ShouldWorkAsCreator()
    {
        var initiative = await AuthenticatedClient.GetAsync(NewValidRequest());
        await Verify(initiative);
        initiative.Collection.UserPermissions.Should().NotBeNull();
        initiative.Collection.UserPermissions.CanEdit.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldWorkAsCreatorWithoutPermissions()
    {
        await RunOnDb(async db => await db.CollectionPermissions.ExecuteDeleteAsync());
        var initiative = await AuthenticatedClient.GetAsync(NewValidRequest());
        await Verify(initiative);
        initiative.Collection.UserPermissions.Should().NotBeNull();
        initiative.Collection.UserPermissions.CanEdit.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldWorkAsDeputy()
    {
        var initiative = await DeputyClient.GetAsync(NewValidRequest());
        await Verify(initiative);
        initiative.Collection.UserPermissions.Should().NotBeNull();
        initiative.Collection.UserPermissions.CanEdit.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldWorkAsReader()
    {
        var initiative = await ReaderClient.GetAsync(NewValidRequest());
        await Verify(initiative);
        initiative.Collection.UserPermissions.Should().NotBeNull();
        initiative.Collection.UserPermissions.CanEdit.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldWorkInCollectionWithIncludes()
    {
        var client = CreateCitizenClient();
        var initiative = await client.GetAsync(new GetInitiativeRequest
        {
            Id = InitiativesCtStGallen.IdUnityEnabledForCollectionCollecting,
            IncludeCommitteeDescription = true,
            IncludeIsSigned = true,
        });
        initiative.Collection.HasIsSigned.Should().BeTrue();
        initiative.Collection.IsSigned.Should().BeFalse();
        initiative.Collection.SignatureType.Should().Be(CollectionSignatureType.Unspecified);
        await Verify(initiative);
    }

    [Fact]
    public async Task ShouldWorkInCollectionSignedWithIncludes()
    {
        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            ssn: VotingStimmregisterAdapterMock.VotingRightPerson12Ssn);

        await client.SignAsync(new SignInitiativeRequest
        {
            Id = InitiativesCtStGallen.IdUnityEnabledForCollectionCollecting,
        });

        var initiative = await client.GetAsync(new GetInitiativeRequest
        {
            Id = InitiativesCtStGallen.IdUnityEnabledForCollectionCollecting,
            IncludeCommitteeDescription = true,
            IncludeIsSigned = true,
        });
        initiative.Collection.HasIsSigned.Should().BeTrue();
        initiative.Collection.IsSigned.Should().BeTrue();
        initiative.Collection.SignatureType.Should().Be(CollectionSignatureType.Electronic);
        await Verify(initiative);
    }

    [Fact]
    public async Task AsDeputyNotAcceptedShouldFail()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.GetAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestDisabledDoiTypeShouldFail()
    {
        await WithEnabledDomainOfInfluenceTypes([], async () => await AssertStatus(
            async () => await AuthenticatedClient.GetAsync(NewValidRequest()),
            StatusCode.NotFound));
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.GetAsync(NewValidRequest(x => x.Id = "7044a193-cef1-41fb-92d3-446e54419f95")),
            StatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(async () => await Client.GetAsync(NewValidRequest()), StatusCode.Unauthenticated);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        var initiative = await AuthenticatedClient.GetAsync(NewValidRequest());
        await Verify(initiative.Collection.UserPermissions).UseParameters(state);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStatesWithoutReadPermissions(CollectionState state)
    {
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation,
            x =>
            {
                x.State = state;
                x.AuditInfo.CreatedById = "some-user";
                x.Permissions = [];
            });

        if (state > CollectionState.Registered)
        {
            var initiative = await AuthenticatedClient.GetAsync(NewValidRequest());
            await Verify(initiative).UseParameters(state);
        }
        else
        {
            await AssertStatus(
                async () => await AuthenticatedClient.GetAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    private GetInitiativeRequest NewValidRequest(Action<GetInitiativeRequest>? customizer = null)
    {
        var request = new GetInitiativeRequest { Id = InitiativesCtStGallen.IdLegislativeInPreparation };
        customizer?.Invoke(request);
        return request;
    }
}
