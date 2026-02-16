// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Adapter.VotingStimmregister;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;
using CollectionType = Voting.ECollecting.Proto.Shared.V1.Enums.CollectionType;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionConfirmSignatureSheetTest : BaseGrpcTest<CollectionSignatureSheetService.CollectionSignatureSheetServiceClient>
{
    private static readonly Guid _municipalityCtSgId = CollectionMunicipalities.BuildGuid(
        ReferendumsCtStGallen.GuidSignatureSheetsSubmitted,
        Bfs.MunicipalityStGallen);

    private static readonly Guid _municipalityMuSgId = CollectionMunicipalities.BuildGuid(
        ReferendumsMuStGallen.GuidSignatureSheetsSubmitted,
        Bfs.MunicipalityStGallen);

    private static readonly Guid _sheetCtSgId = CollectionSignatureSheets.BuildGuid(_municipalityCtSgId, 6);
    private static readonly Guid _sheetMuSgId = CollectionSignatureSheets.BuildGuid(_municipalityMuSgId, 6);

    public CollectionConfirmSignatureSheetTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Default
                    .WithReferendums(
                        ReferendumsCtStGallen.GuidSignatureSheetsSubmitted,
                        ReferendumsCtStGallen.GuidPastEndedCameAbout,
                        ReferendumsMuStGallen.GuidSignatureSheetsSubmitted,
                        ReferendumsMuGoldach.GuidSignatureSheetsSubmitted)
                    .WithInitiatives(InitiativesCtStGallen.GuidUnitySignatureSheetsSubmitted) with
            {
                SeedReferendumSignatureSheets = true,
                SeedReferendumCitizens = true,
                SeedInitiativeSignatureSheets = true,
                SeedInitiativeCitizens = true,
            });
    }

    [Fact]
    public async Task ShouldWork()
    {
        var prevCount = await RunOnDb(db => db.CollectionCounts
            .SingleAsync(x => x.CollectionId == ReferendumsCtStGallen.GuidSignatureSheetsSubmitted));

        var prevCollectionMunicipality = await RunOnDb(db => db.CollectionMunicipalities
            .SingleAsync(x => x.Id == _municipalityCtSgId));

        var prevSheet = await RunOnDb(db => db.CollectionSignatureSheets
            .SingleAsync(x => x.Id == _sheetCtSgId));

        var req = NewValidRequest();
        var response = await CtSgStichprobenverwalterClient.ConfirmAsync(req);
        response.NextSignatureSheetId.Should().Be(CollectionSignatureSheets.BuildGuid(_municipalityCtSgId, 7).ToString());

        var sheet = await RunOnDb(db => db.CollectionSignatureSheets
            .SingleAsync(x => x.Id == _sheetCtSgId));
        sheet.State.Should().Be(CollectionSignatureSheetState.Confirmed);
        sheet.ModifiedBySuperiorAuthority.Should().BeTrue();

        var collectionMunicipality = await RunOnDb(db => db.CollectionMunicipalities
            .SingleAsync(x => x.Id == _municipalityCtSgId));

        var collectionCount = await RunOnDb(db => db.CollectionCounts
            .SingleAsync(x => x.CollectionId == ReferendumsCtStGallen.GuidSignatureSheetsSubmitted));

        // 2 added and 1 removed should result in an increase of 1
        collectionMunicipality.PhysicalCount.Valid.Should().Be(prevCollectionMunicipality.PhysicalCount.Valid + 1);
        collectionCount.TotalCitizenCount.Should().Be(prevCount.TotalCitizenCount + 1);
        sheet.Count.Valid.Should().Be(prevSheet.Count.Valid + 1);

        // total count before 25, now 55, minus +1 new valid count should result in an increase of 29
        collectionMunicipality.PhysicalCount.Invalid.Should().Be(prevCollectionMunicipality.PhysicalCount.Invalid + 29);
        sheet.Count.Invalid.Should().Be(prevSheet.Count.Invalid + 29);

        collectionCount.ElectronicCitizenCount.Should().Be(prevCount.ElectronicCitizenCount);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            var response = await CtSgStichprobenverwalterClient.ConfirmAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries())
                .ScrubMember("VotingStimmregisterIdEncrypted");
        });
    }

    [Fact]
    public async Task ShouldWorkAsMu()
    {
        var prevCount = await RunOnDb(db => db.CollectionCounts
            .SingleAsync(x => x.CollectionId == ReferendumsMuStGallen.GuidSignatureSheetsSubmitted));

        var prevCollectionMunicipality = await RunOnDb(db => db.CollectionMunicipalities
            .SingleAsync(x => x.Id == _municipalityMuSgId));

        var prevSheet = await RunOnDb(db => db.CollectionSignatureSheets
            .SingleAsync(x => x.Id == _sheetMuSgId));

        var req = NewValidRequest();
        req.CollectionId = ReferendumsMuStGallen.IdSignatureSheetsSubmitted;
        req.SignatureSheetId = _sheetMuSgId.ToString();
        var response = await MuSgStichprobenverwalterClient.ConfirmAsync(req);
        response.NextSignatureSheetId.Should().Be(CollectionSignatureSheets.BuildGuid(_municipalityMuSgId, 7).ToString());

        var sheet = await RunOnDb(db => db.CollectionSignatureSheets
            .SingleAsync(x => x.Id == _sheetMuSgId));
        sheet.State.Should().Be(CollectionSignatureSheetState.Confirmed);

        var collectionMunicipality = await RunOnDb(db => db.CollectionMunicipalities
            .SingleAsync(x => x.Id == _municipalityMuSgId));

        var collectionCount = await RunOnDb(db => db.CollectionCounts
            .SingleAsync(x => x.CollectionId == ReferendumsMuStGallen.GuidSignatureSheetsSubmitted));

        // 2 added and 1 removed should result in an increase of 1
        collectionMunicipality.PhysicalCount.Valid.Should().Be(prevCollectionMunicipality.PhysicalCount.Valid + 1);
        collectionCount.TotalCitizenCount.Should().Be(prevCount.TotalCitizenCount + 1);
        sheet.Count.Valid.Should().Be(prevSheet.Count.Valid + 1);

        // total count before 25, now 55, minus +1 new valid count should result in an increase of 29
        collectionMunicipality.PhysicalCount.Invalid.Should().Be(prevCollectionMunicipality.PhysicalCount.Invalid + 29);
        sheet.Count.Invalid.Should().Be(prevSheet.Count.Invalid + 29);

        collectionCount.ElectronicCitizenCount.Should().Be(prevCount.ElectronicCitizenCount);
    }

    [Fact]
    public async Task ShouldWorkWithoutChanges()
    {
        var sheetBefore = await RunOnDb(db => db.CollectionSignatureSheets
            .SingleAsync(x => x.Id == _sheetCtSgId));

        var req = NewValidRequest();
        req.AddedPersonRegisterIds.Clear();
        req.RemovedPersonRegisterIds.Clear();
        req.SignatureCountTotal = sheetBefore.Count.Total;
        var response = await CtSgStichprobenverwalterClient.ConfirmAsync(req);
        response.NextSignatureSheetId.Should().Be(CollectionSignatureSheets.BuildGuid(_municipalityCtSgId, 7).ToString());

        var sheet = await RunOnDb(db => db.CollectionSignatureSheets
            .SingleAsync(x => x.Id == _sheetCtSgId));

        sheet.State.Should().Be(CollectionSignatureSheetState.Confirmed);
        sheet.ModifiedBySuperiorAuthority.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldWorkWithParallelSubmit()
    {
        var prevCount = await RunOnDb(db => db.CollectionCounts
            .SingleAsync(x => x.CollectionId == ReferendumsCtStGallen.GuidSignatureSheetsSubmitted));

        var prevCollectionMunicipality = await RunOnDb(db => db.CollectionMunicipalities
            .SingleAsync(x => x.Id == _municipalityCtSgId));

        var prevSheet = await RunOnDb(db => db.CollectionSignatureSheets
            .SingleAsync(x => x.Id == _sheetCtSgId));

        var confirmCall = CtSgStichprobenverwalterClient.ConfirmAsync(NewValidRequest()).ResponseAsync;
        var req = new SubmitSignatureSheetRequest
        {
            CollectionId = ReferendumsCtStGallen.IdSignatureSheetsSubmitted,
            SignatureSheetId = CollectionSignatureSheets.BuildGuid(_municipalityCtSgId, 4).ToString(),
        };
        var submitCall = CtSgStichprobenverwalterClient.SubmitAsync(req).ResponseAsync;
        await submitCall;
        var response = await confirmCall;
        response.NextSignatureSheetId.Should().Be(CollectionSignatureSheets.BuildGuid(_municipalityCtSgId, 7).ToString());

        var sheet = await RunOnDb(db => db.CollectionSignatureSheets
            .SingleAsync(x => x.Id == _sheetCtSgId));
        sheet.State.Should().Be(CollectionSignatureSheetState.Confirmed);

        var collectionMunicipality = await RunOnDb(db => db.CollectionMunicipalities
            .SingleAsync(x => x.Id == _municipalityCtSgId));

        var collectionCount = await RunOnDb(db => db.CollectionCounts
            .SingleAsync(x => x.CollectionId == ReferendumsCtStGallen.GuidSignatureSheetsSubmitted));

        // 2 added and 1 removed during confirm and submitted 12 additional should result in an increase of 13
        collectionMunicipality.PhysicalCount.Valid.Should().Be(prevCollectionMunicipality.PhysicalCount.Valid + 13);
        collectionCount.TotalCitizenCount.Should().Be(prevCount.TotalCitizenCount + 13);

        // total count before 25, now 55, minus +1 new valid count and submitted 5 additional should result in an increase of 34
        collectionMunicipality.PhysicalCount.Invalid.Should().Be(prevCollectionMunicipality.PhysicalCount.Invalid + 34);

        // sheet count is only influenced by the confirm call
        sheet.Count.Valid.Should().Be(prevSheet.Count.Valid + 1);
        sheet.Count.Invalid.Should().Be(prevSheet.Count.Invalid + 29);

        collectionCount.ElectronicCitizenCount.Should().Be(prevCount.ElectronicCitizenCount);
    }

    [Fact]
    public async Task ShouldWorkWithInitiative()
    {
        var municipalityId = CollectionMunicipalities.BuildGuid(
            InitiativesCtStGallen.GuidUnitySignatureSheetsSubmitted,
            Bfs.MunicipalityStGallen);
        var sheetId = CollectionSignatureSheets.BuildGuid(municipalityId, 6);
        var prevCount = await RunOnDb(db => db.CollectionCounts
            .SingleAsync(x => x.CollectionId == InitiativesCtStGallen.GuidUnitySignatureSheetsSubmitted));

        var prevCollectionMunicipality = await RunOnDb(db => db.CollectionMunicipalities
            .SingleAsync(x => x.Id == municipalityId));

        var prevSheet = await RunOnDb(db => db.CollectionSignatureSheets
            .SingleAsync(x => x.Id == sheetId));

        var req = NewValidRequest();
        req.CollectionId = InitiativesCtStGallen.IdUnitySignatureSheetsSubmitted;
        req.SignatureSheetId = sheetId.ToString();
        req.CollectionType = CollectionType.Initiative;
        var response = await CtSgStichprobenverwalterClient.ConfirmAsync(req);
        response.NextSignatureSheetId.Should().Be(CollectionSignatureSheets.BuildGuid(municipalityId, 7).ToString());

        var sheet = await RunOnDb(db => db.CollectionSignatureSheets
            .SingleAsync(x => x.Id == sheetId));
        sheet.State.Should().Be(CollectionSignatureSheetState.Confirmed);

        var collectionMunicipality = await RunOnDb(db => db.CollectionMunicipalities
            .SingleAsync(x => x.Id == municipalityId));

        var collectionCount = await RunOnDb(db => db.CollectionCounts
            .SingleAsync(x => x.CollectionId == InitiativesCtStGallen.GuidUnitySignatureSheetsSubmitted));

        // 2 added and 1 removed should result in an increase of 1
        collectionMunicipality.PhysicalCount.Valid.Should().Be(prevCollectionMunicipality.PhysicalCount.Valid + 1);
        collectionCount.TotalCitizenCount.Should().Be(prevCount.TotalCitizenCount + 1);
        sheet.Count.Valid.Should().Be(prevSheet.Count.Valid + 1);

        // total count before 25, now 55, minus +1 new valid count should result in an increase of 29
        collectionMunicipality.PhysicalCount.Invalid.Should().Be(prevCollectionMunicipality.PhysicalCount.Invalid + 29);
        sheet.Count.Invalid.Should().Be(prevSheet.Count.Invalid + 29);

        collectionCount.ElectronicCitizenCount.Should().Be(prevCount.ElectronicCitizenCount);
    }

    [Fact]
    public async Task ShouldThrowAsCtOnMu()
    {
        var req = NewValidRequest();
        req.CollectionId = ReferendumsMuStGallen.IdSignatureSheetsSubmitted;
        req.SignatureSheetId = _sheetMuSgId.ToString();
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.ConfirmAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsMuOnCt()
    {
        await AssertStatus(
            async () => await MuSgStichprobenverwalterClient.ConfirmAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowNotSubmitted()
    {
        await ModifyDbEntities<CollectionSignatureSheetEntity>(
            e => e.Id == _sheetCtSgId,
            e => e.State = CollectionSignatureSheetState.Attested);
        await AssertStatus(
            async () => await MuSgStichprobenverwalterClient.ConfirmAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowSheetNotFound()
    {
        var req = NewValidRequest();
        req.SignatureSheetId = "671695d0-7761-4550-9473-f676ae44332f";
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.ConfirmAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowCollectionNotFound()
    {
        var req = NewValidRequest();
        req.CollectionId = "326ee699-1595-4fe4-a706-f656cb9c68ef";
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.ConfirmAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowOtherCollection()
    {
        var req = NewValidRequest();
        req.CollectionId = ReferendumsCtStGallen.IdPastEndedCameAbout;
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.ConfirmAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowOtherTenant()
    {
        var req = NewValidRequest();
        req.CollectionId = ReferendumsMuGoldach.IdSignatureSheetsSubmitted;
        req.SignatureSheetId = CollectionSignatureSheets.BuildGuid(
            CollectionMunicipalities.BuildGuid(
                ReferendumsMuGoldach.GuidSignatureSheetsSubmitted,
                Bfs.MunicipalityGoldach),
            7).ToString();
        await AssertStatus(
            async () => await MuSgStichprobenverwalterClient.ConfirmAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowSamePersonRegisterIdInAddAndRemove()
    {
        var req = NewValidRequest();
        req.AddedPersonRegisterIds.Add(VotingStimmregisterAdapterMock.VotingRightPerson8.RegisterId.ToString());
        req.RemovedPersonRegisterIds.Add(VotingStimmregisterAdapterMock.VotingRightPerson8.RegisterId.ToString());
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.ConfirmAsync(req),
            StatusCode.InvalidArgument,
            "Cannot add and remove the same person register id");
    }

    [Fact]
    public async Task ShouldThrowTotalCountIsLowerThanValid()
    {
        var req = NewValidRequest();
        req.SignatureCountTotal = 20;
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.ConfirmAsync(req),
            StatusCode.InvalidArgument,
            "Cannot set total count lower than valid count");
    }

    [Fact]
    public async Task ShouldThrowRemoveUnknownStimmregisterId()
    {
        var req = NewValidRequest();
        req.RemovedPersonRegisterIds.Add("ba7c83b5-3e12-467f-8b4f-bac5133c0ff8");
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.ConfirmAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAddUnknownStimmregisterId()
    {
        var req = NewValidRequest();
        req.AddedPersonRegisterIds.Add("ba7c83b5-3e12-467f-8b4f-bac5133c0ff8");
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.ConfirmAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAddCitizenWithoutRightToVote()
    {
        var req = NewValidRequest();
        req.AddedPersonRegisterIds.Add(VotingStimmregisterAdapterMock.NoVotingRightPerson1.RegisterId.ToString());
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.ConfirmAsync(req),
            StatusCode.InvalidArgument,
            "One person does not have the right to vote");
    }

    [Fact]
    public async Task ShouldThrowCollectionTypeMismatch()
    {
        var req = NewValidRequest();
        req.CollectionType = CollectionType.Initiative;
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.ConfirmAsync(req),
            StatusCode.NotFound);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInState(CollectionState state)
    {
        await ModifyDbEntities<ReferendumEntity>(
            e => e.Id == ReferendumsCtStGallen.GuidSignatureSheetsSubmitted,
            e => e.State = state);

        if (!state.IsEnded())
        {
            await AssertStatus(
                async () => await CtSgStichprobenverwalterClient.ConfirmAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
        else
        {
            await CtSgStichprobenverwalterClient.ConfirmAsync(NewValidRequest());
        }
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionSignatureSheetService.CollectionSignatureSheetServiceClient(channel)
            .ConfirmAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stichprobenverwalter;
    }

    private static ConfirmSignatureSheetRequest NewValidRequest()
    {
        return new ConfirmSignatureSheetRequest
        {
            CollectionId = ReferendumsCtStGallen.IdSignatureSheetsSubmitted,
            SignatureSheetId = _sheetCtSgId.ToString(),
            CollectionType = CollectionType.Referendum,
            AddedPersonRegisterIds =
            {
                VotingStimmregisterAdapterMock.VotingRightPerson8.RegisterId.ToString(),
                VotingStimmregisterAdapterMock.VotingRightPerson10.RegisterId.ToString(),
            },
            RemovedPersonRegisterIds = { CollectionCitizens.RegisterIdSgSheet6.ToString() },
            SignatureCountTotal = 55,
        };
    }
}
