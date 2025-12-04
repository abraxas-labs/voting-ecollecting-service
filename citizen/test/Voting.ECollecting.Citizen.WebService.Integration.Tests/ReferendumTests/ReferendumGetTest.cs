// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Citizen.Adapter.VotingStimmregister;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.ReferendumTests;

public class ReferendumGetTest : BaseGrpcTest<ReferendumService.ReferendumServiceClient>
{
    public ReferendumGetTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Referendums.WithDecrees(DecreesCtStGallen.GuidInPreparationWithReferendum, DecreesCtStGallen.GuidInCollectionWithReferendum));
    }

    [Fact]
    public async Task ShouldWorkAsCreator()
    {
        var referendum = await AuthenticatedClient.GetAsync(NewValidRequest());
        await Verify(referendum);
    }

    [Fact]
    public async Task ShouldWorkAsCreatorWithoutDecree()
    {
        await ModifyDbEntities<ReferendumEntity>(
            x => x.Id == ReferendumsCtStGallen.GuidInPreparation,
            x =>
            {
                x.DomainOfInfluenceType = null;
                x.DecreeId = null;
            });
        var referendum = await AuthenticatedClient.GetAsync(NewValidRequest());
        await Verify(referendum);
    }

    [Fact]
    public async Task ShouldWorkAsCreatorWithoutPermissions()
    {
        await RunOnDb(async db => await db.CollectionPermissions.ExecuteDeleteAsync());
        var referendum = await AuthenticatedClient.GetAsync(NewValidRequest());
        await Verify(referendum);
        referendum.Collection.UserPermissions.Should().NotBeNull();
        referendum.Collection.UserPermissions.CanEdit.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldWorkAsDeputy()
    {
        var referendum = await DeputyClient.GetAsync(NewValidRequest());
        await Verify(referendum);
        referendum.Collection.UserPermissions.Should().NotBeNull();
        referendum.Collection.UserPermissions.CanEdit.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldWorkAsReader()
    {
        var referendum = await ReaderClient.GetAsync(NewValidRequest());
        await Verify(referendum);
        referendum.Collection.UserPermissions.Should().NotBeNull();
        referendum.Collection.UserPermissions.CanEdit.Should().BeFalse();
    }

    [Fact]
    public async Task AsDeputyNotAcceptedShouldFail()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.GetAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldWorkInCollection()
    {
        var client = CreateCitizenClient();
        var referendum = await client.GetAsync(new GetReferendumRequest
        {
            Id = ReferendumsCtStGallen.IdInCollectionEnabledForCollection,
            IncludeIsSigned = true,
        });
        referendum.Collection.HasIsSigned.Should().BeTrue();
        referendum.Collection.IsSigned.Should().BeFalse();
        await Verify(referendum);
    }

    [Fact]
    public async Task ShouldWorkInCollectionSigned()
    {
        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            ssn: VotingStimmregisterAdapterMock.VotingRightPerson12Ssn);

        await client.SignAsync(new SignReferendumRequest
        {
            Id = ReferendumsCtStGallen.IdInCollectionEnabledForCollection,
        });

        var referendum = await client.GetAsync(new GetReferendumRequest
        {
            Id = ReferendumsCtStGallen.IdInCollectionEnabledForCollection,
            IncludeIsSigned = true,
        });
        referendum.Collection.HasIsSigned.Should().BeTrue();
        referendum.Collection.IsSigned.Should().BeTrue();
        referendum.HasIsOtherReferendumOfSameDecreeSigned.Should().BeTrue();
        referendum.IsOtherReferendumOfSameDecreeSigned.Should().BeFalse();
        await Verify(referendum);
    }

    [Fact]
    public async Task ShouldWorkInCollectionSignedOtherReferendumSameDecree()
    {
        var client = CreateCitizenClient(
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            ssn: VotingStimmregisterAdapterMock.VotingRightPerson12Ssn);

        await client.SignAsync(new SignReferendumRequest
        {
            Id = ReferendumsCtStGallen.IdInCollectionEnabledForCollection2,
        });

        var referendum = await client.GetAsync(new GetReferendumRequest
        {
            Id = ReferendumsCtStGallen.IdInCollectionEnabledForCollection,
            IncludeIsSigned = true,
        });
        referendum.Collection.HasIsSigned.Should().BeTrue();
        referendum.Collection.IsSigned.Should().BeFalse();
        referendum.HasIsOtherReferendumOfSameDecreeSigned.Should().BeTrue();
        referendum.IsOtherReferendumOfSameDecreeSigned.Should().BeTrue();
        await Verify(referendum);
    }

    [Fact]
    public async Task TestDisabledDoiTypeShouldThrow()
    {
        await WithEnabledDomainOfInfluenceTypes([], async () => await AssertStatus(
            async () => await AuthenticatedClient.GetAsync(NewValidRequest()),
            StatusCode.NotFound));
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.GetAsync(NewValidRequest(x => x.Id = "29e0448c-3209-48a1-817e-8108e06663e3")),
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
        await RunOnDb(db => db.Referendums
            .Where(x => x.Id == ReferendumsCtStGallen.GuidInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        var referendum = await AuthenticatedClient.GetAsync(NewValidRequest());
        await Verify(referendum.Collection.UserPermissions).UseParameters(state);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStatesWithoutReadPermissions(CollectionState state)
    {
        await ModifyDbEntities<ReferendumEntity>(
            x => x.Id == ReferendumsCtStGallen.GuidInPreparation,
            x =>
            {
                x.State = state;
                x.AuditInfo.CreatedById = "some-user";
                x.Permissions = [];
            });

        if (state > CollectionState.Registered)
        {
            var referendum = await AuthenticatedClient.GetAsync(NewValidRequest());
            await Verify(referendum).UseParameters(state);
        }
        else
        {
            await AssertStatus(
                async () => await AuthenticatedClient.GetAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    private GetReferendumRequest NewValidRequest(Action<GetReferendumRequest>? customizer = null)
    {
        var request = new GetReferendumRequest { Id = ReferendumsCtStGallen.IdInPreparation };
        customizer?.Invoke(request);
        return request;
    }
}
