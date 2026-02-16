// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
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
using Voting.ECollecting.Shared.Core.Exceptions;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.Lib.Testing.Mocks;
using CollectionType = Voting.ECollecting.Proto.Shared.V1.Enums.CollectionType;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionAddSignatureSheetCitizenTest : BaseGrpcTest<CollectionSignatureSheetService.CollectionSignatureSheetServiceClient>
{
    private static readonly Guid _initiativeSgSheet1Guid = CollectionSignatureSheets.BuildGuid(
        CollectionMunicipalities.BuildGuid(
            InitiativesCh.GuidEnabledForCollectionCollecting,
            Bfs.MunicipalityStGallen),
        1);

    private static readonly Guid _initiativeSgSheet2Guid = CollectionSignatureSheets.BuildGuid(
        CollectionMunicipalities.BuildGuid(
            InitiativesCh.GuidEnabledForCollectionCollecting,
            Bfs.MunicipalityStGallen),
        2);

    private static readonly Guid _referendumSgSheet1Guid = CollectionSignatureSheets.BuildGuid(
        CollectionMunicipalities.BuildGuid(
            ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
            Bfs.MunicipalityStGallen),
        1);

    private static readonly Guid _referendumSgSheet2Guid = CollectionSignatureSheets.BuildGuid(
        CollectionMunicipalities.BuildGuid(
            ReferendumsCtStGallen.GuidInCollectionEnabledForCollection,
            Bfs.MunicipalityStGallen),
        2);

    private static readonly Guid _referendum2SgSheet1Guid = CollectionSignatureSheets.BuildGuid(
        CollectionMunicipalities.BuildGuid(
            ReferendumsCtStGallen.GuidInCollectionEnabledForCollection2,
            Bfs.MunicipalityStGallen),
        1);

    public CollectionAddSignatureSheetCitizenTest(TestApplicationFactory factory)
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
        await MuSgKontrollzeichenerfasserClient.AddCitizenAsync(req);
        var sheet = await RunOnDb(db => db.CollectionSignatureSheets
            .Include(x => x.CollectionMunicipality)
            .Include(x => x.Citizens.OrderBy(y => y.Log!.VotingStimmregisterIdMac)).ThenInclude(x => x.Log)
            .FirstAsync(x => x.Id == Guid.Parse(req.SignatureSheetId)));

        sheet.Citizens.Any(c =>
                c.CollectionMunicipality!.CollectionId == Guid.Parse(req.CollectionId)
                && c.SignatureSheetId == Guid.Parse(req.SignatureSheetId)
                && c.Log!.VotingStimmregisterIdMac.Length > 0)
            .Should()
            .BeTrue();
        await Verify(sheet);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await MuSgKontrollzeichenerfasserClient.AddCitizenAsync(NewValidInitiativeRequest());
            await Verify(await GetAuditTrailEntries()).ScrubMember("VotingStimmregisterIdEncrypted");
        });
    }

    [Fact]
    public async Task ShouldWorkParallelWithDifferentPersons()
    {
        var req = NewValidInitiativeRequest();
        IEnumerable<AddSignatureSheetCitizenRequest> requests =
        [
            req,
            NewValidInitiativeRequest(x => x.PersonRegisterId = VotingStimmregisterAdapterMock.VotingRightPerson8.RegisterId.ToString()),
            NewValidInitiativeRequest(x => x.PersonRegisterId = VotingStimmregisterAdapterMock.VotingRightPerson10.RegisterId.ToString()),
            NewValidInitiativeRequest(x => x.PersonRegisterId = VotingStimmregisterAdapterMock.VotingRightPerson11.RegisterId.ToString()),
        ];
        var tasks = requests
            .Select(async r => await MuSgKontrollzeichenerfasserClient.AddCitizenAsync(r));
        await Task.WhenAll(tasks);

        var sheet = await RunOnDb(db => db.CollectionSignatureSheets
            .Include(x => x.Citizens.OrderBy(y => y.Log!.VotingStimmregisterIdMac)).ThenInclude(x => x.Log)
            .FirstAsync(x => x.Id == Guid.Parse(req.SignatureSheetId)));

        sheet.Citizens.Should().HaveCount(5);
        sheet.Count.Valid.Should().Be(5);
        sheet.Count.Total.Should().Be(20);
        await Verify(sheet);
    }

    [Fact]
    public async Task ShouldThrowParallelWithDuplicatedPerson()
    {
        var req = NewValidInitiativeRequest();
        var requests = Enumerable.Repeat(req, 10);
        var tasks = requests
            .Select(async r => await MuSgKontrollzeichenerfasserClient.AddCitizenAsync(r))
            .ToArray();
        await AssertStatus(
            async () => await Task.WhenAll(tasks),
            StatusCode.InvalidArgument,
            nameof(CollectionAlreadySignedException));
        tasks.Count(t => t.IsFaulted).Should().Be(tasks.Length - 1);
        tasks.Count(t => t.IsCompletedSuccessfully).Should().Be(1);
    }

    [Fact]
    public async Task ShouldThrowParallelWithDuplicatedPersonOnDifferentSheets()
    {
        var req = NewValidInitiativeRequest();
        IEnumerable<AddSignatureSheetCitizenRequest> requests =
        [
            req,
            NewValidInitiativeRequest(x => x.SignatureSheetId = _initiativeSgSheet2Guid.ToString()),
        ];
        var tasks = requests
            .Select(async r => await MuSgKontrollzeichenerfasserClient.AddCitizenAsync(r))
            .ToArray();
        await AssertStatus(
            async () => await Task.WhenAll(tasks),
            StatusCode.InvalidArgument,
            nameof(CollectionAlreadySignedException));
        tasks.Count(t => t.IsFaulted).Should().Be(tasks.Length - 1);
        tasks.Count(t => t.IsCompletedSuccessfully).Should().Be(1);
    }

    [Fact]
    public async Task ShouldWorkReferendum()
    {
        var req = NewValidReferendumRequest();
        await MuSgKontrollzeichenerfasserClient.AddCitizenAsync(req);
        var sheet = await RunOnDb(db => db.CollectionSignatureSheets
            .Include(x => x.CollectionMunicipality)
            .Include(x => x.Citizens.OrderBy(y => y.Log!.VotingStimmregisterIdMac)).ThenInclude(x => x.Log)
            .FirstAsync(x => x.Id == Guid.Parse(req.SignatureSheetId)));

        sheet.Citizens.Any(c =>
                c.CollectionMunicipality!.CollectionId == Guid.Parse(req.CollectionId)
                && c.SignatureSheetId == Guid.Parse(req.SignatureSheetId)
                && c.Log!.VotingStimmregisterIdMac.Length > 0)
            .Should()
            .BeTrue();
        await Verify(sheet);
    }

    [Fact]
    public async Task ShouldWorkReferendumParallelWithDifferentPersons()
    {
        var req = NewValidReferendumRequest();
        IEnumerable<AddSignatureSheetCitizenRequest> requests =
        [
            req,
            NewValidReferendumRequest(x => x.PersonRegisterId = VotingStimmregisterAdapterMock.VotingRightPerson8.RegisterId.ToString()),
            NewValidReferendumRequest(x => x.PersonRegisterId = VotingStimmregisterAdapterMock.VotingRightPerson10.RegisterId.ToString()),
            NewValidReferendumRequest(x => x.PersonRegisterId = VotingStimmregisterAdapterMock.VotingRightPerson11.RegisterId.ToString()),
        ];
        var tasks = requests
            .Select(async r => await MuSgKontrollzeichenerfasserClient.AddCitizenAsync(r));
        await Task.WhenAll(tasks);

        var sheet = await RunOnDb(db => db.CollectionSignatureSheets
            .Include(x => x.Citizens.OrderBy(y => y.Log!.VotingStimmregisterIdMac)).ThenInclude(x => x.Log)
            .FirstAsync(x => x.Id == Guid.Parse(req.SignatureSheetId)));

        sheet.Citizens.Should().HaveCount(5);
        sheet.Count.Valid.Should().Be(5);
        sheet.Count.Total.Should().Be(20);
        await Verify(sheet);
    }

    [Fact]
    public async Task ShouldThrowReferendumParallelWithDuplicatedPerson()
    {
        var req = NewValidReferendumRequest();
        var requests = Enumerable.Repeat(req, 10);
        var tasks = requests
            .Select(async r => await MuSgKontrollzeichenerfasserClient.AddCitizenAsync(r))
            .ToArray();
        await AssertStatus(
            async () => await Task.WhenAll(tasks),
            StatusCode.InvalidArgument,
            nameof(CollectionAlreadySignedException));
        tasks.Count(t => t.IsFaulted).Should().Be(tasks.Length - 1);
        tasks.Count(t => t.IsCompletedSuccessfully).Should().Be(1);
    }

    [Fact]
    public async Task ShouldThrowReferendumParallelWithDuplicatedPersonOnDifferentSheets()
    {
        var req = NewValidReferendumRequest();
        IEnumerable<AddSignatureSheetCitizenRequest> requests =
        [
            req,
            NewValidReferendumRequest(x => x.SignatureSheetId = _referendumSgSheet2Guid.ToString()),
        ];
        var tasks = requests
            .Select(async r => await MuSgKontrollzeichenerfasserClient.AddCitizenAsync(r))
            .ToArray();
        await AssertStatus(
            async () => await Task.WhenAll(tasks),
            StatusCode.InvalidArgument,
            nameof(CollectionAlreadySignedException));
        tasks.Count(t => t.IsFaulted).Should().Be(tasks.Length - 1);
        tasks.Count(t => t.IsCompletedSuccessfully).Should().Be(1);
    }

    [Fact]
    public async Task ShouldThrowReferendumParallelWithDuplicatedPersonOnDifferentCollections()
    {
        var req = NewValidReferendumRequest();
        IEnumerable<AddSignatureSheetCitizenRequest> requests =
        [
            req,
            NewValidReferendumRequest(x =>
            {
                x.CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection2;
                x.SignatureSheetId = _referendum2SgSheet1Guid.ToString();
            }),
        ];
        var tasks = requests
            .Select(async r => await MuSgKontrollzeichenerfasserClient.AddCitizenAsync(r))
            .ToArray();
        await AssertStatus(
            async () => await Task.WhenAll(tasks),
            StatusCode.InvalidArgument,
            nameof(DecreeAlreadySignedException));
        tasks.Count(t => t.IsFaulted).Should().Be(tasks.Length - 1);
        tasks.Count(t => t.IsCompletedSuccessfully).Should().Be(1);
    }

    [Fact]
    public async Task ShouldThrowAsOtherMu()
    {
        await AssertStatus(
            async () => await MuGoldachKontrollzeichenerfasserClient.AddCitizenAsync(NewValidInitiativeRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsCt()
    {
        await AssertStatus(
            async () => await CtSgKontrollzeichenerfasserClient.AddCitizenAsync(NewValidInitiativeRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowCollectionTypeMismatch()
    {
        var req = NewValidInitiativeRequest(x => x.CollectionType = CollectionType.Referendum);
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.AddCitizenAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowCollectionNotFound()
    {
        var req = NewValidInitiativeRequest(x => x.CollectionId = "70743aef-fd76-4a95-9dde-d033c3744001");
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.AddCitizenAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowSheetNotFound()
    {
        var req = NewValidInitiativeRequest(x => x.SignatureSheetId = "5a287c8e-db8a-4715-b971-2ed6fa4daae2");
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.AddCitizenAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowPersonNotFound()
    {
        var req = NewValidInitiativeRequest(x => x.PersonRegisterId = "d78d95dd-99e6-4aa9-8b69-16aa2fd27c89");
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.AddCitizenAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowPersonNoVotingRight()
    {
        var person = VotingStimmregisterAdapterMock.NoVotingRightPerson3;
        person.IsVotingAllowed.Should().BeFalse();

        var req = NewValidInitiativeRequest(x => x.PersonRegisterId = person.RegisterId.ToString());
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.AddCitizenAsync(req),
            StatusCode.InvalidArgument,
            $"{nameof(ValidationException)}: The person does not have the right to vote");
    }

    [Fact]
    public async Task ShouldThrowCollectionPublishedPeriodState()
    {
        var req = NewValidInitiativeRequest();
        await ModifyDbEntities<InitiativeEntity>(
            x => x.Id == Guid.Parse(req.CollectionId),
            x => x.CollectionStartDate = MockedClock.NowDateOnly.AddDays(1));
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.AddCitizenAsync(req),
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
            async () => await MuSgKontrollzeichenerfasserClient.AddCitizenAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowSheetFull()
    {
        var req = NewValidInitiativeRequest();
        await ModifyDbEntities<CollectionSignatureSheetEntity>(
            x => x.Id == Guid.Parse(req.SignatureSheetId),
            x => x.Count.Invalid = 0);
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.AddCitizenAsync(req),
            StatusCode.InvalidArgument,
            $"{nameof(ValidationException)}: The signature sheet is full.");
    }

    [Fact]
    public async Task MunicipalityLockedShouldThrow()
    {
        await ModifyDbEntities<CollectionMunicipalityEntity>(
            x => x.CollectionId == ReferendumsCtStGallen.GuidInCollectionEnabledForCollection && x.Bfs == Bfs.MunicipalityStGallen,
            x => x.IsLocked = true);
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.AddCitizenAsync(NewValidReferendumRequest()),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionSignatureSheetService.CollectionSignatureSheetServiceClient(channel)
            .AddCitizenAsync(NewValidInitiativeRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles() => [Roles.Kontrollzeichenerfasser];

    private AddSignatureSheetCitizenRequest NewValidInitiativeRequest(Action<AddSignatureSheetCitizenRequest>? customizer = null)
    {
        var request = new AddSignatureSheetCitizenRequest
        {
            CollectionId = InitiativesCh.IdEnabledForCollectionCollecting,
            CollectionType = CollectionType.Initiative,
            SignatureSheetId = _initiativeSgSheet1Guid.ToString(),
            PersonRegisterId = VotingStimmregisterAdapterMock.VotingRightPerson12.RegisterId.ToString(),
        };

        customizer?.Invoke(request);
        return request;
    }

    private AddSignatureSheetCitizenRequest NewValidReferendumRequest(Action<AddSignatureSheetCitizenRequest>? customizer = null)
    {
        var request = new AddSignatureSheetCitizenRequest
        {
            CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection,
            CollectionType = CollectionType.Referendum,
            SignatureSheetId = _referendumSgSheet1Guid.ToString(),
            PersonRegisterId = VotingStimmregisterAdapterMock.VotingRightPerson12.RegisterId.ToString(),
        };

        customizer?.Invoke(request);
        return request;
    }
}
