// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Text;
using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Citizen.Core.Configuration;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeSubmitTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeSubmitTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation, InitiativesCh.GuidInPreparation));
    }

    [Fact]
    public async Task ShouldSubmitAsCreator()
    {
        var oldFileId = await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.SignatureSheetTemplateId)
            .SingleAsync());

        await AuthenticatedClient.SubmitAsync(NewValidRequest());

        var initiative = await RunOnDb(db => db.Initiatives
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.State.Should().Be(CollectionState.Submitted);
        initiative.SignatureSheetTemplateId.Should().Be(oldFileId);

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesCtStGallen.GuidLegislativeInPreparation));
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
        var oldFileId = await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.SignatureSheetTemplateId)
            .SingleAsync());

        await DeputyClient.SubmitAsync(NewValidRequest());

        var initiative = await RunOnDb(db => db.Initiatives
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.State.Should().Be(CollectionState.Submitted);
        initiative.SignatureSheetTemplateId.Should().Be(oldFileId);

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesCtStGallen.GuidLegislativeInPreparation));
        await Verify(new { userNotifications, collectionMessage });
    }

    [Fact]
    public async Task ShouldGenerateSignatureSheet()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.SignatureSheetTemplateGenerated, true)));

        var oldFileId = await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.SignatureSheetTemplateId)
            .SingleAsync());

        await AuthenticatedClient.SubmitAsync(NewValidRequest());

        var file = await RunOnDb(db => db.Initiatives
            .Include(x => x.SignatureSheetTemplate!.Content)
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.SignatureSheetTemplate)
            .SingleAsync());

        await VerifyJson(Encoding.UTF8.GetString(file!.Content!.Data));
        file.Name.Should().Be("Initiative_Unterschriftenliste.pdf");

        var oldFileExists = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == oldFileId));
        oldFileExists.Should().BeFalse();
    }

    [Fact]
    public async Task LessApprovedCommitteeMembersShouldFail()
    {
        var config = GetService<CoreAppConfig>();
        config.InitiativeCommitteeMinApprovedMembersCount = 18;

        await AssertStatus(
            async () => await AuthenticatedClient.SubmitAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task MoreApprovedCommitteeMembersOnFederalLevelShouldFail()
    {
        var config = GetService<CoreAppConfig>();
        config.InitiativeCommitteeMaxApprovedMembersCount = 3;

        await AssertStatus(
            async () => await AuthenticatedClient.SubmitAsync(NewValidRequest(x => x.Id = InitiativesCh.IdInPreparation)),
            StatusCode.InvalidArgument);
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
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Description, string.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.SubmitAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoWordingShouldFail()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Wording, string.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.SubmitAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoAddressCommitteeOrPersonShouldFail()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Address.CommitteeOrPerson, string.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.SubmitAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoAddressStreetOrPostOfficeBoxShouldFail()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Address.StreetOrPostOfficeBox, string.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.SubmitAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoAddressZipCodeShouldFail()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Address.ZipCode, string.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.SubmitAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoAddressLocalityShouldFail()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Address.Locality, string.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.SubmitAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task WithAdmissibilityDecisionStateShouldFail()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.AdmissibilityDecisionState, AdmissibilityDecisionState.Open)));
        await AssertStatus(
            async () => await AuthenticatedClient.SubmitAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.SubmitAsync(new SubmitInitiativeRequest { Id = "996776e2-9bc5-4a3c-8e57-b46ae41cc7a6" }),
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

    [Fact]
    public async Task NoUploadedCommitteeListShouldFail()
    {
        await RunOnDb(db => db.Files
            .Where(x => x.CommitteeListOfInitiativeId == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteDeleteAsync());
        await AssertStatus(
            async () => await AuthenticatedClient.SubmitAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task ShouldSubmitWithNoUploadedCommitteeListAndNoUploadedSignatureTypeMembers()
    {
        await RunOnDb(db => db.Files
            .Where(x => x.CommitteeListOfInitiativeId == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteDeleteAsync());

        await RunOnDb(db => db.InitiativeCommitteeMembers
            .Where(x => x.InitiativeId == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.SignatureType, InitiativeCommitteeMemberSignatureType.VerifiedIamIdentity)));

        await AuthenticatedClient.SubmitAsync(NewValidRequest());
        var initiative = await RunOnDb(db => db.Initiatives
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.State.Should().Be(CollectionState.Submitted);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
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

    private SubmitInitiativeRequest NewValidRequest(Action<SubmitInitiativeRequest>? customizer = null)
    {
        var request = new SubmitInitiativeRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeInPreparation,
        };
        customizer?.Invoke(request);
        return request;
    }
}
