// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net.Http.Json;
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
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionSubmitMunicipalitySignatureSheetsTest : BaseGrpcTest<CollectionMunicipalityService.CollectionMunicipalityServiceClient>
{
    private static readonly Guid _municipalityCtSgId = CollectionMunicipalities.BuildGuid(ReferendumsCtStGallen.GuidSignatureSheetsSubmitted, Bfs.MunicipalityStGallen);
    private static readonly Guid _municipalityMuSgId = CollectionMunicipalities.BuildGuid(ReferendumsMuStGallen.GuidSignatureSheetsSubmitted, Bfs.MunicipalityStGallen);

    public CollectionSubmitMunicipalitySignatureSheetsTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Referendums.WithReferendums(ReferendumsCtStGallen.GuidSignatureSheetsSubmitted, ReferendumsMuStGallen.GuidSignatureSheetsSubmitted) with
        {
            SeedReferendumSignatureSheets = true,
            SeedDomainOfInfluences = true,
        });
    }

    [Fact]
    public async Task ShouldWork()
    {
        var prevCount = await RunOnDb(db => db.CollectionCounts
            .SingleAsync(x => x.CollectionId == ReferendumsCtStGallen.GuidSignatureSheetsSubmitted));

        var prevCollectionMunicipality = await RunOnDb(db => db.CollectionMunicipalities
            .SingleAsync(x => x.Id == _municipalityCtSgId));

        var validSignatureCount = await RunOnDb(db => db.CollectionSignatureSheets
            .Where(x => x.CollectionMunicipalityId == _municipalityCtSgId && x.State == CollectionSignatureSheetState.Attested)
            .SumAsync(x => x.Count.Valid));

        var invalidSignatureCount = await RunOnDb(db => db.CollectionSignatureSheets
            .Where(x => x.CollectionMunicipalityId == _municipalityCtSgId && x.State == CollectionSignatureSheetState.Attested)
            .SumAsync(x => x.Count.Invalid));

        var response = await CtSgStichprobenverwalterClient.SubmitSignatureSheetsAsync(NewValidRequest());
        await Verify(response);

        response.Municipality.PhysicalCount.Valid.Should().Be(prevCollectionMunicipality.PhysicalCount.Valid + validSignatureCount);
        response.Municipality.PhysicalCount.Invalid.Should().Be(prevCollectionMunicipality.PhysicalCount.Invalid + invalidSignatureCount);
        response.CollectionCount.ElectronicCitizenCount.Should().Be(prevCount.ElectronicCitizenCount);
        response.CollectionCount.TotalCitizenCount.Should().Be(prevCount.TotalCitizenCount + validSignatureCount);
    }

    [Fact]
    public async Task ShouldWorkAsMu()
    {
        var prevCount = await RunOnDb(db => db.CollectionCounts
            .SingleAsync(x => x.CollectionId == ReferendumsMuStGallen.GuidSignatureSheetsSubmitted));

        var prevCollectionMunicipality = await RunOnDb(db => db.CollectionMunicipalities
            .SingleAsync(x => x.Id == _municipalityMuSgId));

        var validSignatureCount = await RunOnDb(db => db.CollectionSignatureSheets
            .Where(x => x.CollectionMunicipalityId == _municipalityMuSgId && x.State == CollectionSignatureSheetState.Attested)
            .SumAsync(x => x.Count.Valid));

        var invalidSignatureCount = await RunOnDb(db => db.CollectionSignatureSheets
            .Where(x => x.CollectionMunicipalityId == _municipalityMuSgId && x.State == CollectionSignatureSheetState.Attested)
            .SumAsync(x => x.Count.Invalid));

        var req = NewValidRequest(x =>
        {
            x.CollectionId = ReferendumsMuStGallen.IdSignatureSheetsSubmitted;
            x.Bfs = Bfs.MunicipalityStGallen;
        });
        var response = await MuSgStichprobenverwalterClient.SubmitSignatureSheetsAsync(req);
        await Verify(response);

        response.Municipality.PhysicalCount.Valid.Should().Be(prevCollectionMunicipality.PhysicalCount.Valid + validSignatureCount);
        response.Municipality.PhysicalCount.Invalid.Should().Be(prevCollectionMunicipality.PhysicalCount.Invalid + invalidSignatureCount);
        response.CollectionCount.ElectronicCitizenCount.Should().Be(prevCount.ElectronicCitizenCount);
        response.CollectionCount.TotalCitizenCount.Should().Be(prevCount.TotalCitizenCount + validSignatureCount);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            var req = NewValidRequest(x =>
            {
                x.CollectionId = ReferendumsMuStGallen.IdSignatureSheetsSubmitted;
                x.Bfs = Bfs.MunicipalityStGallen;
            });

            await MuSgStichprobenverwalterClient.SubmitSignatureSheetsAsync(req);
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldWorkWithParallelAttest()
    {
        var prevCount = await RunOnDb(db => db.CollectionCounts
            .SingleAsync(x => x.CollectionId == ReferendumsCtStGallen.GuidSignatureSheetsSubmitted));

        var prevCollectionMunicipality = await RunOnDb(db => db.CollectionMunicipalities
            .SingleAsync(x => x.Id == _municipalityCtSgId));

        var validSignatureCount = await RunOnDb(db => db.CollectionSignatureSheets
            .Where(x => x.CollectionMunicipalityId == _municipalityCtSgId && x.State == CollectionSignatureSheetState.Attested)
            .SumAsync(x => x.Count.Valid));

        var invalidSignatureCount = await RunOnDb(db => db.CollectionSignatureSheets
            .Where(x => x.CollectionMunicipalityId == _municipalityCtSgId && x.State == CollectionSignatureSheetState.Attested)
            .SumAsync(x => x.Count.Invalid));

        var submitCall = CtSgStichprobenverwalterClient.SubmitSignatureSheetsAsync(NewValidRequest()).ResponseAsync;

        await ModifyDbEntities<CollectionMunicipalityEntity>(
            x => x.CollectionId == ReferendumsCtStGallen.GuidSignatureSheetsSubmitted && x.Bfs == Bfs.MunicipalityStGallen,
            x => x.IsLocked = false);

        HashSet<Guid> sheetIds =
        [
            CollectionSignatureSheets.BuildGuid(
                CollectionMunicipalities.BuildGuid(
                    ReferendumsCtStGallen.GuidSignatureSheetsSubmitted,
                    Bfs.MunicipalityStGallen),
                1),
            CollectionSignatureSheets.BuildGuid(
                CollectionMunicipalities.BuildGuid(
                    ReferendumsCtStGallen.GuidSignatureSheetsSubmitted,
                    Bfs.MunicipalityStGallen),
                2),
        ];
        var muSgKontrollzeichenerstellerClient = CreateHttpClient(tenant: MockedTenantIds.MUSG, roles: Roles.Kontrollzeichenerfasser);
        var resp = await muSgKontrollzeichenerstellerClient.PostAsJsonAsync($"v1/api/collections/{ReferendumsCtStGallen.IdSignatureSheetsSubmitted}/signature-sheets/attest", sheetIds);
        resp.EnsureSuccessStatusCode();

        var response = await submitCall;
        response.Municipality.PhysicalCount.Valid.Should().Be(prevCollectionMunicipality.PhysicalCount.Valid + validSignatureCount);
        response.Municipality.PhysicalCount.Invalid.Should().Be(prevCollectionMunicipality.PhysicalCount.Invalid + invalidSignatureCount);
        response.CollectionCount.ElectronicCitizenCount.Should().Be(prevCount.ElectronicCitizenCount);
        response.CollectionCount.TotalCitizenCount.Should().Be(prevCount.TotalCitizenCount + validSignatureCount);
    }

    [Fact]
    public async Task ShouldWorkWithoutSheetsInAttestedState()
    {
        await ModifyDbEntities<CollectionSignatureSheetEntity>(
            x => x.CollectionMunicipalityId == _municipalityCtSgId,
            x => x.State = CollectionSignatureSheetState.Submitted);

        var prevCount = await RunOnDb(db => db.CollectionCounts
            .SingleAsync(x => x.CollectionId == ReferendumsCtStGallen.GuidSignatureSheetsSubmitted));

        var prevCollectionMunicipality = await RunOnDb(db => db.CollectionMunicipalities
            .SingleAsync(x => x.Id == _municipalityCtSgId));

        var response = await CtSgStichprobenverwalterClient.SubmitSignatureSheetsAsync(NewValidRequest());
        await Verify(response);

        response.Municipality.PhysicalCount.Valid.Should().Be(prevCollectionMunicipality.PhysicalCount.Valid);
        response.Municipality.PhysicalCount.Invalid.Should().Be(prevCollectionMunicipality.PhysicalCount.Invalid);
        response.CollectionCount.ElectronicCitizenCount.Should().Be(prevCount.ElectronicCitizenCount);
        response.CollectionCount.TotalCitizenCount.Should().Be(prevCount.TotalCitizenCount);
    }

    [Fact]
    public async Task ShouldThrowAsMuOnCt()
    {
        await AssertStatus(
            async () => await MuSgStichprobenverwalterClient.SubmitSignatureSheetsAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsCtOnMu()
    {
        var req = NewValidRequest(x =>
        {
            x.CollectionId = ReferendumsMuStGallen.IdSignatureSheetsSubmitted;
            x.Bfs = Bfs.MunicipalityStGallen;
        });
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.SubmitSignatureSheetsAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowOtherMuTenant()
    {
        var req = NewValidRequest(x =>
        {
            x.CollectionId = ReferendumsMuStGallen.IdSignatureSheetsSubmitted;
            x.Bfs = Bfs.MunicipalityStGallen;
        });
        await AssertStatus(
            async () => await MuGoldachKontrollzeichenerfasserClient.SubmitSignatureSheetsAsync(req),
            StatusCode.PermissionDenied);
    }

    [Fact]
    public async Task ShouldThrowNotFound()
    {
        var req = NewValidRequest(x => x.Bfs = "9999");
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.SubmitSignatureSheetsAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowCollectionIdNotFound()
    {
        var req = NewValidRequest(x => x.CollectionId = "0fe7c8d7-5052-45d3-9982-f8678b1e0c4a");
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.SubmitSignatureSheetsAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowOtherCollectionId()
    {
        var req = NewValidRequest(x => x.CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection);
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.SubmitSignatureSheetsAsync(req),
            StatusCode.NotFound);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task WorksInState(CollectionState state)
    {
        await ModifyDbEntities<ReferendumEntity>(
            e => e.Id == ReferendumsCtStGallen.GuidSignatureSheetsSubmitted,
            e => e.State = state);

        if (state.IsEnded())
        {
            await CtSgStichprobenverwalterClient.SubmitSignatureSheetsAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await CtSgStichprobenverwalterClient.SubmitSignatureSheetsAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionMunicipalityService.CollectionMunicipalityServiceClient(channel)
            .SubmitSignatureSheetsAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stichprobenverwalter;
    }

    private static SubmitCollectionMunicipalitySignatureSheetsRequest NewValidRequest(Action<SubmitCollectionMunicipalitySignatureSheetsRequest>? customizer = null)
    {
        var req = new SubmitCollectionMunicipalitySignatureSheetsRequest
        {
            CollectionId = ReferendumsCtStGallen.IdSignatureSheetsSubmitted,
            Bfs = Bfs.MunicipalityStGallen,
        };
        customizer?.Invoke(req);
        return req;
    }
}
