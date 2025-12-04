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
using CollectionType = Voting.ECollecting.Proto.Shared.V1.Enums.CollectionType;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.CollectionTests;

public class CollectionSetSignatureSheetTemplateGeneratedTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionSetSignatureSheetTemplateGeneratedTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation));
    }

    [Fact]
    public async Task ShouldSetSignatureSheetGenerated()
    {
        var fileId = await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.SignatureSheetTemplate!.Id)
            .SingleAsync());

        await AuthenticatedClient.SetSignatureSheetTemplateGeneratedAsync(new SetSignatureSheetTemplateGeneratedRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeInPreparation,
            CollectionType = CollectionType.Initiative,
        });

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.SignatureSheetTemplate!.Content)
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.SignatureSheetTemplate.Should().NotBeNull();
        initiative.SignatureSheetTemplateGenerated.Should().BeTrue();

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
            await AuthenticatedClient.SetSignatureSheetTemplateGeneratedAsync(new SetSignatureSheetTemplateGeneratedRequest
            {
                Id = InitiativesCtStGallen.IdLegislativeInPreparation,
                CollectionType = CollectionType.Initiative,
            });
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldSetSignatureSheetGeneratedAsDeputy()
    {
        var fileId = await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .Select(x => x.SignatureSheetTemplate!.Id)
            .SingleAsync());

        await DeputyClient.SetSignatureSheetTemplateGeneratedAsync(new SetSignatureSheetTemplateGeneratedRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeInPreparation,
            CollectionType = CollectionType.Initiative,
        });

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.SignatureSheetTemplate!.Content)
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.SignatureSheetTemplate.Should().NotBeNull();
        initiative.SignatureSheetTemplateGenerated.Should().BeTrue();

        var hasOldFile = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == fileId));
        hasOldFile.Should().BeFalse();

        var hasOldFileContent = await RunOnDb(db => db.FileContents.AnyAsync(x => x.FileId == fileId));
        hasOldFileContent.Should().BeFalse();
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.SetSignatureSheetTemplateGeneratedAsync(new SetSignatureSheetTemplateGeneratedRequest
            {
                Id = "1eed8f27-077c-4839-b2d4-04a9ac3a62f6",
                CollectionType = CollectionType.Initiative,
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestCollectionTypeUnspecifiedInvalidArgument()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.SetSignatureSheetTemplateGeneratedAsync(new SetSignatureSheetTemplateGeneratedRequest
            {
                Id = "1eed8f27-077c-4839-b2d4-04a9ac3a62f6",
                CollectionType = CollectionType.Unspecified,
            }),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task AsReaderShouldFail()
    {
        await AssertStatus(
            async () => await ReaderClient.SetSignatureSheetTemplateGeneratedAsync(new SetSignatureSheetTemplateGeneratedRequest
            {
                Id = "1f82ef51-7a07-4855-8ac5-4d107fcc4895",
                CollectionType = CollectionType.Initiative,
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsDeputyNotAcceptedShouldFail()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.SetSignatureSheetTemplateGeneratedAsync(new SetSignatureSheetTemplateGeneratedRequest
            {
                Id = InitiativesCtStGallen.IdLegislativeInPreparation,
                CollectionType = CollectionType.Initiative,
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(
            async () => await Client.SetSignatureSheetTemplateGeneratedAsync(new SetSignatureSheetTemplateGeneratedRequest
            {
                Id = InitiativesCtStGallen.IdLegislativeInPreparation,
                CollectionType = CollectionType.Initiative,
            }),
            StatusCode.Unauthenticated);
    }

    [Fact]
    public async Task ShouldGenerateSignatureSheetWhenEnabledForCollection()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, CollectionState.EnabledForCollection)));

        await AuthenticatedClient.SetSignatureSheetTemplateGeneratedAsync(new SetSignatureSheetTemplateGeneratedRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeInPreparation,
            CollectionType = CollectionType.Initiative,
        });

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.SignatureSheetTemplate!.Content)
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation));
        initiative.SignatureSheetTemplateGenerated.Should().BeTrue();
        initiative.SignatureSheetTemplate.Should().NotBeNull();
        initiative.SignatureSheetTemplate!.Content.Should().NotBeNull();
        await VerifyJson(Encoding.UTF8.GetString(initiative.SignatureSheetTemplate!.Content!.Data));
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        if (state.IsEndedOrAborted())
        {
            await AssertStatus(
                async () => await AuthenticatedClient.SetSignatureSheetTemplateGeneratedAsync(new SetSignatureSheetTemplateGeneratedRequest
                {
                    Id = InitiativesCtStGallen.IdLegislativeInPreparation,
                    CollectionType = CollectionType.Initiative,
                }),
                StatusCode.NotFound);
        }
        else
        {
            await AuthenticatedClient.SetSignatureSheetTemplateGeneratedAsync(new SetSignatureSheetTemplateGeneratedRequest
            {
                Id = InitiativesCtStGallen.IdLegislativeInPreparation,
                CollectionType = CollectionType.Initiative,
            });
        }
    }
}
