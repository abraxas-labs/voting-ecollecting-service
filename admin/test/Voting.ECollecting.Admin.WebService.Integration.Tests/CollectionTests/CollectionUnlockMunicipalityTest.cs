// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
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

public class CollectionUnlockMunicipalityTest : BaseGrpcTest<CollectionMunicipalityService.CollectionMunicipalityServiceClient>
{
    private static readonly Guid _municipalityCtSgId = CollectionMunicipalities.BuildGuid(ReferendumsCtStGallen.GuidSignatureSheetsSubmitted, Bfs.MunicipalityStGallen);
    private static readonly Guid _municipalityMuSgId = CollectionMunicipalities.BuildGuid(ReferendumsMuStGallen.GuidSignatureSheetsSubmitted, Bfs.MunicipalityStGallen);

    public CollectionUnlockMunicipalityTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Referendums.WithReferendums(ReferendumsCtStGallen.GuidSignatureSheetsSubmitted, ReferendumsMuStGallen.GuidSignatureSheetsSubmitted));
    }

    [Fact]
    public async Task ShouldWork()
    {
        await CtSgStichprobenverwalterClient.UnlockAsync(NewValidRequest());
        var updated = await RunOnDb(db => db.CollectionMunicipalities.FirstAsync(x => x.Id == _municipalityCtSgId));
        await Verify(updated);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStichprobenverwalterClient.UnlockAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldThrowAlreadyUnlocked()
    {
        await ModifyDbEntities<CollectionMunicipalityEntity>(
            x => x.Id == _municipalityCtSgId,
            x => x.IsLocked = false);
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.UnlockAsync(NewValidRequest()),
            StatusCode.InvalidArgument,
            $"{nameof(ValidationException)}: Collection municipality is already in state locked: {bool.FalseString}.");
    }

    [Fact]
    public async Task ShouldWorkAsMu()
    {
        var req = NewValidRequest(x =>
        {
            x.CollectionId = ReferendumsMuStGallen.IdSignatureSheetsSubmitted;
            x.Bfs = Bfs.MunicipalityStGallen;
        });
        await MuSgStichprobenverwalterClient.UnlockAsync(req);
        var updated = await RunOnDb(db => db.CollectionMunicipalities.FirstAsync(x => x.Id == _municipalityMuSgId));
        await Verify(updated);
    }

    [Fact]
    public async Task ShouldThrowAsMuOnCt()
    {
        await AssertStatus(
            async () => await MuSgStichprobenverwalterClient.UnlockAsync(NewValidRequest()),
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
            async () => await CtSgStichprobenverwalterClient.UnlockAsync(req),
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
            async () => await MuGoldachKontrollzeichenerfasserClient.UnlockAsync(req),
            StatusCode.PermissionDenied);
    }

    [Fact]
    public async Task ShouldThrowNotFound()
    {
        var req = NewValidRequest(x => x.Bfs = "9999");
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.UnlockAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowCollectionIdNotFound()
    {
        var req = NewValidRequest(x => x.CollectionId = "15ba27d2-ac77-4b8e-a45e-078893b0c691");
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.UnlockAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowOtherCollectionId()
    {
        var req = NewValidRequest(x => x.CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection);
        await AssertStatus(
            async () => await CtSgStichprobenverwalterClient.UnlockAsync(req),
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
            await CtSgStichprobenverwalterClient.UnlockAsync(NewValidRequest());
        }
        else
        {
            await AssertStatus(
                async () => await CtSgStichprobenverwalterClient.UnlockAsync(NewValidRequest()),
                StatusCode.NotFound);
        }
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionMunicipalityService.CollectionMunicipalityServiceClient(channel)
            .UnlockAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stichprobenverwalter;
    }

    private static UnlockCollectionMunicipalityRequest NewValidRequest(Action<UnlockCollectionMunicipalityRequest>? customizer = null)
    {
        var req = new UnlockCollectionMunicipalityRequest
        {
            CollectionId = ReferendumsCtStGallen.IdSignatureSheetsSubmitted,
            Bfs = Bfs.MunicipalityStGallen,
        };
        customizer?.Invoke(req);
        return req;
    }
}
