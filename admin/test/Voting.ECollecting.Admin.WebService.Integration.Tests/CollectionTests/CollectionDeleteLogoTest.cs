// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Text;
using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionDeleteLogoTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionDeleteLogoTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation, InitiativesMuStGallen.GuidInPreparation));
    }

    [Fact]
    public async Task ShouldDeleteLogo()
    {
        var fileId = await RunOnDb(db => db.Initiatives.Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.Logo!.Id)
            .SingleAsync());

        var response = await CtSgStammdatenverwalterClient.DeleteLogoAsync(NewValidRequest());
        response.GeneratedSignatureSheetTemplate.Should().BeNull();

        var collection = await RunOnDb(db => db.Collections
            .Include(x => x.Logo)
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        collection.Logo.Should().BeNull();

        var hasFile = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == fileId));
        hasFile.Should().BeFalse();

        var hasFileContent = await RunOnDb(db => db.FileContents.AnyAsync(x => x.FileId == fileId));
        hasFileContent.Should().BeFalse();

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
            await CtSgStammdatenverwalterClient.DeleteLogoAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldGenerateSignatureSheet()
    {
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation,
            x => x.SignatureSheetTemplateGenerated = true);

        var oldFileId = await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.SignatureSheetTemplateId)
            .SingleAsync());

        var response = await CtSgStammdatenverwalterClient.DeleteLogoAsync(NewValidRequest());
        await Verify(response).UseMethodName(nameof(ShouldGenerateSignatureSheet) + "_response");

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
    public async Task AsMuOnOwnCollectionShouldWork()
    {
        var fileId = await RunOnDb(db => db.Initiatives.Where(x => x.Id == InitiativesMuStGallen.GuidInPreparation)
            .Select(x => x.Logo!.Id)
            .SingleAsync());

        var response = await MuSgStammdatenverwalterClient.DeleteLogoAsync(NewValidRequest(x => x.CollectionId = InitiativesMuStGallen.IdInPreparation));
        response.GeneratedSignatureSheetTemplate.Should().BeNull();

        var collection = await RunOnDb(db => db.Collections
            .Include(x => x.Logo)
            .FirstAsync(x => x.Id == InitiativesMuStGallen.GuidInPreparation));
        collection.Logo.Should().BeNull();

        var hasFile = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == fileId));
        hasFile.Should().BeFalse();

        var hasFileContent = await RunOnDb(db => db.FileContents.AnyAsync(x => x.FileId == fileId));
        hasFileContent.Should().BeFalse();

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesMuStGallen.GuidInPreparation)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesMuStGallen.GuidInPreparation));
        await Verify(new { userNotifications, collectionMessage });
    }

    [Fact]
    public async Task AsMuOnCtCollectionShouldFail()
    {
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.DeleteLogoAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsMuOnOtherMuCollectionShouldFail()
    {
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.DeleteLogoAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsCtOnMuCollectionShouldWork()
    {
        var fileId = await RunOnDb(db => db.Initiatives.Where(x => x.Id == InitiativesMuStGallen.GuidInPreparation)
            .Select(x => x.LogoId)
            .SingleAsync());
        await CtSgStammdatenverwalterClient.DeleteLogoAsync(NewValidRequest(x => x.CollectionId = InitiativesMuStGallen.IdInPreparation));

        var exists = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == fileId));
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.DeleteLogoAsync(NewValidRequest(x => x.CollectionId = "fffb515c-a261-482f-bd7d-7d6c5f419567")),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task NoLogoShouldFail()
    {
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation,
            x => x.LogoId = null);

        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.DeleteLogoAsync(NewValidRequest()),
            StatusCode.InvalidArgument,
            "Logo cannot be deleted because it is not set");
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionService.CollectionServiceClient(channel)
            .DeleteLogoAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private DeleteCollectionLogoRequest NewValidRequest(Action<DeleteCollectionLogoRequest>? customizer = null)
    {
        var request = new DeleteCollectionLogoRequest
        {
            CollectionId = InitiativesCtStGallen.IdLegislativeInPreparation,
            CollectionType = CollectionType.Initiative,
        };
        customizer?.Invoke(request);
        return request;
    }
}
