// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.Admin.WebService.Integration.Tests.Mocks;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionUpdateSignatureSheetTest : BaseGrpcTest<CollectionSignatureSheetService.CollectionSignatureSheetServiceClient>
{
    private static readonly Guid _sheet1Id = CollectionSignatureSheets.BuildGuid(
        CollectionMunicipalities.BuildGuid(
            ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
            Bfs.MunicipalityStGallen),
        1);

    private static readonly Guid _sheet3Id = CollectionSignatureSheets.BuildGuid(
        CollectionMunicipalities.BuildGuid(
            ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
            Bfs.MunicipalityStGallen),
        3);

    public CollectionUpdateSignatureSheetTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Referendums.WithReferendums(ReferendumsCtStGallen.GuidInCollectionEnabledForCollection, ReferendumsCtStGallen.GuidInCollectionEnabledForCollection2) with
            {
                SeedReferendumSignatureSheets = true,
            });
    }

    [Fact]
    public async Task ShouldWork()
    {
        await MuSgKontrollzeichenerfasserClient.UpdateAsync(NewValidRequest());
        var updated = await RunOnDb(db => db.CollectionSignatureSheets.FirstAsync(x => x.Id == _sheet1Id));
        await Verify(updated);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await MuSgKontrollzeichenerfasserClient.UpdateAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldThrowMinCount()
    {
        var req = NewValidRequest(x =>
        {
            x.SignatureSheetId = _sheet3Id.ToString();
            x.SignatureCountTotal = 10;
        });
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.UpdateAsync(req),
            StatusCode.InvalidArgument,
            $"{nameof(ValidationException)}: Cannot set total count lower than valid count");
    }

    [Fact]
    public async Task ShouldThrowOtherTenant()
    {
        await AssertStatus(
            async () => await MuGoldachKontrollzeichenerfasserClient.UpdateAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowNotFound()
    {
        var req = NewValidRequest(x => x.SignatureSheetId = "8ffc7359-36fd-464a-ae13-13a91075ec22");
        await AssertStatus(
            async () => await MuGoldachKontrollzeichenerfasserClient.UpdateAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowCollectionIdNotFound()
    {
        var req = NewValidRequest(x => x.CollectionId = "8ffc7359-36fd-464a-ae13-13a91075ec22");
        await AssertStatus(
            async () => await MuGoldachKontrollzeichenerfasserClient.UpdateAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowOtherCollectionId()
    {
        var req = NewValidRequest(x => x.CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection2);
        await AssertStatus(
            async () => await MuGoldachKontrollzeichenerfasserClient.UpdateAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsCt()
    {
        await AssertStatus(
            async () => await CtSgKontrollzeichenerfasserClient.UpdateAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task NotPublishedShouldThrow()
    {
        await ModifyDbEntities(
            (ReferendumEntity e) => e.Id == ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
            e => e.CollectionStartDate = MockedClock.NowDateOnly.AddDays(1));
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.UpdateAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task MunicipalityLockedShouldThrow()
    {
        await ModifyDbEntities<CollectionMunicipalityEntity>(
            x => x.CollectionId == ReferendumsCtStGallen.GuidInCollectionEnabledForCollection && x.Bfs == Bfs.MunicipalityStGallen,
            x => x.IsLocked = true);
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.UpdateAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ReceivedAtInFutureShouldThrow()
    {
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.UpdateAsync(NewValidRequest(x => x.ReceivedAt = MockedClock.GetDate(1).ToProtoDate())),
            StatusCode.InvalidArgument,
            "Received at date can't be in the future.");
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionSignatureSheetService.CollectionSignatureSheetServiceClient(channel)
            .UpdateAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Kontrollzeichenerfasser;
    }

    private static UpdateSignatureSheetRequest NewValidRequest(Action<UpdateSignatureSheetRequest>? customizer = null)
    {
        var req = new UpdateSignatureSheetRequest
        {
            CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection,
            SignatureSheetId = _sheet1Id.ToString(),
            ReceivedAt = MockedClock.GetDate(-1).ToProtoDate(),
            SignatureCountTotal = 20,
        };
        customizer?.Invoke(req);
        return req;
    }
}
