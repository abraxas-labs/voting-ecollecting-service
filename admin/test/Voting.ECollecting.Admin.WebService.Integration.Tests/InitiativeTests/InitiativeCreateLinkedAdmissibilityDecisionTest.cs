// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Models;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class InitiativeCreateLinkedAdmissibilityDecisionTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeCreateLinkedAdmissibilityDecisionTest(TestApplicationFactory factory)
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
                InitiativesMuStGallen.GuidSubmitted));
    }

    [Fact]
    public async Task ShouldWork()
    {
        await CtSgStammdatenverwalterClient.CreateLinkedAdmissibilityDecisionAsync(NewValidRequest());

        var initiative = await RunOnDb(db => db.Initiatives.FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeSubmitted));
        initiative.AdmissibilityDecisionState.Should().Be(Shared.Domain.Entities.AdmissibilityDecisionState.Valid);
        initiative.State.Should().Be(CollectionState.ReadyForRegistration);
        initiative.GovernmentDecisionNumber.Should().Be("123");

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == InitiativesCtStGallen.GuidLegislativeSubmitted)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == InitiativesCtStGallen.GuidLegislativeSubmitted));
        await Verify(new { userNotifications, collectionMessage });
    }

    [Fact]
    public async Task ShouldWorkValidSubjectToConditions()
    {
        await CtSgStammdatenverwalterClient.CreateLinkedAdmissibilityDecisionAsync(NewValidRequest(x => x.AdmissibilityDecisionState = AdmissibilityDecisionState.ValidButSubjectToConditions));

        var initiative = await RunOnDb(db => db.Initiatives.FirstAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeSubmitted));
        initiative.AdmissibilityDecisionState.Should().Be(Shared.Domain.Entities.AdmissibilityDecisionState.ValidButSubjectToConditions);
        initiative.State.Should().Be(CollectionState.Submitted);
        initiative.GovernmentDecisionNumber.Should().Be("123");
    }

    [Fact]
    public async Task ShouldWorkMu()
    {
        await MuSgStammdatenverwalterClient.CreateLinkedAdmissibilityDecisionAsync(NewValidRequest(x => x.InitiativeId = InitiativesMuStGallen.IdSubmitted));

        var initiative = await RunOnDb(db => db.Initiatives.FirstAsync(x => x.Id == InitiativesMuStGallen.GuidSubmitted));
        initiative.AdmissibilityDecisionState.Should().Be(Shared.Domain.Entities.AdmissibilityDecisionState.Valid);
        initiative.State.Should().Be(CollectionState.ReadyForRegistration);
        initiative.GovernmentDecisionNumber.Should().Be("123");
    }

    [Fact]
    public async Task ShouldThrowCtOnMu()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.CreateLinkedAdmissibilityDecisionAsync(NewValidRequest(x => x.InitiativeId = InitiativesMuStGallen.IdSubmitted)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowOtherMu()
    {
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.CreateLinkedAdmissibilityDecisionAsync(NewValidRequest(x => x.InitiativeId = InitiativesMuStGallen.IdSubmitted)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowNotFound()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.CreateLinkedAdmissibilityDecisionAsync(NewValidRequest(x => x.InitiativeId = "40d9d029-d9e5-4113-be9b-9bb6ec956acd")),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStammdatenverwalterClient.CreateLinkedAdmissibilityDecisionAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new InitiativeService.InitiativeServiceClient(channel)
            .CreateLinkedAdmissibilityDecisionAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private CreateLinkedAdmissibilityDecisionRequest NewValidRequest(Action<CreateLinkedAdmissibilityDecisionRequest>? customizer = null)
    {
        var req = new CreateLinkedAdmissibilityDecisionRequest
        {
            AdmissibilityDecisionState = AdmissibilityDecisionState.Valid,
            GovernmentDecisionNumber = "123",
            InitiativeId = InitiativesCtStGallen.IdLegislativeSubmitted,
        };
        customizer?.Invoke(req);
        return req;
    }
}
