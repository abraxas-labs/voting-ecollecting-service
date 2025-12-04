// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Grpc.Net.Client;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class InitiativeVerifyCommitteeMemberTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    private readonly Guid _idCommitteeMemberCt = InitiativeCommitteeMembers.BuildGuid(
        InitiativesCtStGallen.GuidLegislativeInPreparation,
        "thomas.keller@example.com");

    private readonly Guid _idCommitteeMemberMu = InitiativeCommitteeMembers.BuildGuid(
        InitiativesMuStGallen.GuidInPreparation,
        "thomas.keller@example.com");

    private readonly Guid _idCommitteeMemberNotFound = InitiativeCommitteeMembers.BuildGuid(
        InitiativesCtStGallen.GuidLegislativeInPreparation,
        "lukas.meier@example.com");

    public InitiativeVerifyCommitteeMemberTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation, InitiativesMuStGallen.GuidInPreparation));
    }

    [Fact]
    public async Task ShouldVerifyCommitteeMember()
    {
        var response = await CtSgStammdatenverwalterClient.VerifyCommitteeMemberAsync(NewValidRequest());
        await Verify(response);
    }

    [Fact]
    public async Task AsMuOnOwnCollectionShouldWork()
    {
        var response = await MuSgStammdatenverwalterClient.VerifyCommitteeMemberAsync(NewValidRequest(x =>
        {
            x.InitiativeId = InitiativesMuStGallen.IdInPreparation;
            x.Id = _idCommitteeMemberMu.ToString();
        }));
        await Verify(response);
    }

    [Fact]
    public async Task AsMuOnCtCollectionShouldFail()
    {
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.VerifyCommitteeMemberAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsMuOnOtherMuCollectionShouldFail()
    {
        var req = NewValidRequest(x => x.Id = InitiativesMuStGallen.IdSubmitted);
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.VerifyCommitteeMemberAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsCtOnMuCollectionShouldFail()
    {
        var req = NewValidRequest(x => x.Id = InitiativesMuStGallen.IdSubmitted);
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.VerifyCommitteeMemberAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestPersonNotFound()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.VerifyCommitteeMemberAsync(NewValidRequest(x => x.Id = _idCommitteeMemberNotFound.ToString())),
            StatusCode.NotFound,
            "PersonNotFoundException");
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.VerifyCommitteeMemberAsync(NewValidRequest(x => x.Id = "36ef5cb2-a227-4ef1-97f6-2a555d3ceafc")),
            StatusCode.NotFound);
    }

    [Theory]
    [EnumData<InitiativeCommitteeMemberApprovalState>]
    public async Task WorksInStates(InitiativeCommitteeMemberApprovalState state)
    {
        await ModifyDbEntities<InitiativeCommitteeMemberEntity>(
            x => x.Id == _idCommitteeMemberCt,
            x => x.ApprovalState = state);

        if (state is InitiativeCommitteeMemberApprovalState.Requested or InitiativeCommitteeMemberApprovalState.Signed)
        {
            await CtSgStammdatenverwalterClient.VerifyCommitteeMemberAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await CtSgStammdatenverwalterClient.VerifyCommitteeMemberAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new InitiativeService.InitiativeServiceClient(channel).VerifyCommitteeMemberAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private VerifyCommitteeMemberRequest NewValidRequest(Action<VerifyCommitteeMemberRequest>? customizer = null)
    {
        var request = new VerifyCommitteeMemberRequest
        {
            InitiativeId = InitiativesCtStGallen.IdLegislativeInPreparation,
            Id = _idCommitteeMemberCt.ToString(),
        };

        customizer?.Invoke(request);
        return request;
    }
}
