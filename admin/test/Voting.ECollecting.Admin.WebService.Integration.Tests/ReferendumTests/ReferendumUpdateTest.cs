// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Test.MockedData;
using CollectionAddress = Voting.ECollecting.Proto.Admin.Services.V1.Models.CollectionAddress;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.ReferendumTests;

public class ReferendumUpdateTest : BaseGrpcTest<ReferendumService.ReferendumServiceClient>
{
    public ReferendumUpdateTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Referendums
            .WithReferendums(
                ReferendumsCtStGallen.GuidInPreparation,
                ReferendumsCtStGallen.GuidSignatureSheetsSubmitted,
                ReferendumsMuStGallen.GuidInCollectionActive));
    }

    [Fact]
    public async Task ShouldUpdateReferendum()
    {
        await CtSgStammdatenverwalterClient.UpdateAsync(NewValidRequest());
        var referendum = await CtSgStammdatenverwalterClient.GetAsync(new GetReferendumRequest
        {
            Id = ReferendumsCtStGallen.IdInPreparation,
        });
        await Verify(referendum);
    }

    [Fact]
    public async Task ShouldCreateCollectionMessage()
    {
        await CtSgStammdatenverwalterClient.UpdateAsync(NewValidRequest());

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == ReferendumsCtStGallen.GuidInPreparation)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db =>
            await db.CollectionMessages.FirstAsync(x => x.CollectionId == ReferendumsCtStGallen.GuidInPreparation));

        await Verify(new { userNotifications, collectionMessage });
    }

    [Fact]
    public async Task EndedShouldThrow()
    {
        var req = NewValidRequest(x => x.Id = ReferendumsCtStGallen.IdSignatureSheetsSubmitted);
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.UpdateAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task OtherMuShouldThrow()
    {
        var req = NewValidRequest(x => x.Id = ReferendumsMuStGallen.IdInCollectionActive);
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.UpdateAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task NotFoundShouldThrow()
    {
        var req = NewValidRequest(x => x.Id = "93dde01e-eb88-48f6-88ba-e4fa7b30152f");
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.UpdateAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStammdatenverwalterClient.UpdateAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new ReferendumService.ReferendumServiceClient(channel).UpdateAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles() => [Roles.Stammdatenverwalter];

    private UpdateReferendumRequest NewValidRequest(Action<UpdateReferendumRequest>? customizer = null)
    {
        var request = new UpdateReferendumRequest
        {
            Id = ReferendumsCtStGallen.IdInPreparation,
            Description = "Referendum gegen das Verbot-updated",
            Reason = "Neue Begründung für das Referendum",
            MembersCommittee = "Hans Muster, Präsident-updated",
            Link = "https://www.sg.ch/updated",
            Address = new CollectionAddress
            {
                CommitteeOrPerson = "Komitee Freie Zeitwahl-updated",
                StreetOrPostOfficeBox = "Neugasse 11-updated",
                ZipCode = "9001",
                Locality = "St.Gallen-updated",
            },
        };

        customizer?.Invoke(request);
        return request;
    }
}
