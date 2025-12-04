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

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeDeleteCommitteeListTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    private readonly Guid _fileId = Files.BuildGuid(
        InitiativesCtStGallen.GuidLegislativeInPreparation,
        "committee-list-1.pdf");

    public InitiativeDeleteCommitteeListTest(TestApplicationFactory factory)
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
        var exists = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == _fileId));
        exists.Should().BeTrue();

        await AuthenticatedClient.DeleteCommitteeListAsync(NewValidRequest());

        exists = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == _fileId));
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await AuthenticatedClient.DeleteCommitteeListAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldWorkAsDeputy()
    {
        var exists = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == _fileId));
        exists.Should().BeTrue();

        await DeputyClient.DeleteCommitteeListAsync(NewValidRequest());

        exists = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == _fileId));
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldThrowAsReader()
    {
        var exists = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == _fileId));
        exists.Should().BeTrue();

        await AssertStatus(
            async () => await ReaderClient.DeleteCommitteeListAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsDeputyNotAccepted()
    {
        var exists = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == _fileId));
        exists.Should().BeTrue();

        await AssertStatus(
            async () => await DeputyNotAcceptedClient.DeleteCommitteeListAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowWithLockedFields()
    {
        var req = NewValidRequest();
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == Guid.Parse(req.InitiativeId),
            x => x.LockedFields = new InitiativeLockedFields
            {
                CommitteeMembers = true,
            });
        await AssertStatus(
            async () => await AuthenticatedClient.DeleteCommitteeListAsync(req),
            StatusCode.InvalidArgument,
            "Cannot edit locked field CommitteeMembers");
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        if (state.InPreparationOrReturnForCorrection())
        {
            await AuthenticatedClient.DeleteCommitteeListAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await DeputyNotAcceptedClient.DeleteCommitteeListAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    private DeleteCommitteeListRequest NewValidRequest()
    {
        return new DeleteCommitteeListRequest
        {
            InitiativeId = InitiativesCtStGallen.IdLegislativeInPreparation,
            Id = _fileId.ToString(),
        };
    }
}
