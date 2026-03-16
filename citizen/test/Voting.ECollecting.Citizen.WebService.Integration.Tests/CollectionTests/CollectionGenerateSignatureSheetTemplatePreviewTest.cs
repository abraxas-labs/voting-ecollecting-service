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
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;
using Voting.Lib.Database.Models;
using CollectionType = Voting.ECollecting.Proto.Shared.V1.Enums.CollectionType;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.CollectionTests;

public class CollectionGenerateSignatureSheetTemplatePreviewTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionGenerateSignatureSheetTemplatePreviewTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Default.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation).WithReferendums(ReferendumsCtStGallen.GuidInPreparation));
    }

    [Fact]
    public async Task ShouldWorkWithInitiative()
    {
        var oldFileId = await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.SignatureSheetTemplateId)
            .SingleAsync());

        var response = await AuthenticatedClient.GenerateSignatureSheetTemplatePreviewAsync(NewValidRequest());
        await Verify(response).UseMethodName(nameof(ShouldWorkWithInitiative) + "_response");

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.SignatureSheetTemplate!.Content)
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .SingleAsync());

        await VerifyJson(Encoding.UTF8.GetString(initiative.SignatureSheetTemplate!.Content!.Data));
        initiative.SignatureSheetTemplate.Name.Should().Be($"Unterschriftenliste_{initiative.Description}.pdf");

        var oldFileExists = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == oldFileId));
        oldFileExists.Should().BeFalse();
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await AuthenticatedClient.GenerateSignatureSheetTemplatePreviewAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldWorkWithReferendum()
    {
        var oldFileId = await RunOnDb(db => db.Referendums
            .Where(x => x.Id == ReferendumsCtStGallen.GuidInPreparation)
            .Select(x => x.SignatureSheetTemplateId)
            .SingleAsync());

        var response = await AuthenticatedClient.GenerateSignatureSheetTemplatePreviewAsync(NewValidRequest(x =>
        {
            x.Id = ReferendumsCtStGallen.IdInPreparation;
            x.CollectionType = CollectionType.Referendum;
        }));
        await Verify(response).UseMethodName(nameof(ShouldWorkWithReferendum) + "_response");

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
    public async Task ShouldWorkAsDeputy()
    {
        var response = await DeputyClient.GenerateSignatureSheetTemplatePreviewAsync(NewValidRequest());
        await Verify(response).UseMethodName(nameof(ShouldWorkAsDeputy) + "_response");

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.SignatureSheetTemplate!.Content)
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .SingleAsync());

        await VerifyJson(Encoding.UTF8.GetString(initiative.SignatureSheetTemplate!.Content!.Data));
        initiative.SignatureSheetTemplate.Name.Should().Be($"Unterschriftenliste_{initiative.Description}.pdf");
    }

    [Fact]
    public async Task NoDescriptionShouldFail()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Description, string.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.GenerateSignatureSheetTemplatePreviewAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoWordingShouldFail()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Wording, MarkdownString.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.GenerateSignatureSheetTemplatePreviewAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoAddressCommitteeOrPersonShouldFail()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Address.CommitteeOrPerson, string.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.GenerateSignatureSheetTemplatePreviewAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoAddressStreetOrPostOfficeBoxShouldFail()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Address.StreetOrPostOfficeBox, string.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.GenerateSignatureSheetTemplatePreviewAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoAddressZipCodeShouldFail()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Address.ZipCode, string.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.GenerateSignatureSheetTemplatePreviewAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoAddressLocalityShouldFail()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Address.Locality, string.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.GenerateSignatureSheetTemplatePreviewAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task UnspecifiedCollectionTypeShouldFail()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.GenerateSignatureSheetTemplatePreviewAsync(NewValidRequest(x => x.CollectionType = CollectionType.Unspecified)),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task ShouldThrowAsDeputyNotAccepted()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.GenerateSignatureSheetTemplatePreviewAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsReader()
    {
        await AssertStatus(
            async () => await ReaderClient.GenerateSignatureSheetTemplatePreviewAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsReaderNotAccepted()
    {
        await AssertStatus(
            async () => await ReaderNotAcceptedClient.GenerateSignatureSheetTemplatePreviewAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowUnauthenticated()
    {
        await AssertStatus(
            async () => await Client.GenerateSignatureSheetTemplatePreviewAsync(NewValidRequest()),
            StatusCode.Unauthenticated);
    }

    [Fact]
    public async Task ShouldThrowWithoutPermissions()
    {
        await AssertStatus(
            async () => await AuthenticatedNoPermissionClient.GenerateSignatureSheetTemplatePreviewAsync(NewValidRequest()),
            StatusCode.NotFound);
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
            await AuthenticatedClient.GenerateSignatureSheetTemplatePreviewAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await AuthenticatedClient.GenerateSignatureSheetTemplatePreviewAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    private GenerateSignatureSheetTemplatePreviewRequest NewValidRequest(Action<GenerateSignatureSheetTemplatePreviewRequest>? customizer = null)
    {
        var request = new GenerateSignatureSheetTemplatePreviewRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeInPreparation,
            CollectionType = CollectionType.Initiative,
        };

        customizer?.Invoke(request);
        return request;
    }
}
