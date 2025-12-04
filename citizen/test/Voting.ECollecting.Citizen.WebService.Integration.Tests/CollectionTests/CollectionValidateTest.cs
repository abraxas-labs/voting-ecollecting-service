// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.CollectionTests;

public class CollectionValidateTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionValidateTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Default.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation).WithReferendums(ReferendumsCtStGallen.GuidInPreparation));
    }

    [Fact]
    public async Task ShouldValidateAsCreator()
    {
        var response = await AuthenticatedClient.ValidateAsync(NewValidRequest());
        await Verify(response);
    }

    [Fact]
    public async Task ShouldValidateAsDeputy()
    {
        var response = await DeputyClient.ValidateAsync(NewValidRequest());
        await Verify(response);
    }

    [Fact]
    public async Task ShouldValidateWithReferendum()
    {
        var response = await AuthenticatedClient.ValidateAsync(NewValidRequest(x => x.Id = ReferendumsCtStGallen.IdInPreparation));
        await Verify(response);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.ValidateAsync(new ValidateCollectionRequest { Id = "01dd6da1-9ed9-466e-87f5-cafce3816e3f" }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsReaderShouldFail()
    {
        await AssertStatus(
            async () => await ReaderClient.ValidateAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsDeputyNotAcceptedShouldFail()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.ValidateAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(
            async () => await Client.ValidateAsync(NewValidRequest()),
            StatusCode.Unauthenticated);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        if (state is CollectionState.InPreparation or CollectionState.ReturnedForCorrection)
        {
            await AuthenticatedClient.ValidateAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await AuthenticatedClient.ValidateAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    private ValidateCollectionRequest NewValidRequest(Action<ValidateCollectionRequest>? customizer = null)
    {
        var request = new ValidateCollectionRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeInPreparation,
        };
        customizer?.Invoke(request);
        return request;
    }
}
