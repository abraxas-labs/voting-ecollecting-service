// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;
using Voting.Lib.Common;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.CollectionTests;

public class CollectionRejectPermissionByTokenTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
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

    public CollectionRejectPermissionByTokenTest(TestApplicationFactory factory)
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
        await Client.RejectPermissionByTokenAsync(NewValidRequest());
        var permission = await RunOnDb(db => db.CollectionPermissions.SingleAsync(x => x.Id == _id));
        await Verify(permission);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await Client.RejectPermissionByTokenAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task NotFound()
    {
        var req = new RejectCollectionPermissionRequest { Token = UrlToken.New() };
        await AssertStatus(
            async () => await Client.RejectPermissionByTokenAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ExpiredShouldThrow()
    {
        await ModifyDbEntities(
            (CollectionPermissionEntity e) => e.Token == _token,
            e => e.TokenExpiry = MockedClock.GetDate(-4));

        await AssertStatus(
            async () => await Client.RejectPermissionByTokenAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task CollectionState(CollectionState state)
    {
        await ModifyDbEntities<InitiativeEntity>(
            e => e.Id == InitiativesCtStGallen.GuidLegislativeInPreparation,
            e => e.State = state);
        if (state.IsEndedOrAborted())
        {
            await AssertStatus(
                async () => await Client.RejectPermissionByTokenAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
        else
        {
            await Client.RejectPermissionByTokenAsync(NewValidRequest());
        }
    }

    [Theory]
    [EnumData<CollectionPermissionState>]
    public async Task PermissionState(CollectionPermissionState state)
    {
        await ModifyDbEntities<CollectionPermissionEntity>(
            e => e.Id == _id,
            e => e.State = state);

        if (state == CollectionPermissionState.Pending)
        {
            await Client.RejectPermissionByTokenAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await Client.RejectPermissionByTokenAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    private RejectCollectionPermissionRequest NewValidRequest()
    {
        return new RejectCollectionPermissionRequest { Token = _token.ToString() };
    }
}
