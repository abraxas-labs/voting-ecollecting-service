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

public class CollectionDeleteImageTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionDeleteImageTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation, InitiativesMuStGallen.GuidInPreparation));
    }

    [Fact]
    public async Task ShouldDeleteImage()
    {
        var fileId = await RunOnDb(db => db.Initiatives.Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.Image!.Id)
            .SingleAsync());

        await CtSgStammdatenverwalterClient.DeleteImageAsync(NewValidRequest());

        var collection = await RunOnDb(db => db.Collections
            .Include(x => x.Image)
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        collection.Image.Should().BeNull();

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
            await CtSgStammdatenverwalterClient.DeleteImageAsync(NewValidRequest());
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

        await CtSgStammdatenverwalterClient.DeleteImageAsync(NewValidRequest());

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
    public async Task AsMuOnOwnCollectionShouldWork()
    {
        var fileId = await RunOnDb(db => db.Initiatives.Where(x => x.Id == InitiativesMuStGallen.GuidInPreparation)
            .Select(x => x.Image!.Id)
            .SingleAsync());

        await MuSgStammdatenverwalterClient.DeleteImageAsync(NewValidRequest(x => x.CollectionId = InitiativesMuStGallen.IdInPreparation));

        var collection = await RunOnDb(db => db.Collections
            .Include(x => x.Image)
            .FirstAsync(x => x.Id == InitiativesMuStGallen.GuidInPreparation));
        collection.Image.Should().BeNull();

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
            async () => await MuSgStammdatenverwalterClient.DeleteImageAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsMuOnOtherMuCollectionShouldFail()
    {
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.DeleteImageAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsCtOnMuCollectionShouldWork()
    {
        var fileId = await RunOnDb(db => db.Initiatives.Where(x => x.Id == InitiativesMuStGallen.GuidInPreparation)
            .Select(x => x.Image!.Id)
            .SingleAsync());
        await CtSgStammdatenverwalterClient.DeleteImageAsync(NewValidRequest(x => x.CollectionId = InitiativesMuStGallen.IdInPreparation));

        var exists = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == fileId));
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.DeleteImageAsync(NewValidRequest(x => x.CollectionId = "fffb515c-a261-482f-bd7d-7d6c5f419567")),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionService.CollectionServiceClient(channel)
            .DeleteImageAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private DeleteCollectionImageRequest NewValidRequest(Action<DeleteCollectionImageRequest>? customizer = null)
    {
        var request = new DeleteCollectionImageRequest
        {
            CollectionId = InitiativesCtStGallen.IdLegislativeInPreparation,
            CollectionType = CollectionType.Initiative,
        };
        customizer?.Invoke(request);
        return request;
    }
}
