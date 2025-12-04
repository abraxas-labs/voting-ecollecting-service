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
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;
using CollectionAddress = Voting.ECollecting.Proto.Citizen.Services.V1.Models.CollectionAddress;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.ReferendumTests;

public class ReferendumUpdateTest : BaseGrpcTest<ReferendumService.ReferendumServiceClient>
{
    public ReferendumUpdateTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Referendums.WithReferendums(ReferendumsCtStGallen.GuidInPreparation));
    }

    [Fact]
    public async Task TestShouldUpdateAsCreator()
    {
        await AuthenticatedClient.UpdateAsync(NewValidRequest());
        var referendum = await RunOnDb(db =>
            db.Referendums.FirstAsync(x => x.Id == ReferendumsCtStGallen.GuidInPreparation));
        referendum.SetPeriodState(GetService<TimeProvider>().GetUtcNowDateTime());
        await Verify(referendum);
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
    public async Task TestShouldUpdateAsDeputy()
    {
        await DeputyClient.UpdateAsync(NewValidRequest());
        var referendum = await RunOnDb(db => db.Referendums.FirstAsync(x => x.Id == ReferendumsCtStGallen.GuidInPreparation));
        referendum.SetPeriodState(GetService<TimeProvider>().GetUtcNowDateTime());
        await Verify(referendum);
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
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.UpdateAsync(NewValidRequest(x =>
                x.Id = "443a1e0e-f349-47e8-96a1-ec3301407251")),
            StatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(async () => await Client.UpdateAsync(new UpdateReferendumRequest()), StatusCode.Unauthenticated);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInState(CollectionState state)
    {
        // referendum never is in state ReturnForCorrection, so we can skip it
        if (state == CollectionState.ReturnedForCorrection)
        {
            return;
        }

        await ModifyDbEntities<ReferendumEntity>(
            e => e.Id == ReferendumsCtStGallen.GuidInPreparation,
            e => e.State = state);

        if (state == CollectionState.InPreparation)
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

    private UpdateReferendumRequest NewValidRequest(Action<UpdateReferendumRequest>? customizer = null)
    {
        var request = new UpdateReferendumRequest
        {
            Id = ReferendumsCtStGallen.IdInPreparation,
            Description = "Gegen das Schmutzwassergsetz",
            Reason = "Trinkwasser muss erhalten bleiben.",
            MembersCommittee = "Heinz Müller, Präsident SP St. Gallen",
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
