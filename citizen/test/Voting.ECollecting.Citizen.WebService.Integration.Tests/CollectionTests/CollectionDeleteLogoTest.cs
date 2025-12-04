// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
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

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.CollectionTests;

public class CollectionDeleteLogoTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionDeleteLogoTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation));
    }

    [Fact]
    public async Task ShouldDeleteLogo()
    {
        var fileId = await RunOnDb(db => db.Initiatives.Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.Logo!.Id)
            .SingleAsync());

        await AuthenticatedClient.DeleteLogoAsync(NewValidRequest());

        var collection = await RunOnDb(db => db.Collections
            .Include(x => x.Logo)
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        collection.Logo.Should().BeNull();

        var hasFile = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == fileId));
        hasFile.Should().BeFalse();

        var hasFileContent = await RunOnDb(db => db.FileContents.AnyAsync(x => x.FileId == fileId));
        hasFileContent.Should().BeFalse();
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await AuthenticatedClient.DeleteLogoAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldWorkAsDeputy()
    {
        var fileId = await RunOnDb(db => db.Initiatives.Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.Logo!.Id)
            .SingleAsync());

        await DeputyClient.DeleteLogoAsync(NewValidRequest());

        var collection = await RunOnDb(db => db.Collections
            .Include(x => x.Logo)
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        collection.Logo.Should().BeNull();

        var hasFile = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == fileId));
        hasFile.Should().BeFalse();

        var hasFileContent = await RunOnDb(db => db.FileContents.AnyAsync(x => x.FileId == fileId));
        hasFileContent.Should().BeFalse();
    }

    [Fact]
    public async Task AsReaderShouldFail()
    {
        await AssertStatus(
            async () => await ReaderClient.DeleteLogoAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsDeputyNotAcceptedShouldFail()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.DeleteLogoAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.DeleteLogoAsync(NewValidRequest(x => x.CollectionId = "dcedcdb0-6aaa-4be7-be20-37d1b9f66839")),
            StatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(async () => await Client.DeleteLogoAsync(new DeleteCollectionLogoRequest()), StatusCode.Unauthenticated);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInState(CollectionState state)
    {
        await ModifyDbEntities<InitiativeEntity>(
            e => e.Id == InitiativesCtStGallen.GuidLegislativeInPreparation,
            e => e.State = state);

        if (state.InPreparationOrReturnForCorrection())
        {
            await AuthenticatedClient.DeleteLogoAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await AuthenticatedClient.DeleteLogoAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    private DeleteCollectionLogoRequest NewValidRequest(Action<DeleteCollectionLogoRequest>? customizer = null)
    {
        var request = new DeleteCollectionLogoRequest
        {
            CollectionId = InitiativesCtStGallen.IdLegislativeInPreparation,
        };
        customizer?.Invoke(request);
        return request;
    }
}
