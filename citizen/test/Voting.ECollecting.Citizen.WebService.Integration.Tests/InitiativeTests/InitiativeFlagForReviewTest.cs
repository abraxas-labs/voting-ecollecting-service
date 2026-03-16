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
using Voting.ECollecting.Shared.Domain.Entities.Audit;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;
using Voting.Lib.Database.Models;
using Voting.Lib.Testing;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeFlagForReviewTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeFlagForReviewTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeReturnedForCorrection, InitiativesCh.GuidReturnedForCorrection));
    }

    [Fact]
    public async Task ShouldSubmitAsCreator()
    {
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection,
            x =>
            {
                x.SignatureSheetTemplateGenerated = false;
                x.SignatureSheetTemplate = new FileEntity
                {
                    Name = "test.pdf",
                    ContentType = "application/pdf",
                    Content = new FileContentEntity { Data = Files.PlaceholderSignaturesPdf, },
                    AuditInfo = new AuditInfo
                    {
                        CreatedById = TestDefaults.UserId,
                        CreatedByName = TestDefaults.UserId,
                        CreatedAt = MockedClock.UtcNowDate,
                    },
                };
            });

        var oldFileId = await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection)
            .Select(x => x.SignatureSheetTemplateId)
            .SingleAsync());

        await AuthenticatedClient.FlagForReviewAsync(NewValidRequest());

        var initiative = await RunOnDb(db => db.Initiatives
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection));
        initiative.State.Should().Be(CollectionState.UnderReview);
        initiative.SignatureSheetTemplateId.Should().Be(oldFileId);

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection));
        await Verify(new { userNotifications, collectionMessage });
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection,
            x =>
            {
                x.SignatureSheetTemplateGenerated = false;
                x.SignatureSheetTemplate = new FileEntity
                {
                    Name = "test.pdf",
                    ContentType = "application/pdf",
                    Content = new FileContentEntity { Data = Files.PlaceholderSignaturesPdf, },
                    AuditInfo = new AuditInfo
                    {
                        CreatedById = TestDefaults.UserId,
                        CreatedByName = TestDefaults.UserId,
                        CreatedAt = MockedClock.UtcNowDate,
                    },
                };
            });

        await RunInAuditTrailTestScope(async () =>
        {
            await AuthenticatedClient.FlagForReviewAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldSubmitAsDeputy()
    {
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection,
            x =>
            {
                x.SignatureSheetTemplateGenerated = false;
                x.SignatureSheetTemplate = new FileEntity
                {
                    Name = "test.pdf",
                    ContentType = "application/pdf",
                    Content = new FileContentEntity { Data = Files.PlaceholderSignaturesPdf, },
                    AuditInfo = new AuditInfo
                    {
                        CreatedById = TestDefaults.UserId,
                        CreatedByName = TestDefaults.UserId,
                        CreatedAt = MockedClock.UtcNowDate,
                    },
                };
            });

        var oldFileId = await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection)
            .Select(x => x.SignatureSheetTemplateId)
            .SingleAsync());

        await DeputyClient.FlagForReviewAsync(NewValidRequest());

        var initiative = await RunOnDb(db => db.Initiatives
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection));
        initiative.State.Should().Be(CollectionState.UnderReview);
        initiative.SignatureSheetTemplateId.Should().Be(oldFileId);

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection));
        await Verify(new { userNotifications, collectionMessage });
    }

    [Fact]
    public async Task ShouldGenerateSignatureSheet()
    {
        var oldFileId = await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection)
            .Select(x => x.SignatureSheetTemplateId)
            .SingleAsync());

        await AuthenticatedClient.FlagForReviewAsync(NewValidRequest());

        var initiative = await RunOnDb(db => db.Initiatives
            .Include(x => x.SignatureSheetTemplate!.Content)
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection)
            .SingleAsync());

        await VerifyJson(Encoding.UTF8.GetString(initiative.SignatureSheetTemplate!.Content!.Data));
        initiative.SignatureSheetTemplate.Name.Should().Be($"Unterschriftenliste_{initiative.Description}.pdf");

        var oldFileExists = await RunOnDb(db => db.Files.AnyAsync(x => x.Id == oldFileId));
        oldFileExists.Should().BeFalse();
    }

    [Fact]
    public async Task LessApprovedCommitteeMembersShouldFail()
    {
        var config = GetService<CoreAppConfig>();
        config.InitiativeCommitteeMinApprovedMembersCount = 18;

        // ensure the app config is used.
        await ModifyDbEntities<DomainOfInfluenceEntity>(
            x => x.Bfs == Bfs.CantonStGallen,
            x => x.InitiativeNumberOfMembersCommittee = null);

        await AssertStatus(
            async () => await AuthenticatedClient.FlagForReviewAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task LessApprovedCommitteeMembersDefinedOnDoiShouldFail()
    {
        var config = GetService<CoreAppConfig>();
        config.InitiativeCommitteeMinApprovedMembersCount = 1;

        await ModifyDbEntities<DomainOfInfluenceEntity>(
            x => x.Bfs == Bfs.CantonStGallen,
            x => x.InitiativeNumberOfMembersCommittee = 50);

        await AssertStatus(
            async () => await AuthenticatedClient.FlagForReviewAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task ApprovedCommitteeMembersDefinedOnDoiShouldWork()
    {
        var config = GetService<CoreAppConfig>();
        config.InitiativeCommitteeMinApprovedMembersCount = 50;

        await ModifyDbEntities<DomainOfInfluenceEntity>(
            x => x.Bfs == Bfs.CantonStGallen,
            x => x.InitiativeNumberOfMembersCommittee = 1);

        await AuthenticatedClient.FlagForReviewAsync(NewValidRequest());
    }

    [Fact]
    public async Task MoreApprovedCommitteeMembersOnFederalLevelShouldFail()
    {
        var config = GetService<CoreAppConfig>();
        config.InitiativeCommitteeMaxApprovedMembersCount = 3;

        await AssertStatus(
            async () => await AuthenticatedClient.FlagForReviewAsync(NewValidRequest(x => x.Id = InitiativesCh.IdReturnedForCorrection)),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoDeputyPermissionShouldFail()
    {
        await RunOnDb(async db => await db.CollectionPermissions.ExecuteDeleteAsync());
        await AssertStatus(
            async () => await AuthenticatedClient.FlagForReviewAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoDescriptionShouldFail()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Description, string.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.FlagForReviewAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoWordingShouldFail()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Wording, MarkdownString.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.FlagForReviewAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoAddressCommitteeOrPersonShouldFail()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Address.CommitteeOrPerson, string.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.FlagForReviewAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoAddressStreetOrPostOfficeBoxShouldFail()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Address.StreetOrPostOfficeBox, string.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.FlagForReviewAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoAddressZipCodeShouldFail()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Address.ZipCode, string.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.FlagForReviewAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task NoAddressLocalityShouldFail()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.Address.Locality, string.Empty)));
        await AssertStatus(
            async () => await AuthenticatedClient.FlagForReviewAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task InPreparationWithAdmissibilityDecisionStateShouldWork()
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPreparation)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.AdmissibilityDecisionState, AdmissibilityDecisionState.Open)));
        await AssertStatus(
            async () => await AuthenticatedClient.FlagForReviewAsync(NewValidRequest(x => x.Id = InitiativesCtStGallen.IdLegislativeInPreparation)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task InPreparationWithoutAdmissibilityDecisionStateShouldFail()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.FlagForReviewAsync(NewValidRequest(x => x.Id = InitiativesCtStGallen.IdLegislativeInPreparation)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await AuthenticatedClient.FlagForReviewAsync(new FlagInitiativeForReviewRequest { Id = "69ca7ac2-5f41-4d5e-aa5b-c6be582675c1" }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsReaderShouldFail()
    {
        await AssertStatus(
            async () => await ReaderClient.FlagForReviewAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsDeputyNotAcceptedShouldFail()
    {
        await AssertStatus(
            async () => await DeputyNotAcceptedClient.FlagForReviewAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public Task UnauthenticatedShouldFail()
    {
        return AssertStatus(
            async () => await Client.FlagForReviewAsync(NewValidRequest()),
            StatusCode.Unauthenticated);
    }

    [Fact]
    public async Task NoUploadedCommitteeListShouldFail()
    {
        await RunOnDb(db => db.Files
            .Where(x => x.CommitteeListOfInitiativeId == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection)
            .ExecuteDeleteAsync());
        await AssertStatus(
            async () => await AuthenticatedClient.FlagForReviewAsync(NewValidRequest()),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task ShouldSubmitWithNoUploadedCommitteeListAndNoUploadedSignatureTypeMembers()
    {
        await RunOnDb(db => db.Files
            .Where(x => x.CommitteeListOfInitiativeId == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection)
            .ExecuteDeleteAsync());

        await RunOnDb(db => db.InitiativeCommitteeMembers
            .Where(x => x.InitiativeId == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.SignatureType, InitiativeCommitteeMemberSignatureType.VerifiedIamIdentity)));

        await AuthenticatedClient.FlagForReviewAsync(NewValidRequest());
        var initiative = await RunOnDb(db => db.Initiatives
            .FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection));
        initiative.State.Should().Be(CollectionState.UnderReview);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInStates(CollectionState state)
    {
        await RunOnDb(db => db.Initiatives
            .Where(x => x.Id == InitiativesCtStGallen.GuidLegislativeReturnedForCorrection)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.State, state)));

        if (state is CollectionState.InPreparation or CollectionState.ReturnedForCorrection)
        {
            await AuthenticatedClient.FlagForReviewAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await AuthenticatedClient.FlagForReviewAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    private FlagInitiativeForReviewRequest NewValidRequest(Action<FlagInitiativeForReviewRequest>? customizer = null)
    {
        var request = new FlagInitiativeForReviewRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeReturnedForCorrection,
        };
        customizer?.Invoke(request);
        return request;
    }
}
