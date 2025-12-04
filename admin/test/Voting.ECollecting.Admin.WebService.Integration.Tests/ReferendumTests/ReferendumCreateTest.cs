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
using Voting.ECollecting.Shared.Test.MockedData;
using CollectionAddress = Voting.ECollecting.Proto.Admin.Services.V1.Models.CollectionAddress;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.ReferendumTests;

public class ReferendumCreateTest : BaseGrpcTest<ReferendumService.ReferendumServiceClient>
{
    public ReferendumCreateTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Referendums.WithReferendums(ReferendumsCtStGallen.GuidSignatureSheetsSubmitted));
    }

    [Fact]
    public async Task ShouldCreateReferendum()
    {
        var response = await CtSgStammdatenverwalterClient.CreateAsync(NewValidRequest());
        var referendum = await RunOnDb(db => db.Referendums.IgnoreQueryFilters().Include(x => x.Municipalities!.OrderBy(y => y.Bfs)).FirstAsync(x => x.Id == Guid.Parse(response.Id)));
        referendum.SetPeriodState(GetService<TimeProvider>().GetUtcNowDateTime());
        await Verify(referendum)
            .IgnoreMember<ReferendumEntity>(x => x.Number)
            .IgnoreMembers<CollectionBaseEntity>(x => x.MacKeyId, x => x.EncryptionKeyId);
        referendum.MacKeyId.Should().NotBeNullOrEmpty();
        referendum.EncryptionKeyId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStammdatenverwalterClient.CreateAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries())
                .ScrubMember("Number")
                .ScrubMember("EncryptionKeyId")
                .ScrubMember("MacKeyId");
        });
    }

    [Fact]
    public async Task AsMuOnOwnCollectionShouldWork()
    {
        var response = await MuSgStammdatenverwalterClient.CreateAsync(NewValidRequest(x => x.DecreeId = DecreesMuStGallen.IdInCollectionWithReferendum));
        var referendum = await RunOnDb(db => db.Referendums.IgnoreQueryFilters().Include(x => x.Municipalities!.OrderBy(y => y.Bfs)).FirstAsync(x => x.Id == Guid.Parse(response.Id)));
        referendum.SetPeriodState(GetService<TimeProvider>().GetUtcNowDateTime());
        await Verify(referendum)
            .IgnoreMember<ReferendumEntity>(x => x.Number)
            .IgnoreMembers<CollectionBaseEntity>(x => x.MacKeyId, x => x.EncryptionKeyId);
        referendum.MacKeyId.Should().NotBeNullOrEmpty();
        referendum.EncryptionKeyId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AsMuOnCtCollectionShouldWork()
    {
        var response = await MuSgStammdatenverwalterClient.CreateAsync(NewValidRequest());
        var referendum = await RunOnDb(db => db.Referendums.IgnoreQueryFilters().Include(x => x.Municipalities!.OrderBy(y => y.Bfs)).FirstAsync(x => x.Id == Guid.Parse(response.Id)));
        referendum.SetPeriodState(GetService<TimeProvider>().GetUtcNowDateTime());
        await Verify(referendum)
            .IgnoreMember<ReferendumEntity>(x => x.Number)
            .IgnoreMembers<CollectionBaseEntity>(x => x.MacKeyId, x => x.EncryptionKeyId);
        referendum.MacKeyId.Should().NotBeNullOrEmpty();
        referendum.EncryptionKeyId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AsMuOnOtherMuCollectionShouldFail()
    {
        var req = NewValidRequest(x => x.DecreeId = DecreesMuStGallen.IdInCollectionWithReferendum);
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.CreateAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsCtOnMuCollectionShouldWork()
    {
        var req = NewValidRequest(x => x.DecreeId = DecreesMuStGallen.IdInCollectionWithReferendum);
        var response = await CtSgStammdatenverwalterClient.CreateAsync(req);
        var referendum = await RunOnDb(db => db.Referendums.IgnoreQueryFilters().Include(x => x.Municipalities!.OrderBy(y => y.Bfs)).FirstAsync(x => x.Id == Guid.Parse(response.Id)));
        referendum.SetPeriodState(GetService<TimeProvider>().GetUtcNowDateTime());
        await Verify(referendum)
            .IgnoreMember<ReferendumEntity>(x => x.Number)
            .IgnoreMembers<CollectionBaseEntity>(x => x.MacKeyId, x => x.EncryptionKeyId);
        referendum.MacKeyId.Should().NotBeNullOrEmpty();
        referendum.EncryptionKeyId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AsKontrollzeichenerfasserShouldWork()
    {
        var response = await CtSgKontrollzeichenerfasserClient.CreateAsync(NewValidRequest());
        var referendum = await RunOnDb(db => db.Referendums.IgnoreQueryFilters().Include(x => x.Municipalities!.OrderBy(y => y.Bfs)).FirstAsync(x => x.Id == Guid.Parse(response.Id)));
        referendum.SetPeriodState(GetService<TimeProvider>().GetUtcNowDateTime());
        await Verify(referendum)
            .IgnoreMember<ReferendumEntity>(x => x.Number)
            .IgnoreMembers<CollectionBaseEntity>(x => x.MacKeyId, x => x.EncryptionKeyId);
        referendum.MacKeyId.Should().NotBeNullOrEmpty();
        referendum.EncryptionKeyId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PeriodStateNotInCollectionShouldFail()
    {
        var req = NewValidRequest(x => x.DecreeId = DecreesCtStGallen.IdFutureNoReferendum);
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.CreateAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task DuplicateDescriptionShouldFail()
    {
        var req = NewValidRequest(x => x.Description = "Referendum über den Kantonsratsbeschluss über die Genehmigung des Regierungsbeschlusses über den Beitritt zur Interkantonalen Vereinbarung über die Steuerförderung");
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.CreateAsync(req),
            StatusCode.FailedPrecondition);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.CreateAsync(NewValidRequest(x => x.DecreeId = "93dde01e-eb88-48f6-88ba-e4fa7b30152f")),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new ReferendumService.ReferendumServiceClient(channel).CreateAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
        yield return Roles.Kontrollzeichenerfasser;
    }

    private CreateReferendumRequest NewValidRequest(Action<CreateReferendumRequest>? customizer = null)
    {
        var request = new CreateReferendumRequest
        {
            DecreeId = DecreesCtStGallen.IdInCollectionWithReferendum,
            Description = "Sammlung gegen das Abwassergesetz",
            Address = new CollectionAddress
            {
                CommitteeOrPerson = "Abwasser Komitee",
                StreetOrPostOfficeBox = "Otmarstrasse",
                ZipCode = "9000",
                Locality = "St.Gallen",
            },
        };

        customizer?.Invoke(request);
        return request;
    }
}
