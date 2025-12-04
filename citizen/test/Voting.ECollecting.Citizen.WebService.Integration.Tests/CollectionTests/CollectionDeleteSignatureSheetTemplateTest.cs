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
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.CollectionTests;

public class CollectionDeleteSignatureSheetTemplateTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionDeleteSignatureSheetTemplateTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation));
    }

    [Fact]
    public async Task ShouldDeleteSignatureSheet()
    {
        var fileId = await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.SignatureSheetTemplate!.Id)
            .SingleAsync());

        await AuthenticatedClient.DeleteSignatureSheetTemplateAsync(new DeleteSignatureSheetTemplateRequest { Id = InitiativesCtStGallen.IdLegislativeInPreparation });

        var initiative = await RunOnDb(db => db.Initiatives.FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.SignatureSheetTemplate.Should().BeNull();
        initiative.SignatureSheetTemplateGenerated.Should().BeFalse();

        var hasFile = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == fileId));
        hasFile.Should().BeFalse();

        var hasFileContent = await RunOnDb(db => db.FileContents.AnyAsync(x => x.FileId == fileId));
        hasFileContent.Should().BeFalse();
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await AuthenticatedClient.DeleteSignatureSheetTemplateAsync(new DeleteSignatureSheetTemplateRequest { Id = InitiativesCtStGallen.IdLegislativeInPreparation });
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldDeleteSignatureSheetAsDeputy()
    {
        var fileId = await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.SignatureSheetTemplate!.Id)
            .SingleAsync());
        await DeputyClient.DeleteSignatureSheetTemplateAsync(new DeleteSignatureSheetTemplateRequest { Id = InitiativesCtStGallen.IdLegislativeInPreparation });

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.SignatureSheetTemplate!.Content)
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.SignatureSheetTemplate.Should().BeNull();
        initiative.SignatureSheetTemplateGenerated.Should().BeFalse();

        var hasFile = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == fileId));
        hasFile.Should().BeFalse();

        var hasFileContent = await RunOnDb(db => db.FileContents.AnyAsync(x => x.FileId == fileId));
        hasFileContent.Should().BeFalse();
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.DeleteSignatureSheetTemplateAsync(new DeleteSignatureSheetTemplateRequest { Id = "1f82ef51-7a07-4855-8ac5-4d107fcc4895" }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsReaderShouldFail()
    {
        await AssertStatus(
            async () => await ReaderClient.DeleteSignatureSheetTemplateAsync(new DeleteSignatureSheetTemplateRequest { Id = InitiativesCtStGallen.IdLegislativeInPreparation }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsDeputyNotAcceptedShouldFail()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.DeleteSignatureSheetTemplateAsync(new DeleteSignatureSheetTemplateRequest { Id = InitiativesCtStGallen.IdLegislativeInPreparation }),
            StatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(
            async () => await Client.DeleteSignatureSheetTemplateAsync(new DeleteSignatureSheetTemplateRequest { Id = InitiativesCtStGallen.IdLegislativeInPreparation }),
            StatusCode.Unauthenticated);
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
            await AuthenticatedClient.DeleteSignatureSheetTemplateAsync(new DeleteSignatureSheetTemplateRequest { Id = InitiativesCtStGallen.IdLegislativeInPreparation });
        }
        else
        {
            await AssertStatus(
                async () => await AuthenticatedClient.DeleteSignatureSheetTemplateAsync(new DeleteSignatureSheetTemplateRequest { Id = InitiativesCtStGallen.IdLegislativeInPreparation }),
                StatusCode.NotFound);
        }
    }
}
