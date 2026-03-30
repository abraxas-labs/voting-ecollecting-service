// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using Grpc.Core;
using Grpc.Net.Client;
using Voting.ECollecting.Admin.Core.Exceptions;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.ModelBuilders;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class UpdateInitiativeTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public UpdateInitiativeTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives
            .WithInitiatives(
                InitiativesMuStGallen.GuidPreRecorded,
                InitiativesCtStGallen.GuidLegislativeInPaperSubmission,
                InitiativesCtStGallen.GuidLegislativeInPaperSubmissionAdmissibilityDecisionValid));
    }

    [Fact]
    public async Task CtShouldWork()
    {
        await CtSgStammdatenverwalterClient.UpdateAsync(NewValidRequest());
        var initiative = await
            CtSgStammdatenverwalterClient.GetAsync(new GetInitiativeRequest
            {
                Id = InitiativesCtStGallen.IdLegislativeInPaperSubmission,
            });
        await Verify(initiative);
    }

    [Fact]
    public async Task MuShouldWork()
    {
        await MuSgStammdatenverwalterClient.UpdateAsync(NewValidMuRequest());
        var initiative = await
            MuSgStammdatenverwalterClient.GetAsync(new GetInitiativeRequest
            {
                Id = InitiativesMuStGallen.IdPreRecorded,
            });
        await Verify(initiative);
    }

    [Fact]
    public async Task CtOnMuShouldThrow()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.UpdateAsync(NewValidMuRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task MuOnOtherMuShouldThrow()
    {
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.UpdateAsync(NewValidMuRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task CtUnrelatedSubTypeShouldThrow()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.UpdateAsync(NewValidRequest(x => x.SubTypeId = InitiativeModelBuilder.FederalId.ToString())),
            StatusCode.NotFound,
            nameof(InitiativeSubTypeEntity));
    }

    [Fact]
    public async Task CtNoSubTypeShouldThrow()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.UpdateAsync(NewValidRequest(x => x.SubTypeId = string.Empty)),
            StatusCode.InvalidArgument,
            nameof(ValidationException) + ": SubType is required for cantonal initiatives.");
    }

    [Fact]
    public async Task CtNoWordingShouldThrow()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.UpdateAsync(NewValidRequest(x => x.Wording = string.Empty)),
            StatusCode.InvalidArgument,
            nameof(ValidationException) + ": Wording is required for non-federal initiatives.");
    }

    [Fact]
    public async Task CtInEndedShouldThrow()
    {
        await ModifyDbEntities(
            (InitiativeEntity i) => i.Id == InitiativesCtStGallen.GuidLegislativeInPaperSubmission,
            i => i.State = CollectionState.EndedCameAbout);
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.UpdateAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task CtDuplicateShouldThrow()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.UpdateAsync(NewValidRequest(x => x.Description = "Für kantonale Brunch-Treffs (Kantons-Brunch-Initiative)")),
            StatusCode.FailedPrecondition,
            nameof(CollectionAlreadyExistsException));
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await MuSgStammdatenverwalterClient.UpdateAsync(NewValidMuRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new InitiativeService.InitiativeServiceClient(channel)
            .UpdateAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles() => [Roles.Stammdatenverwalter];

    private UpdateInitiativeRequest NewValidRequest(Action<UpdateInitiativeRequest>? init = null)
    {
        var req = new UpdateInitiativeRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeInPaperSubmission,
            SubTypeId = InitiativeModelBuilder.LegislativeId.ToString(),
            Description = "foo bar baz-updated",
            Reason = "reason-updated",
            Address = new()
            {
                CommitteeOrPerson = "Hans Muster-updated",
                StreetOrPostOfficeBox = "Postfach 12-updated",
                ZipCode = "9001-updated",
                Locality = "St.Gallen-updated",
            },
            Wording = "Für mehr gerechtigkeit!-updated",
        };
        init?.Invoke(req);
        return req;
    }

    private UpdateInitiativeRequest NewValidMuRequest()
    {
        return NewValidRequest(x =>
        {
            x.Id = InitiativesMuStGallen.IdPreRecorded;
            x.SubTypeId = string.Empty;
        });
    }
}
