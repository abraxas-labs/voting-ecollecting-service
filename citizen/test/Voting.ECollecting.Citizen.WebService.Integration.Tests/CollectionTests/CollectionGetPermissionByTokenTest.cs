// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;
using Voting.Lib.Common;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.CollectionTests;

public class CollectionGetPermissionByTokenTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    private static readonly Guid _id =
        CollectionPermissions.BuildGuid(
            InitiativesCtStGallen.GuidLegislativeInPreparation,
            false,
            CollectionPermissionRole.Deputy);

    private static readonly UrlToken _token =
        CollectionPermissions.BuildToken(
            InitiativesCtStGallen.GuidLegislativeInPreparation,
            CollectionPermissionRole.Deputy);

    public CollectionGetPermissionByTokenTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation));
    }

    [Fact]
    public async Task ShouldWork()
    {
        ResetUserNotificationSender();

        var response = await Client.GetPendingPermissionByTokenAsync(NewValidRequest());
        await Verify(response);
    }

    [Fact]
    public async Task NotFound()
    {
        await AssertStatus(
            async () => await Client.GetPendingPermissionByTokenAsync(
                new GetPendingCollectionPermissionByTokenRequest { Token = UrlToken.New() }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ExpiredShouldThrow()
    {
        await ModifyDbEntities(
            (CollectionPermissionEntity e) => e.Token == _token,
            e => e.TokenExpiry = MockedClock.GetDate(-4));

        await AssertStatus(
            async () => await Client.GetPendingPermissionByTokenAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Theory]
    [EnumData<CollectionPermissionState>]
    public async Task States(CollectionPermissionState state)
    {
        await ModifyDbEntities<CollectionPermissionEntity>(
            e => e.Id == _id,
            e => e.State = state);

        if (state == CollectionPermissionState.Pending)
        {
            await Client.GetPendingPermissionByTokenAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await Client.GetPendingPermissionByTokenAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    private GetPendingCollectionPermissionByTokenRequest NewValidRequest()
    {
        return new GetPendingCollectionPermissionByTokenRequest
        {
            Token = _token.ToString(),
        };
    }
}
