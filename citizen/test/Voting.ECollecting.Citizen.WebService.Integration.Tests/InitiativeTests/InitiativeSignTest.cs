// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Citizen.Abstractions.Adapter.VotingStimmregister;
using Voting.ECollecting.Citizen.Adapter.VotingStimmregister;
using Voting.ECollecting.Citizen.Core.Exceptions;
using Voting.ECollecting.Citizen.WebService.Exceptions;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Shared.Core.Exceptions;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.Lib.Cryptography;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeSignTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeSignTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidUnityEnabledForCollectionCollecting));
    }

    [Fact]
    public async Task ShouldWork()
    {
        var prevCount = await RunOnDb(db => db.CollectionCounts
            .SingleAsync(x => x.CollectionId == InitiativesCtStGallen.GuidUnityEnabledForCollectionCollecting));

        var prevCollectionMunicipality = await RunOnDb(db => db.CollectionMunicipalities
            .SingleAsync(x => x.Id == CollectionMunicipalities.BuildGuid(InitiativesCtStGallen.GuidUnityEnabledForCollectionCollecting, Bfs.MunicipalityStGallen)));

        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            ssn: VotingStimmregisterAdapterMock.VotingRightPerson12Ssn);
        await client.SignAsync(NewValidRequest());

        var citizenEntry = await RunOnDb(db => db.CollectionCitizens
            .Include(x => x.Log)
            .Include(x => x.CollectionMunicipality!.Collection)
            .SingleAsync(x => x.CollectionMunicipality!.CollectionId == InitiativesCtStGallen.GuidUnityEnabledForCollectionCollecting));

        if (citizenEntry.CollectionMunicipality!.Collection is InitiativeEntity initiative)
        {
            initiative.SetPeriodState(GetService<TimeProvider>().GetUtcTodayDateOnly());
        }

        var crypto = GetService<ICryptoProvider>();

        // ensure stimmregisterid mac works
        var stimmregisterIdSerialized = VotingStimmregisterAdapterMock.VotingRightPerson12.RegisterId.ToByteArray(true);
        var stimmregisterIdMac = await crypto.CreateHmacSha256(
            stimmregisterIdSerialized,
            citizenEntry.CollectionMunicipality.Collection!.MacKeyId ?? throw new InvalidOperationException("Key not set"));
        citizenEntry.Log!.VotingStimmregisterIdMac.Should().BeEquivalentTo(stimmregisterIdMac);

        // ensure encrypted stimmregister id is not set
        citizenEntry.Log!.VotingStimmregisterIdEncrypted.Should().BeEmpty();

        await Verify(citizenEntry);

        citizenEntry.CollectionMunicipality.ElectronicCitizenCount.Should().Be(prevCollectionMunicipality.ElectronicCitizenCount + 1);
        citizenEntry.CollectionMunicipality.TotalValidCitizenCount.Should().Be(prevCollectionMunicipality.TotalValidCitizenCount + 1);

        var updatedCount = await RunOnDb(db => db.CollectionCounts
            .SingleAsync(x => x.CollectionId == InitiativesCtStGallen.GuidUnityEnabledForCollectionCollecting));
        updatedCount.ElectronicCitizenCount.Should().Be(prevCount.ElectronicCitizenCount + 1);
        updatedCount.TotalCitizenCount.Should().Be(prevCount.TotalCitizenCount + 1);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            var client = CreateCitizenClient(
                acrValue: CitizenAuthMockDefaults.AcrValue400,
                ssn: VotingStimmregisterAdapterMock.VotingRightPerson12Ssn);
            await client.SignAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task SignTwiceShouldThrow()
    {
        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            ssn: VotingStimmregisterAdapterMock.VotingRightPerson12Ssn);
        await client.SignAsync(NewValidRequest());
        await AssertStatus(
            async () => await client.SignAsync(NewValidRequest()),
            StatusCode.FailedPrecondition,
            nameof(CollectionAlreadySignedException));
    }

    [Fact]
    public async Task SignParallelShouldOnlyAcceptOne()
    {
        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            ssn: VotingStimmregisterAdapterMock.VotingRightPerson12Ssn);

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => client.SignAsync(NewValidRequest()))
            .ToList();

        foreach (var task in tasks)
        {
            try
            {
                await task;
            }
            catch (RpcException)
            {
            }
        }

        tasks.Count(x => x.GetStatus().StatusCode == StatusCode.OK)
            .Should()
            .Be(1);
        tasks.Count(x =>
                x.GetStatus().StatusCode == StatusCode.FailedPrecondition
                && x.GetStatus().Detail.Contains(nameof(CollectionAlreadySignedException)))
            .Should()
            .Be(tasks.Count - 1);
    }

    [Fact]
    public async Task TooManyElectronicSignaturesShouldThrow()
    {
        await ModifyDbEntities(
            (CollectionCountEntity entity) => entity.CollectionId == InitiativesCtStGallen.GuidUnityEnabledForCollectionCollecting,
            e => e.ElectronicCitizenCount = 1_000);

        await ModifyDbEntities(
            (InitiativeEntity entity) => entity.Id == InitiativesCtStGallen.GuidUnityEnabledForCollectionCollecting,
            e => e.MaxElectronicSignatureCount = 1_000);

        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            ssn: VotingStimmregisterAdapterMock.VotingRightPerson12Ssn);
        await AssertStatus(
            async () => await client.SignAsync(NewValidRequest()),
            StatusCode.FailedPrecondition,
            nameof(CollectionMaxElectronicSignatureCountReachedException));
    }

    [Fact]
    public async Task ParallelElectronicSignaturesOnlyOneShouldSucceed()
    {
        await ModifyDbEntities(
            (CollectionCountEntity entity) => entity.CollectionId == InitiativesCtStGallen.GuidUnityEnabledForCollectionCollecting,
            e => e.ElectronicCitizenCount = 2);

        await ModifyDbEntities(
            (InitiativeEntity entity) => entity.Id == InitiativesCtStGallen.GuidUnityEnabledForCollectionCollecting,
            e => e.MaxElectronicSignatureCount = 3);

        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            ssn: VotingStimmregisterAdapterMock.VotingRightPerson12Ssn);
        var client2 = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            ssn: VotingStimmregisterAdapterMock.VotingRightPerson11Ssn);

        var call1 = client.SignAsync(NewValidRequest());
        var call2 = client2.SignAsync(NewValidRequest());

        // one should succeed, one should fail
        // but we don't know which fails and which succeeds
        try
        {
            await call1;
        }
        catch (RpcException)
        {
        }

        try
        {
            await call2;
        }
        catch (RpcException)
        {
        }

        if (call1.GetStatus().StatusCode == StatusCode.OK)
        {
            call2.GetStatus().StatusCode.Should().Be(StatusCode.FailedPrecondition);
            call2.GetStatus().Detail.Should().Contain(nameof(CollectionMaxElectronicSignatureCountReachedException));
        }
        else
        {
            call1.GetStatus().StatusCode.Should().Be(StatusCode.FailedPrecondition);
            call1.GetStatus().Detail.Should().Contain(nameof(CollectionMaxElectronicSignatureCountReachedException));
        }
    }

    [Fact]
    public async Task InsufficientAcrShouldThrow()
    {
        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue100,
            ssn: VotingStimmregisterAdapterMock.VotingRightPerson12Ssn);
        await AssertStatus(
            async () => await client.SignAsync(NewValidRequest()),
            StatusCode.PermissionDenied,
            nameof(InsufficientAcrException));
    }

    [Fact]
    public async Task NoVotingRightShouldThrow()
    {
        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            ssn: VotingStimmregisterAdapterMock.NoVotingRightPerson1Ssn);
        await AssertStatus(
            async () => await client.SignAsync(NewValidRequest()),
            StatusCode.InvalidArgument,
            nameof(PersonOrVotingRightNotFoundException));
    }

    [Fact]
    public async Task NoSsnShouldThrow()
    {
        var client = CreateCitizenClient(acrValue: CitizenAuthMockDefaults.AcrValue400);
        await AssertStatus(
            async () => await client.SignAsync(NewValidRequest()),
            StatusCode.InvalidArgument,
            "Social security number not set");
    }

    [Fact]
    public async Task NotEnabledForCollectionShouldThrow()
    {
        await ModifyDbEntities(
            (InitiativeEntity entity) => entity.Id == InitiativesCtStGallen.GuidUnityEnabledForCollectionCollecting,
            e => e.State = CollectionState.PreparingForCollection);

        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            ssn: VotingStimmregisterAdapterMock.VotingRightPerson12Ssn);
        await AssertStatus(
            async () => await client.SignAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task NotInCollectionShouldThrow()
    {
        await ModifyDbEntities(
            (InitiativeEntity entity) => entity.Id == InitiativesCtStGallen.GuidUnityEnabledForCollectionCollecting,
            e => e.CollectionStartDate = MockedClock.NowDateOnly.AddDays(1));

        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            ssn: VotingStimmregisterAdapterMock.VotingRightPerson12Ssn);
        await AssertStatus(
            async () => await client.SignAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowUnauthenticated()
    {
        await AssertStatus(async () => await Client.SignAsync(NewValidRequest()), StatusCode.Unauthenticated);
    }

    private SignInitiativeRequest NewValidRequest()
    {
        return new SignInitiativeRequest { Id = InitiativesCtStGallen.IdUnityEnabledForCollectionCollecting };
    }
}
