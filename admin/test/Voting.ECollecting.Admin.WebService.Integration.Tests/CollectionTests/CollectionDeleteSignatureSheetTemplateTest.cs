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
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionDeleteSignatureSheetTemplateTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionDeleteSignatureSheetTemplateTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation, InitiativesMuStGallen.GuidInPreparation));
    }

    [Fact]
    public async Task ShouldDeleteSignatureSheet()
    {
        var oldFileId = await RunOnDb(db => db.Initiatives.Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.SignatureSheetTemplate!.Id)
            .SingleAsync());

        var response = await CtSgStammdatenverwalterClient.DeleteSignatureSheetTemplateAsync(NewValidRequest());

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.SignatureSheetTemplate!.Content)
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .SingleAsync());

        var fileData = Encoding.UTF8.GetString(initiative.SignatureSheetTemplate!.Content!.Data);
        initiative.SignatureSheetTemplate.Name.Should().Be($"Unterschriftenliste_{initiative.Description}.pdf");

        var oldFileExists = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == oldFileId));
        oldFileExists.Should().BeFalse();

        var collection = await RunOnDb(db => db.Collections
            .Include(x => x.SignatureSheetTemplate)
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        collection.SignatureSheetTemplate.Should().NotBeNull();
        collection.SignatureSheetTemplateGenerated.Should().BeTrue();

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesCtStGallen.GuidLegislativeInPreparation));
        await Verify(new { fileData, userNotifications, collectionMessage, response });
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStammdatenverwalterClient.DeleteSignatureSheetTemplateAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task AsMuOnOwnCollectionShouldWork()
    {
        var oldFileId = await RunOnDb(db => db.Initiatives.Where(x => x.Id == InitiativesMuStGallen.GuidInPreparation)
            .Select(x => x.SignatureSheetTemplate!.Id)
            .SingleAsync());

        var response = await MuSgStammdatenverwalterClient.DeleteSignatureSheetTemplateAsync(NewValidRequest(x => x.CollectionId = InitiativesMuStGallen.IdInPreparation));

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.SignatureSheetTemplate!.Content)
            .Where(x => x.Id == InitiativesMuStGallen.GuidInPreparation)
            .SingleAsync());

        var fileData = Encoding.UTF8.GetString(initiative.SignatureSheetTemplate!.Content!.Data);
        initiative.SignatureSheetTemplate.Name.Should().Be($"Unterschriftenliste_{initiative.Description}.pdf");

        var oldFileExists = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == oldFileId));
        oldFileExists.Should().BeFalse();

        var collection = await RunOnDb(db => db.Collections
            .Include(x => x.SignatureSheetTemplate)
            .FirstAsync(x => x.Id == InitiativesMuStGallen.GuidInPreparation));
        collection.SignatureSheetTemplate.Should().NotBeNull();
        collection.SignatureSheetTemplateGenerated.Should().BeTrue();

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesMuStGallen.GuidInPreparation)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesMuStGallen.GuidInPreparation));
        await Verify(new { fileData, userNotifications, collectionMessage, response });
    }

    [Fact]
    public async Task AsMuOnCtCollectionShouldFail()
    {
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.DeleteSignatureSheetTemplateAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsMuOnOtherMuCollectionShouldFail()
    {
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.DeleteSignatureSheetTemplateAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsCtOnMuCollectionShouldWork()
    {
        var oldFileId = await RunOnDb(db => db.Initiatives.Where(x => x.Id == InitiativesMuStGallen.GuidInPreparation)
            .Select(x => x.SignatureSheetTemplate!.Id)
            .SingleAsync());
        await CtSgStammdatenverwalterClient.DeleteSignatureSheetTemplateAsync(NewValidRequest(x => x.CollectionId = InitiativesMuStGallen.IdInPreparation));

        var exists = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == oldFileId));
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.DeleteSignatureSheetTemplateAsync(NewValidRequest(x => x.CollectionId = "fffb515c-a261-482f-bd7d-7d6c5f419567")),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionService.CollectionServiceClient(channel)
            .DeleteSignatureSheetTemplateAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private DeleteSignatureSheetTemplateRequest NewValidRequest(Action<DeleteSignatureSheetTemplateRequest>? customizer = null)
    {
        var request = new DeleteSignatureSheetTemplateRequest
        {
            CollectionId = InitiativesCtStGallen.IdLegislativeInPreparation,
            CollectionType = CollectionType.Initiative,
        };
        customizer?.Invoke(request);
        return request;
    }
}
