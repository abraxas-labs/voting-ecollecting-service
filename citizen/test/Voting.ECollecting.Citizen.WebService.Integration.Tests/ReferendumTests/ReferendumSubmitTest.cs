// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Text;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.ReferendumTests;

public class ReferendumSubmitTest : BaseGrpcTest<ReferendumService.ReferendumServiceClient>
{
    public ReferendumSubmitTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Referendums.WithReferendums(ReferendumsCtStGallen.GuidInPreparation));
    }

    [Fact]
    public async Task ShouldSubmitAsCreator()
    {
        var oldFileId = await RunOnDb(db => db.Referendums
            .Where(x => x.Id == ReferendumsCtStGallen.GuidInPreparation)
            .Select(x => x.SignatureSheetTemplateId)
            .SingleAsync());

        await AuthenticatedClient.SubmitAsync(NewValidRequest());

        var referendum = await RunOnDb(db => db.Referendums
            .FirstAsync(x => x.Id == ReferendumsCtStGallen.GuidInPreparation));
        referendum.State.Should().Be(CollectionState.PreparingForCollection);
        referendum.SignatureSheetTemplateId.Should().Be(oldFileId);

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == ReferendumsCtStGallen.GuidInPreparation)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == ReferendumsCtStGallen.GuidInPreparation));
        await Verify(new { userNotifications, collectionMessage });
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await AuthenticatedClient.SubmitAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldSubmitAsDeputy()
    {
        var oldFileId = await RunOnDb(db => db.Referendums
            .Where(x => x.Id == ReferendumsCtStGallen.GuidInPreparation)
            .Select(x => x.SignatureSheetTemplateId)
            .SingleAsync());

        await DeputyClient.SubmitAsync(NewValidRequest());

        var referendum = await RunOnDb(db => db.Referendums
            .FirstAsync(x => x.Id == ReferendumsCtStGallen.GuidInPreparation));
        referendum.State.Should().Be(CollectionState.PreparingForCollection);
        referendum.SignatureSheetTemplateId.Should().Be(oldFileId);

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == ReferendumsCtStGallen.GuidInPreparation)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == ReferendumsCtStGallen.GuidInPreparation));
        await Verify(new { userNotifications, collectionMessage });
    }

    [Fact]
    public async Task ShouldGenerateSignatureSheet()
    {
        await RunOnDb(db => db.Referendums
            .Where(x => x.Id == ReferendumsCtStGallen.GuidInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.SignatureSheetTemplateGenerated, true)));

        var oldFileId = await RunOnDb(db => db.Referendums
            .Where(x => x.Id == ReferendumsCtStGallen.GuidInPreparation)
            .Select(x => x.SignatureSheetTemplateId)
            .SingleAsync());

        await AuthenticatedClient.SubmitAsync(NewValidRequest());

        var referendum = await RunOnDb(db => db.Referendums
            .Include(x => x.SignatureSheetTemplate!.Content)
            .Where(x => x.Id == ReferendumsCtStGallen.GuidInPreparation)
            .SingleAsync());

        await VerifyJson(Encoding.UTF8.GetString(referendum.SignatureSheetTemplate!.Content!.Data));
        referendum.SignatureSheetTemplate.Name.Should().Be($"Unterschriftenliste_{referendum.Description}.pdf");

        var oldFileExists = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == oldFileId));
        oldFileExists.Should().BeFalse();
    }

    [Fact]
    public async Task NoDeputyPermissionShouldFail()
    {
        await RunOnDb(async db => await db.CollectionPermissions.ExecuteDeleteAsync());
        await AssertStatus(
            async () => await AuthenticatedClient.SubmitAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoDescriptionShouldFail()
    {
        await RunOnDb(db => db.Referendums
            .Where(x => x.Id == ReferendumsCtStGallen.GuidInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Description, string.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.SubmitAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoAddressCommitteeOrPersonShouldFail()
    {
        await RunOnDb(db => db.Referendums
            .Where(x => x.Id == ReferendumsCtStGallen.GuidInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Address.CommitteeOrPerson, string.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.SubmitAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoAddressStreetOrPostOfficeBoxShouldFail()
    {
        await RunOnDb(db => db.Referendums
            .Where(x => x.Id == ReferendumsCtStGallen.GuidInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Address.StreetOrPostOfficeBox, string.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.SubmitAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoAddressZipCodeShouldFail()
    {
        await RunOnDb(db => db.Referendums
            .Where(x => x.Id == ReferendumsCtStGallen.GuidInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Address.ZipCode, string.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.SubmitAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoAddressLocalityShouldFail()
    {
        await RunOnDb(db => db.Referendums
            .Where(x => x.Id == ReferendumsCtStGallen.GuidInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Address.Locality, string.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.SubmitAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoDecreeShouldFail()
    {
        await RunOnDb(db => db.Referendums
            .Where(x => x.Id == ReferendumsCtStGallen.GuidInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.DecreeId, (Guid?)null)));
        await AssertStatus(
            async () => await AuthenticatedClient.SubmitAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.SubmitAsync(new SubmitReferendumRequest { Id = "e0f0c54b-985f-4f43-aa28-8d940258b32b" }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsReaderShouldFail()
    {
        await AssertStatus(
            async () => await ReaderClient.SubmitAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsDeputyNotAcceptedShouldFail()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.SubmitAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(
            async () => await Client.SubmitAsync(NewValidRequest()),
            StatusCode.Unauthenticated);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStates(CollectionState state)
    {
        await RunOnDb(db => db.Referendums
            .Where(x => x.Id == ReferendumsCtStGallen.GuidInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        if (state is CollectionState.InPreparation)
        {
            await AuthenticatedClient.SubmitAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await AuthenticatedClient.SubmitAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    private SubmitReferendumRequest NewValidRequest(Action<SubmitReferendumRequest>? customizer = null)
    {
        var request = new SubmitReferendumRequest
        {
            Id = ReferendumsCtStGallen.IdInPreparation,
        };
        customizer?.Invoke(request);
        return request;
    }
}
