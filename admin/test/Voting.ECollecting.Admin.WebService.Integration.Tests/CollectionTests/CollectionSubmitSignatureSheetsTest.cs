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
using Voting.ECollecting.Shared.Test.Models;
using Voting.Lib.Common;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionSubmitSignatureSheetsTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionSubmitSignatureSheetsTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Referendums.WithReferendums(ReferendumsCtStGallen.GuidInCollectionEnabledForCollection, ReferendumsMuStGallen.GuidInCollectionActive) with
            {
                SeedReferendumSignatureSheets = true,
                SeedReferendumCitizens = true,
            });
    }

    [Fact]
    public async Task ShouldWork()
    {
        var request = NewValidRequest();
        var sheetsCreatedIds = await RunOnDb(db => db.CollectionSignatureSheets
            .Where(x => x.CollectionMunicipality!.CollectionId == GuidParser.Parse(request.CollectionId) && x.State == CollectionSignatureSheetState.Created)
            .Select(x => x.Id)
            .ToListAsync());
        sheetsCreatedIds.Should().NotBeEmpty();

        var hasCitizens = await RunOnDb(db => db.CollectionCitizens
            .Where(x => x.SignatureSheetId.HasValue && sheetsCreatedIds.Contains(x.SignatureSheetId.Value))
            .AnyAsync());
        hasCitizens.Should().BeTrue();

        var hasCitizenLogs = await RunOnDb(db => db.CollectionCitizenLogs
            .Where(x => x.CollectionCitizen!.SignatureSheetId.HasValue && sheetsCreatedIds.Contains(x.CollectionCitizen!.SignatureSheetId.Value))
            .AnyAsync());
        hasCitizenLogs.Should().BeTrue();

        var response = await CtSgStammdatenverwalterClient.SubmitSignatureSheetsAsync(request);

        var referendum = await RunOnDb(db => db.Referendums
            .Include(x => x.Municipalities!)
            .ThenInclude(x => x.SignatureSheets)
            .FirstAsync(x => x.Id == ReferendumsCtStGallen.GuidInCollectionEnabledForCollection));
        referendum.State.Should().Be(CollectionState.SignatureSheetsSubmitted);
        referendum.Municipalities!.Should().AllSatisfy(x => x.IsLocked.Should().BeTrue());
        referendum.Municipalities!.SelectMany(x => x.SignatureSheets!).Should().AllSatisfy(x =>
            x.State.Should().NotBe(CollectionSignatureSheetState.Created));

        var hasSheetsInCreatedState = await RunOnDb(db => db.CollectionSignatureSheets
            .Where(x => x.CollectionMunicipality!.CollectionId == GuidParser.Parse(request.CollectionId) && x.State == CollectionSignatureSheetState.Created)
            .AnyAsync());
        hasSheetsInCreatedState.Should().BeFalse();

        var hasCitizensAfterSubmit = await RunOnDb(db => db.CollectionCitizens
            .Where(x => x.SignatureSheetId.HasValue && sheetsCreatedIds.Contains(x.SignatureSheetId.Value))
            .AnyAsync());
        hasCitizensAfterSubmit.Should().BeFalse();

        var hasCitizenLogsAfterSubmit = await RunOnDb(db => db.CollectionCitizenLogs
            .Where(x => x.CollectionCitizen!.SignatureSheetId.HasValue && sheetsCreatedIds.Contains(x.CollectionCitizen!.SignatureSheetId.Value))
            .AnyAsync());
        hasCitizenLogsAfterSubmit.Should().BeFalse();

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == ReferendumsCtStGallen.GuidInCollectionEnabledForCollection)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == ReferendumsCtStGallen.GuidInCollectionEnabledForCollection));
        await Verify(new { userNotifications, collectionMessage, response });
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStammdatenverwalterClient.SubmitSignatureSheetsAsync(NewValidRequest());

            var result = await GetAuditTrailEntries();
            result.AuditTrailEntries.Count(e => e.SourceEntityName == "CollectionSignatureSheets" && e.Action == "Deleted")
                .Should().Be(12);

            result = new AuditTrailEntriesResult(
                result.AuditTrailEntries.Where(e => !(e.SourceEntityName == "CollectionSignatureSheets" && e.Action == "Deleted"))
                    .ToList(),
                result.CollectionCitizenLogAuditTrailEntries);

            await Verify(result);
        });
    }

    [Fact]
    public async Task ShouldWorkAsMuAdmin()
    {
        var request = NewValidRequest(x => x.CollectionId = ReferendumsMuStGallen.IdInCollectionActive);
        var sheetsCreatedIds = await RunOnDb(db => db.CollectionSignatureSheets
            .Where(x => x.CollectionMunicipality!.CollectionId == GuidParser.Parse(request.CollectionId) && x.State == CollectionSignatureSheetState.Created)
            .Select(x => x.Id)
            .ToListAsync());
        sheetsCreatedIds.Should().NotBeEmpty();

        var hasCitizens = await RunOnDb(db => db.CollectionCitizens
            .Where(x => x.SignatureSheetId.HasValue && sheetsCreatedIds.Contains(x.SignatureSheetId.Value))
            .AnyAsync());
        hasCitizens.Should().BeTrue();

        var hasCitizenLogs = await RunOnDb(db => db.CollectionCitizenLogs
            .Where(x => x.CollectionCitizen!.SignatureSheetId.HasValue && sheetsCreatedIds.Contains(x.CollectionCitizen!.SignatureSheetId.Value))
            .AnyAsync());
        hasCitizenLogs.Should().BeTrue();

        var response = await MuSgStammdatenverwalterClient.SubmitSignatureSheetsAsync(request);

        var referendum = await RunOnDb(db => db.Referendums
            .Include(x => x.Municipalities!)
            .ThenInclude(x => x.SignatureSheets)
            .FirstAsync(x => x.Id == ReferendumsMuStGallen.GuidInCollectionActive));
        referendum.Municipalities!.Should().AllSatisfy(x => x.IsLocked.Should().BeTrue());
        referendum.State.Should().Be(CollectionState.SignatureSheetsSubmitted);
        referendum.Municipalities!.SelectMany(x => x.SignatureSheets!).Should().AllSatisfy(x =>
            x.State.Should().NotBe(CollectionSignatureSheetState.Created));

        var hasSheetsInCreatedState = await RunOnDb(db => db.CollectionSignatureSheets
            .Where(x => x.CollectionMunicipality!.CollectionId == GuidParser.Parse(request.CollectionId) && x.State == CollectionSignatureSheetState.Created)
            .AnyAsync());
        hasSheetsInCreatedState.Should().BeFalse();

        var hasCitizensAfterSubmit = await RunOnDb(db => db.CollectionCitizens
            .Where(x => x.SignatureSheetId.HasValue && sheetsCreatedIds.Contains(x.SignatureSheetId.Value))
            .AnyAsync());
        hasCitizensAfterSubmit.Should().BeFalse();

        var hasCitizenLogsAfterSubmit = await RunOnDb(db => db.CollectionCitizenLogs
            .Where(x => x.CollectionCitizen!.SignatureSheetId.HasValue && sheetsCreatedIds.Contains(x.CollectionCitizen!.SignatureSheetId.Value))
            .AnyAsync());
        hasCitizenLogsAfterSubmit.Should().BeFalse();

        var userNotifications = await RunOnDb(async db => await db.UserNotifications
            .Where(x => x.TemplateBag.CollectionId == ReferendumsMuStGallen.GuidInCollectionActive)
            .OrderBy(x => x.RecipientEMail)
            .ToListAsync());

        var collectionMessage = await RunOnDb(async db => await db.CollectionMessages.FirstAsync(x => x.CollectionId == ReferendumsMuStGallen.GuidInCollectionActive));
        await Verify(new { userNotifications, collectionMessage, response });
    }

    [Fact]
    public async Task AsMuOnCtCollectionShouldFail()
    {
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.SubmitSignatureSheetsAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsMuOnOtherMuCollectionShouldFail()
    {
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.SubmitSignatureSheetsAsync(NewValidRequest(x => x.CollectionId = ReferendumsMuStGallen.IdInCollectionActive)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task AsCtOnMuCollectionShouldFail()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.SubmitSignatureSheetsAsync(NewValidRequest(x => x.CollectionId = ReferendumsMuStGallen.IdInCollectionActive)),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task CollectionNotStartedShouldFail()
    {
        await ModifyDbEntities<ReferendumEntity>(
            e => e.Id == ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
            e => e.CollectionStartDate = MockedClock.NowDateOnly.AddDays(2));

        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.SubmitSignatureSheetsAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionService.CollectionServiceClient(channel)
            .SubmitSignatureSheetsAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private static SubmitSignatureSheetsRequest NewValidRequest(Action<SubmitSignatureSheetsRequest>? customizer = null)
    {
        var request = new SubmitSignatureSheetsRequest
        {
            CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection,
        };

        customizer?.Invoke(request);
        return request;
    }
}
