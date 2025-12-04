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
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class InitiativeDeleteAdmissibilityDecisionTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeDeleteAdmissibilityDecisionTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Initiatives.WithInitiatives(
                InitiativesCtStGallen.GuidLegislativeInPaperSubmission,
                InitiativesCtStGallen.GuidLegislativeInPaperSubmissionAdmissibilityDecisionValidInCollection,
                InitiativesCtStGallen.GuidLegislativeInPreparation,
                InitiativesMuStGallen.GuidPreRecorded,
                InitiativesMuGoldach.GuidPreRecorded));
    }

    [Fact]
    public async Task ShouldWork()
    {
        await CtSgStammdatenverwalterClient.DeleteAdmissibilityDecisionAsync(NewValidRequest());

        var exists = await RunOnDb(db =>
            db.Initiatives.AnyAsync(x => x.Id == InitiativesCtStGallen.GuidLegislativeInPaperSubmission));
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldThrowOtherState()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.DeleteAdmissibilityDecisionAsync(new DeleteAdmissibilityDecisionRequest
            {
                Id = InitiativesCtStGallen.IdLegislativeInPreparation,
            }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowInCollection()
    {
        var req = new DeleteAdmissibilityDecisionRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeInPaperSubmissionAdmissibilityDecisionValidInCollection,
        };
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.DeleteAdmissibilityDecisionAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldWorkAsMu()
    {
        await MuSgStammdatenverwalterClient.DeleteAdmissibilityDecisionAsync(new DeleteAdmissibilityDecisionRequest
        {
            Id = InitiativesMuStGallen.IdPreRecorded,
        });

        var exists = await RunOnDb(db =>
            db.Initiatives.AnyAsync(x => x.Id == InitiativesMuStGallen.GuidPreRecorded));
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldThrowDeleteTwice()
    {
        await CtSgStammdatenverwalterClient.DeleteAdmissibilityDecisionAsync(NewValidRequest());
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.DeleteAdmissibilityDecisionAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowNotFound()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.DeleteAdmissibilityDecisionAsync(
                new DeleteAdmissibilityDecisionRequest { Id = "751491a1-17b6-47b1-aad2-ed1fd8bf8271" }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowOtherMu()
    {
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.DeleteAdmissibilityDecisionAsync(
                new DeleteAdmissibilityDecisionRequest { Id = InitiativesMuGoldach.IdPreRecorded }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsCtOnMu()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.DeleteAdmissibilityDecisionAsync(
                new DeleteAdmissibilityDecisionRequest { Id = InitiativesMuGoldach.IdPreRecorded }),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStammdatenverwalterClient.DeleteAdmissibilityDecisionAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new InitiativeService.InitiativeServiceClient(channel)
            .DeleteAdmissibilityDecisionAsync(new DeleteAdmissibilityDecisionRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private DeleteAdmissibilityDecisionRequest NewValidRequest()
    {
        return new DeleteAdmissibilityDecisionRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeInPaperSubmission,
        };
    }
}
