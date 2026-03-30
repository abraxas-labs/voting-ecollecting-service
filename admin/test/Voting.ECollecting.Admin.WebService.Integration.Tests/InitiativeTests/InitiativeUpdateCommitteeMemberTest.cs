// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
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

public class InitiativeUpdateCommitteeMemberTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    private readonly Guid _idCommitteeMemberCt = InitiativeCommitteeMembers.BuildGuid(
        InitiativesCtStGallen.GuidLegislativeInPreparation,
        "margarita@example.com");

    private readonly Guid _idCommitteeMemberMu = InitiativeCommitteeMembers.BuildGuid(
        InitiativesMuStGallen.GuidInPreparation,
        "margarita@example.com");

    public InitiativeUpdateCommitteeMemberTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation, InitiativesMuStGallen.GuidInPreparation));
    }

    [Fact]
    public async Task ShouldUpdateCommitteeMember()
    {
        await CtSgStammdatenverwalterClient.UpdateCommitteeMemberAsync(NewValidRequest());
        var member = await RunOnDb(db => db.InitiativeCommitteeMembers
            .FirstAsync(x => x.Id == _idCommitteeMemberCt));
        await Verify(member);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStammdatenverwalterClient.UpdateCommitteeMemberAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task AsMuOnOwnCollectionShouldWork()
    {
        await MuSgStammdatenverwalterClient.UpdateCommitteeMemberAsync(NewValidRequest(x =>
        {
            x.InitiativeId = InitiativesMuStGallen.IdInPreparation;
            x.Id = _idCommitteeMemberMu.ToString();
        }));
        var member = await RunOnDb(db => db.InitiativeCommitteeMembers
            .FirstAsync(x => x.Id == _idCommitteeMemberMu));
        await Verify(member);
    }

    [Fact]
    public async Task AsMuOnCtCollectionShouldFail()
    {
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.UpdateCommitteeMemberAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsCtOnMuCollectionShouldFail()
    {
        var req = NewValidRequest(x =>
        {
            x.InitiativeId = InitiativesMuStGallen.IdInPreparation;
            x.Id = _idCommitteeMemberMu.ToString();
        });
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.UpdateCommitteeMemberAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsMuOnOtherMuCollectionShouldFail()
    {
        var req = NewValidRequest(x =>
        {
            x.InitiativeId = InitiativesMuStGallen.IdInPreparation;
            x.Id = _idCommitteeMemberMu.ToString();
        });
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.UpdateCommitteeMemberAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.UpdateCommitteeMemberAsync(NewValidRequest(x => x.Id = "7d035b1c-73bf-4b53-bfa7-6851505a39e0")),
            StatusCode.NotFound);
    }

    [Theory]
    [EnumData<InitiativeCommitteeMemberApprovalState>]
    public async Task WorksInStates(InitiativeCommitteeMemberApprovalState state)
    {
        await ModifyDbEntities<InitiativeCommitteeMemberEntity>(
            x => x.Id == _idCommitteeMemberCt,
            x => x.ApprovalState = state);

        await CtSgStammdatenverwalterClient.UpdateCommitteeMemberAsync(NewValidRequest());
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new InitiativeService.InitiativeServiceClient(channel).UpdateCommitteeMemberAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private UpdateCommitteeMemberRequest NewValidRequest(Action<UpdateCommitteeMemberRequest>? customizer = null)
    {
        var request = new UpdateCommitteeMemberRequest
        {
            InitiativeId = InitiativesCtStGallen.IdLegislativeInPreparation,
            Id = _idCommitteeMemberCt.ToString(),
            PoliticalFirstName = "Margarita (updated)",
            PoliticalLastName = "Hamrick (updated)",
            PoliticalResidence = "Goldach",
            PoliticalDuty = "Gemeinderätin",
        };

        customizer?.Invoke(request);
        return request;
    }
}
