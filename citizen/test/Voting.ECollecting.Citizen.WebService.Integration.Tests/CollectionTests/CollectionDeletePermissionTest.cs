// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Citizen.Core.Exceptions;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.CollectionTests;

public class CollectionDeletePermissionTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    private static readonly Guid _permissionId = CollectionPermissions.BuildGuid(
        InitiativesCtStGallen.GuidLegislativeInPreparation,
        true,
        CollectionPermissionRole.Deputy);

    private static readonly Guid _readerPermissionId = CollectionPermissions.BuildGuid(
        InitiativesCtStGallen.GuidLegislativeInPreparation,
        true,
        CollectionPermissionRole.Reader);

    public CollectionDeletePermissionTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation));
    }

    [Fact]
    public async Task ShouldWorkAsCreator()
    {
        await AuthenticatedClient.DeletePermissionAsync(NewValidRequest());
        var permission = await RunOnDb(db => db.CollectionPermissions.FirstOrDefaultAsync(x => x.Id == _permissionId));
        permission.Should().BeNull();
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await AuthenticatedClient.DeletePermissionAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldWorkAsDeputy()
    {
        await DeputyClient.DeletePermissionAsync(NewValidRequest(x => x.Id = _readerPermissionId.ToString()));
        var permission = await RunOnDb(db => db.CollectionPermissions.FirstOrDefaultAsync(x => x.Id == _readerPermissionId));
        permission.Should().BeNull();
    }

    [Fact]
    public async Task ShouldThrowAsDeputySelf()
    {
        await ModifyDbEntities<CollectionPermissionEntity>(
            e => e.Id == _permissionId,
            e => e.Email = CitizenAuthMockDefaults.UserTestEMail);

        await AssertStatus(
            async () => await DeputyClient.DeletePermissionAsync(NewValidRequest()),
            StatusCode.InvalidArgument,
            nameof(CannotDeleteOwnPermissionException));
    }

    [Fact]
    public async Task AsReaderShouldFail()
    {
        await AssertStatus(
            async () => await ReaderClient.DeletePermissionAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsDeputyNotAcceptedShouldFail()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.DeletePermissionAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.DeletePermissionAsync(NewValidRequest(x => x.Id = "d986d89a-3410-4dfd-9c25-d27f210b2f25")),
            StatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(async () => await Client.DeletePermissionAsync(new DeleteCollectionPermissionRequest()), StatusCode.Unauthenticated);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInState(CollectionState state)
    {
        await ModifyDbEntities<InitiativeEntity>(
            e => e.Id == InitiativesCtStGallen.GuidLegislativeInPreparation,
            e => e.State = state);

        if (state.IsEndedOrAborted())
        {
            await AssertStatus(
                async () => await AuthenticatedClient.DeletePermissionAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
        else
        {
            await AuthenticatedClient.DeletePermissionAsync(NewValidRequest());
        }
    }

    private DeleteCollectionPermissionRequest NewValidRequest(Action<DeleteCollectionPermissionRequest>? customizer = null)
    {
        var request = new DeleteCollectionPermissionRequest
        {
            Id = _permissionId.ToString(),
        };
        customizer?.Invoke(request);
        return request;
    }
}
