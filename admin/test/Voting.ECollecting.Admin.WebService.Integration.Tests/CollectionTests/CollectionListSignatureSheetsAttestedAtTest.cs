// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Grpc.Net.Client;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionListSignatureSheetsAttestedAtTest : BaseGrpcTest<CollectionSignatureSheetService.CollectionSignatureSheetServiceClient>
{
    public CollectionListSignatureSheetsAttestedAtTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Referendums.WithReferendums(ReferendumsCtStGallen.GuidInCollectionEnabledForCollection) with
            {
                SeedReferendumSignatureSheets = true,
            });
    }

    [Fact]
    public async Task ShouldListAttestedAt()
    {
        var data = await MuSgKontrollzeichenerfasserClient.ListAttestedAtAsync(NewValidRequest());
        await Verify(data);
    }

    [Fact]
    public async Task AsCtShouldThrow()
    {
        await AssertStatus(
            async () => await CtSgKontrollzeichenerfasserClient.ListAttestedAtAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task CollectionNotFoundShouldThrow()
    {
        var req = NewValidRequest(r => r.CollectionId = "93d6777d-60e9-4eb9-995f-b3abea1b1a16");
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.ListAttestedAtAsync(req),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionSignatureSheetService.CollectionSignatureSheetServiceClient(channel)
            .ListAttestedAtAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Kontrollzeichenerfasser;
    }

    private static ListSignatureSheetsAttestedAtRequest NewValidRequest(Action<ListSignatureSheetsAttestedAtRequest>? customizer = null)
    {
        var req = new ListSignatureSheetsAttestedAtRequest
        {
            CollectionId = ReferendumsCtStGallen.IdInCollectionEnabledForCollection,
        };
        customizer?.Invoke(req);
        return req;
    }
}
