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
using Voting.ECollecting.Shared.Domain.ModelBuilders;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;
using CollectionAddress = Voting.ECollecting.Proto.Citizen.Services.V1.Models.CollectionAddress;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeUpdateTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeUpdateTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(
            InitiativesCtStGallen.GuidLegislativeInPreparation,
            InitiativesCtStGallen.GuidLegislativeReturnedForCorrection,
            InitiativesCh.GuidInPreparation));
    }

    [Fact]
    public async Task TestShouldUpdateAsCreator()
    {
        await AuthenticatedClient.UpdateAsync(NewValidRequest());
        var initiative = await RunOnDb(db => db.Initiatives.FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.SetPeriodState(GetService<TimeProvider>().GetUtcTodayDateOnly());
        await Verify(initiative);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await AuthenticatedClient.UpdateAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task TestShouldThrowAsCreatorWithLockedFields()
    {
        var req = NewValidRequest(x => x.Id = InitiativesCtStGallen.IdLegislativeReturnedForCorrection);
        await AssertStatus(
            async () => await AuthenticatedClient.UpdateAsync(req),
            StatusCode.InvalidArgument,
            "Cannot edit locked field Description");
    }

    [Fact]
    public async Task TestShouldUpdateAsCreatorWithLockedFields()
    {
        var req = NewValidRequest(x =>
        {
            x.Id = InitiativesCtStGallen.IdLegislativeReturnedForCorrection;
            x.SubTypeId = InitiativeModelBuilder.LegislativeId.ToString();
            x.Description = "Für kantonale Begegnungsräume (Begegnungs-Initiative)";
            x.Wording = "foo bar baz updated";
        });
        await AuthenticatedClient.UpdateAsync(req);
        var initiative = await RunOnDb(db => db.Initiatives.FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection));
        initiative.SetPeriodState(GetService<TimeProvider>().GetUtcTodayDateOnly());
        initiative.Wording.Should().Be("foo bar baz updated");
        await Verify(initiative);
    }

    [Fact]
    public async Task TestShouldUpdateAsDeputy()
    {
        await DeputyClient.UpdateAsync(NewValidRequest());
        var initiative = await RunOnDb(db => db.Initiatives.FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.SetPeriodState(GetService<TimeProvider>().GetUtcTodayDateOnly());
        await Verify(initiative);
    }

    [Fact]
    public async Task AsReaderShouldFail()
    {
        await AssertStatus(
            async () => await ReaderClient.UpdateAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsDeputyNotAcceptedShouldFail()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.UpdateAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestChInitiativeWithCtSubTypeShouldThrow()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.UpdateAsync(NewValidRequest(r =>
            {
                r.Id = InitiativesCh.IdInPreparation;
                r.SubTypeId = InitiativeModelBuilder.ConstitutionalId.ToString();
            })),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestChInitiativeWithoutSubTypeShouldThrow()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.UpdateAsync(NewValidRequest(r =>
            {
                r.Id = InitiativesCh.IdInPreparation;
                r.SubTypeId = string.Empty;
            })),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task TestCtInitiativeWithChSubTypeShouldThrow()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.UpdateAsync(NewValidRequest(r => r.SubTypeId = InitiativeModelBuilder.FederalId.ToString())),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestCtInitiativeWithAdmissibilityDecisionAndUpdatedSubTypeShouldThrow()
    {
        var req = NewValidRequest(req =>
        {
            req.Id = InitiativesCtStGallen.IdLegislativeReturnedForCorrection;
            req.SubTypeId = InitiativeModelBuilder.ConstitutionalId.ToString();
            req.Description = "Für kantonale Begegnungsräume (Begegnungs-Initiative)";
        });
        await AssertStatus(
            async () => await AuthenticatedClient.UpdateAsync(req),
            StatusCode.InvalidArgument,
            "Cannot edit locked field SubTypeId");
    }

    [Fact]
    public async Task TestCtInitiativeWithoutAdmissibilityDecisionAndUpdatedSubTypeShouldThrow()
    {
        var req = NewValidRequest(req => req.SubTypeId = InitiativeModelBuilder.ConstitutionalId.ToString());
        await AuthenticatedClient.UpdateAsync(req);
        var initiative = await RunOnDb(db => db.Initiatives.FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.SubTypeId.Should().Be(InitiativeModelBuilder.ConstitutionalId);
    }

    [Fact]
    public async Task TestCtWithoutSubTypeShouldThrow()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.UpdateAsync(NewValidRequest(r => r.SubTypeId = string.Empty)),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.UpdateAsync(NewValidRequest(x => x.Id = "13c0489d-d893-4bf3-9a52-4f8112f2c2e4")),
            StatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(async () => await Client.UpdateAsync(new UpdateInitiativeRequest()), StatusCode.Unauthenticated);
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
            await AuthenticatedClient.UpdateAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await AuthenticatedClient.UpdateAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    private UpdateInitiativeRequest NewValidRequest(Action<UpdateInitiativeRequest>? customizer = null)
    {
        var request = new UpdateInitiativeRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeInPreparation,
            Description = "Abwasser Initiative",
            SubTypeId = InitiativeModelBuilder.ConstitutionalId.ToString(),
            Wording = "Gewässer schützen vor dem schmutzigen Abwasser.",
            Reason = "Trinkwasser muss erhalten bleiben.",
            Address = new CollectionAddress
            {
                CommitteeOrPerson = "Abwasser Komitee",
                StreetOrPostOfficeBox = "Otmarstrasse",
                ZipCode = "9000",
                Locality = "St.Gallen",
            },
            Link = "https://www.ratsinfo.sg.ch/geschaefte/4754-updated",
        };
        customizer?.Invoke(request);
        return request;
    }
}
