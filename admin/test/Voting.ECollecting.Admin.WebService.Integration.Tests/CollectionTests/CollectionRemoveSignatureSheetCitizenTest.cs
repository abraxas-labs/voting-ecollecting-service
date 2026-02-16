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
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionRemoveSignatureSheetCitizenTest : BaseGrpcTest<CollectionSignatureSheetService.CollectionSignatureSheetServiceClient>
{
    private static readonly Guid _initiativeSgSheet1Guid = CollectionSignatureSheets.BuildGuid(
        CollectionMunicipalities.BuildGuid(
            InitiativesCh.GuidEnabledForCollectionCollecting,
            Bfs.MunicipalityStGallen),
        1);

    private static readonly Guid _referendumSgSheet1Guid = CollectionSignatureSheets.BuildGuid(
        CollectionMunicipalities.BuildGuid(
            ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
            Bfs.MunicipalityStGallen),
        1);

    public CollectionRemoveSignatureSheetCitizenTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Default
                    .WithDecrees(DecreesCtStGallen.GuidInCollectionWithReferendum)
                    .WithInitiatives(InitiativesCh.GuidEnabledForCollectionCollecting));
    }

    [Fact]
    public async Task ShouldWork()
    {
        var req = NewValidInitiativeRequest();
        await MuSgKontrollzeichenerfasserClient.RemoveCitizenAsync(req);
        var sheet = await RunOnDb(db => db.CollectionSignatureSheets
            .Include(x => x.Citizens.OrderBy(y => y.Log!.VotingStimmregisterIdMac)).ThenInclude(x => x.Log)
            .FirstAsync(x => x.Id == Guid.Parse(req.SignatureSheetId)));

        sheet.Count.Valid.Should().Be(0);
        sheet.Count.Total.Should().Be(20);
        await Verify(sheet);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await MuSgKontrollzeichenerfasserClient.RemoveCitizenAsync(NewValidInitiativeRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldWorkReferendum()
    {
        var req = NewValidReferendumRequest();
        await MuSgKontrollzeichenerfasserClient.RemoveCitizenAsync(req);
        var sheet = await RunOnDb(db => db.CollectionSignatureSheets
            .Include(x => x.Citizens.OrderBy(y => y.Log!.VotingStimmregisterIdMac)).ThenInclude(x => x.Log)
            .FirstAsync(x => x.Id == Guid.Parse(req.SignatureSheetId)));

        sheet.Count.Valid.Should().Be(0);
        sheet.Count.Total.Should().Be(20);
        await Verify(sheet);
    }

    [Fact]
    public async Task ShouldThrowAsOtherMu()
    {
        await AssertStatus(
            async () => await MuGoldachKontrollzeichenerfasserClient.RemoveCitizenAsync(NewValidInitiativeRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsCt()
    {
        await AssertStatus(
            async () => await CtSgKontrollzeichenerfasserClient.RemoveCitizenAsync(NewValidInitiativeRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowCollectionNotFound()
    {
        var req = NewValidInitiativeRequest(x => x.CollectionId = "70743aef-fd76-4a95-9dde-d033c3744001");
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.RemoveCitizenAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowSheetNotFound()
    {
        var req = NewValidInitiativeRequest(x => x.SignatureSheetId = "5a287c8e-db8a-4715-b971-2ed6fa4daae2");
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.RemoveCitizenAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowPersonNotFound()
    {
        var req = NewValidInitiativeRequest(x => x.PersonRegisterId = "d78d95dd-99e6-4aa9-8b69-16aa2fd27c89");
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.RemoveCitizenAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowCollectionPublishedPeriodState()
    {
        var req = NewValidInitiativeRequest();
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == Guid.Parse(req.CollectionId),
            x => x.CollectionStartDate = MockedClock.NowDateOnly.AddDays(1));
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.RemoveCitizenAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowSheetAttested()
    {
        var req = NewValidInitiativeRequest();
        await ModifyDbEntities<CollectionSignatureSheetEntity>(
            x => x.Id == Guid.Parse(req.SignatureSheetId),
            x => x.State = CollectionSignatureSheetState.Attested);
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.RemoveCitizenAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task MunicipalityLockedShouldThrow()
    {
        await ModifyDbEntities<CollectionMunicipalityEntity>(
            x => x.CollectionId == ReferendumsCtStGallen.GuidInCollectionEnabledForCollection && x.Bfs == Bfs.MunicipalityStGallen,
            x => x.IsLocked = true);
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.RemoveCitizenAsync(NewValidReferendumRequest()),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionSignatureSheetService.CollectionSignatureSheetServiceClient(channel)
            .RemoveCitizenAsync(NewValidInitiativeRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles() => [Roles.Kontrollzeichenerfasser];

    private RemoveSignatureSheetCitizenRequest NewValidInitiativeRequest(Action<RemoveSignatureSheetCitizenRequest>? customizer = null)
    {
        var request = new RemoveSignatureSheetCitizenRequest
        {
            CollectionId = InitiativesCh.IdEnabledForCollectionCollecting,
            SignatureSheetId = _initiativeSgSheet1Guid.ToString(),
            PersonRegisterId = CollectionCitizens.RegisterIdSgSheet1.ToString(),
        };

        customizer?.Invoke(request);
        return request;
    }

    private RemoveSignatureSheetCitizenRequest NewValidReferendumRequest(Action<RemoveSignatureSheetCitizenRequest>? customizer = null)
    {
        var request = new RemoveSignatureSheetCitizenRequest
        {
            CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection,
            SignatureSheetId = _referendumSgSheet1Guid.ToString(),
            PersonRegisterId = CollectionCitizens.RegisterIdCollection2SgSheet1.ToString(),
        };

        customizer?.Invoke(request);
        return request;
    }
}
