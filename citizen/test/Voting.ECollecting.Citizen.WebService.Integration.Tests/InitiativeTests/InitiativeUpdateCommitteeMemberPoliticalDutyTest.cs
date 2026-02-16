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

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeUpdateCommitteeMemberPoliticalDutyTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    private static readonly Guid _id = InitiativeCommitteeMembers.BuildGuid(
        InitiativesCtStGallen.GuidLegislativeInPreparation,
        "margarita@example.com");

    public InitiativeUpdateCommitteeMemberPoliticalDutyTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation, InitiativesCtStGallen.GuidLegislativeReturnedForCorrection));
    }

    [Fact]
    public async Task ShouldWork()
    {
        await AuthenticatedClient.UpdateCommitteeMemberPoliticalDutyAsync(NewValidRequest());

        var member = await RunOnDb(db => db.InitiativeCommitteeMembers
            .Include(x => x.Permission)
            .SingleAsync(x => x.Id == _id));

        await Verify(member);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await AuthenticatedClient.UpdateCommitteeMemberPoliticalDutyAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldWorkAsDeputy()
    {
        await DeputyClient.UpdateCommitteeMemberPoliticalDutyAsync(NewValidRequest());

        var member = await RunOnDb(db => db.InitiativeCommitteeMembers
            .Include(x => x.Permission)
            .SingleAsync(x => x.Id == _id));

        await Verify(member);
    }

    [Fact]
    public async Task ShouldThrowReader()
    {
        await AssertStatus(
            async () => await ReaderClient.UpdateCommitteeMemberPoliticalDutyAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowDeputyNotAccepted()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.UpdateCommitteeMemberPoliticalDutyAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowUnauthenticated()
    {
        await AssertStatus(
            async () => await Client.UpdateCommitteeMemberPoliticalDutyAsync(NewValidRequest()),
            StatusCode.Unauthenticated);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        if (state.IsNotEndedAndNotAborted())
        {
            await AuthenticatedClient.UpdateCommitteeMemberPoliticalDutyAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await DeputyNotAcceptedClient.UpdateCommitteeMemberPoliticalDutyAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    [Fact]
    public async Task ShouldThrowUnknownInitiativeId()
    {
        var req = NewValidRequest();
        req.InitiativeId = "48a5b8f1-663b-4108-acba-7b601e440964";
        await AssertStatus(
            async () => await AuthenticatedClient.UpdateCommitteeMemberPoliticalDutyAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowUnknownId()
    {
        var req = NewValidRequest();
        req.Id = "c9242e65-91bc-4deb-8115-9ce56f107a19";
        await AssertStatus(
            async () => await AuthenticatedClient.UpdateCommitteeMemberPoliticalDutyAsync(req),
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
            async () => await AuthenticatedClient.UpdateCommitteeMemberPoliticalDutyAsync(req),
            StatusCode.InvalidArgument,
            "Cannot edit locked field CommitteeMembers");
    }

    private UpdateCommitteeMemberPoliticalDutyRequest NewValidRequest()
    {
        return new UpdateCommitteeMemberPoliticalDutyRequest
        {
            Id = _id.ToString(),
            InitiativeId = InitiativesCtStGallen.IdLegislativeInPreparation,
            PoliticalDuty = "Protokollführer (updated)",
        };
    }
}
