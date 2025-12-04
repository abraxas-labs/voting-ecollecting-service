// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Net.Client;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class InitiativeListAdmissibilityDecisionsTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeListAdmissibilityDecisionsTest(TestApplicationFactory factory)
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
                InitiativesCtStGallen.GuidLegislativeInPaperSubmissionAdmissibilityDecisionValid,
                InitiativesCtStGallen.GuidLegislativeInPaperSubmissionAdmissibilityDecisionValidButSubjectToConditions,
                InitiativesCtStGallen.GuidLegislativeInPaperSubmissionAdmissibilityDecisionRejected,
                InitiativesMuGoldach.GuidEnabledForCollection,
                InitiativesMuStGallen.GuidEnabledForCollectionCollecting));
    }

    [Fact]
    public async Task ShouldWork()
    {
        var resp =
            await CtSgStammdatenverwalterClient.ListAdmissibilityDecisionsAsync(
                new ListAdmissibilityDecisionsRequest());
        await Verify(resp);
    }

    [Fact]
    public async Task ShouldWorkAsMu()
    {
        var resp =
            await MuSgStammdatenverwalterClient.ListAdmissibilityDecisionsAsync(
                new ListAdmissibilityDecisionsRequest());
        await Verify(resp);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new InitiativeService.InitiativeServiceClient(channel)
            .ListAdmissibilityDecisionsAsync(new ListAdmissibilityDecisionsRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }
}
