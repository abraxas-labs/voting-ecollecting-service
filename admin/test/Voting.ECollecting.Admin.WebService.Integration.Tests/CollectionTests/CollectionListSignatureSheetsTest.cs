// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Ecollecting.Shared.V1.Enums;
using Abraxas.Voting.Ecollecting.Shared.V1.Models;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Models;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionListSignatureSheetsTest : BaseGrpcTest<CollectionSignatureSheetService.CollectionSignatureSheetServiceClient>
{
    public CollectionListSignatureSheetsTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Referendums.WithReferendums(ReferendumsCtStGallen.GuidInCollectionEnabledForCollection, ReferendumsCtStGallen.GuidSignatureSheetsSubmitted) with
            {
                SeedReferendumSignatureSheets = true,
            });
    }

    [Fact]
    public async Task ShouldListCreated()
    {
        var sheets = await MuSgKontrollzeichenerfasserClient.ListAsync(NewValidRequest());
        await Verify(sheets);
    }

    [Fact]
    public async Task ShouldListAttestedWithSort()
    {
        var req = NewValidRequest();
        req.States.Clear();
        req.States.Add([CollectionSignatureSheetState.Attested, CollectionSignatureSheetState.Submitted, CollectionSignatureSheetState.NotSubmitted]);
        req.Sort = ListSignatureSheetsSort.CountTotal;
        req.SortDirection = SortDirection.Descending;
        var sheets = await MuSgKontrollzeichenerfasserClient.ListAsync(req);
        await Verify(sheets);
    }

    [Fact]
    public async Task ShouldListAttestedWithFilter()
    {
        var req = NewValidRequest();
        req.States.Clear();
        req.States.Add([CollectionSignatureSheetState.Attested, CollectionSignatureSheetState.Submitted, CollectionSignatureSheetState.NotSubmitted]);
        req.AttestedAts.Add(MockedClock.UtcNowDate.AddDays(-2).ToTimestamp());
        req.AttestedAts.Add(MockedClock.UtcNowDate.AddDays(-3).ToTimestamp());
        var sheets = await MuSgKontrollzeichenerfasserClient.ListAsync(req);
        await Verify(sheets);
    }

    [Fact]
    public async Task AsCtShouldThrow()
    {
        var req = NewValidRequest();
        await AssertStatus(
            async () => await CtSgKontrollzeichenerfasserClient.ListAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task CollectionNotFoundShouldThrow()
    {
        var req = NewValidRequest(r => r.CollectionId = "f413bb4c-28f0-40d8-974f-381c2f0556fc");
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.ListAsync(req),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionSignatureSheetService.CollectionSignatureSheetServiceClient(channel)
            .ListAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Kontrollzeichenerfasser;
        yield return Roles.Stichprobenverwalter;
    }

    private static ListSignatureSheetsRequest NewValidRequest(Action<ListSignatureSheetsRequest>? customizer = null)
    {
        var req = new ListSignatureSheetsRequest
        {
            CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection,
            Pageable = new Pageable
            {
                PageSize = 10,
                Page = 1,
            },
            Sort = ListSignatureSheetsSort.Number,
            SortDirection = SortDirection.Ascending,
            States = { CollectionSignatureSheetState.Created },
        };
        customizer?.Invoke(req);
        return req;
    }
}
