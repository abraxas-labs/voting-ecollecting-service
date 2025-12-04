// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
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
using AdmissibilityDecisionState = Voting.ECollecting.Proto.Admin.Services.V1.Models.AdmissibilityDecisionState;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class InitiativeUpdateAdmissibilityDecisionTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeUpdateAdmissibilityDecisionTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Initiatives.WithInitiatives(
                InitiativesCtStGallen.GuidLegislativeSubmitted,
                InitiativesCtStGallen.GuidLegislativeSubmittedOpen,
                InitiativesCtStGallen.GuidLegislativeUnderReview,
                InitiativesMuStGallen.GuidSubmittedOpen));
    }

    [Fact]
    public async Task ShouldWorkAsCt()
    {
        await CtSgStammdatenverwalterClient.UpdateAdmissibilityDecisionAsync(NewValidRequest());
        var initiative = await CtSgStammdatenverwalterClient.GetAsync(new GetInitiativeRequest { Id = InitiativesCtStGallen.IdLegislativeSubmittedOpen });
        initiative.Collection.State.Should().Be(CollectionState.NotPassed);

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCtStGallen.GuidLegislativeSubmittedOpen)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesCtStGallen.GuidLegislativeSubmittedOpen));
        await Verify(new { initiative, userNotifications, collectionMessage });
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStammdatenverwalterClient.UpdateAdmissibilityDecisionAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldWorkAsCtValidSubjectToConditions()
    {
        var req = NewValidRequest(x => x.AdmissibilityDecisionState = AdmissibilityDecisionState.ValidButSubjectToConditions);
        await CtSgStammdatenverwalterClient.UpdateAdmissibilityDecisionAsync(req);
        var initiative = await CtSgStammdatenverwalterClient.GetAsync(new GetInitiativeRequest { Id = InitiativesCtStGallen.IdLegislativeSubmittedOpen });
        initiative.Collection.State.Should().Be(CollectionState.Submitted);
        await Verify(initiative);
    }

    [Fact]
    public async Task ShouldWorkAsMu()
    {
        await MuSgStammdatenverwalterClient.UpdateAdmissibilityDecisionAsync(NewValidRequest(x => x.InitiativeId = InitiativesMuStGallen.IdSubmittedOpen));
        var initiative = await MuSgStammdatenverwalterClient.GetAsync(new GetInitiativeRequest { Id = InitiativesMuStGallen.IdSubmittedOpen });
        initiative.Collection.State.Should().Be(CollectionState.NotPassed);
        await Verify(initiative);
    }

    [Fact]
    public async Task OtherMuShouldThrow()
    {
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.UpdateAdmissibilityDecisionAsync(NewValidRequest(x => x.InitiativeId = InitiativesMuStGallen.IdSubmittedOpen)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsCtOnMuShouldThrow()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.UpdateAdmissibilityDecisionAsync(NewValidRequest(x => x.InitiativeId = InitiativesMuStGallen.IdSubmittedOpen)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsMuOnCtShouldThrow()
    {
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.UpdateAdmissibilityDecisionAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task NotOpenShouldThrow()
    {
        var req = NewValidRequest(x =>
        {
            x.InitiativeId = InitiativesMuStGallen.IdSubmittedOpen;
            x.AdmissibilityDecisionState = AdmissibilityDecisionState.Open;
        });
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.UpdateAdmissibilityDecisionAsync(req),
            StatusCode.InvalidArgument,
            "Cannot update to Open state.");
    }

    [Fact]
    public async Task UpdateGovernmentDecisionNumberForOpenShouldWork()
    {
        var req = NewValidRequest(x =>
        {
            x.InitiativeId = InitiativesMuStGallen.IdSubmittedOpen;
            x.AdmissibilityDecisionState = AdmissibilityDecisionState.Open;
            x.GovernmentDecisionNumber = "newGDN";
        });
        await MuSgStammdatenverwalterClient.UpdateAdmissibilityDecisionAsync(req);
        var initiative = await CtSgStammdatenverwalterClient.GetAsync(new GetInitiativeRequest { Id = InitiativesMuStGallen.IdSubmittedOpen });
        initiative.AdmissibilityDecisionState.Should().Be(AdmissibilityDecisionState.Open);
        initiative.Collection.State.Should().Be(CollectionState.Submitted);
        initiative.GovernmentDecisionNumber.Should().Be("newGDN");
    }

    [Fact]
    public async Task UpdateGovernmentDecisionNumberAndStateForOpenShouldWork()
    {
        var req = NewValidRequest(x =>
        {
            x.InitiativeId = InitiativesMuStGallen.IdSubmittedOpen;
            x.AdmissibilityDecisionState = AdmissibilityDecisionState.Rejected;
            x.GovernmentDecisionNumber = "newGDN";
        });
        await MuSgStammdatenverwalterClient.UpdateAdmissibilityDecisionAsync(req);
        var initiative = await CtSgStammdatenverwalterClient.GetAsync(new GetInitiativeRequest { Id = InitiativesMuStGallen.IdSubmittedOpen });
        initiative.AdmissibilityDecisionState.Should().Be(AdmissibilityDecisionState.Rejected);
        initiative.Collection.State.Should().Be(CollectionState.NotPassed);
        initiative.GovernmentDecisionNumber.Should().Be("newGDN");
    }

    [Fact]
    public async Task RejectWithoutGovernmentDecisionNumberShouldThrow()
    {
        await ModifyDbEntities((InitiativeEntity e) => e.Id == InitiativesCtStGallen.GuidLegislativeSubmittedOpen, x => x.GovernmentDecisionNumber = string.Empty);
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.UpdateAdmissibilityDecisionAsync(NewValidRequest()),
            StatusCode.InvalidArgument,
            $"{nameof(ValidationException)}: Cannot set a state other than open without a government decision number.");
    }

    [Fact]
    public async Task RejectWithGovernmentDecisionNumberShouldWork()
    {
        await ModifyDbEntities((InitiativeEntity e) => e.Id == InitiativesCtStGallen.GuidLegislativeSubmittedOpen, x => x.GovernmentDecisionNumber = string.Empty);
        await CtSgStammdatenverwalterClient.UpdateAdmissibilityDecisionAsync(NewValidRequest(x => x.GovernmentDecisionNumber = "newGDN"));
        var initiative = await CtSgStammdatenverwalterClient.GetAsync(new GetInitiativeRequest { Id = InitiativesCtStGallen.IdLegislativeSubmittedOpen });
        initiative.AdmissibilityDecisionState.Should().Be(AdmissibilityDecisionState.Rejected);
        initiative.Collection.State.Should().Be(CollectionState.NotPassed);
        initiative.GovernmentDecisionNumber.Should().Be("newGDN");
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new InitiativeService.InitiativeServiceClient(channel)
            .UpdateAdmissibilityDecisionAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private UpdateAdmissibilityDecisionRequest NewValidRequest(
        Action<UpdateAdmissibilityDecisionRequest>? customizer = null)
    {
        var request = new UpdateAdmissibilityDecisionRequest
        {
            AdmissibilityDecisionState = AdmissibilityDecisionState.Rejected,
            InitiativeId = InitiativesCtStGallen.IdLegislativeSubmittedOpen,
        };

        customizer?.Invoke(request);
        return request;
    }
}
