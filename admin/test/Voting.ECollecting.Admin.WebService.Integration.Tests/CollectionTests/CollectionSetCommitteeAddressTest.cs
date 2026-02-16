// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Models;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class CollectionSetCommitteeAddressTest : BaseGrpcTest<CollectionService.CollectionServiceClient>
{
    public CollectionSetCommitteeAddressTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Initiatives.WithInitiatives(
                InitiativesCh.GuidEnabledForCollectionCollectingWithoutAddress,
                InitiativesCh.GuidEnabledForCollectionCollecting));
    }

    [Fact]
    public async Task ShouldWork()
    {
        await MuSgKontrollzeichenerfasserClient.SetCommitteeAddressAsync(NewValidRequest());

        var collection = await RunOnDb(db =>
            db.Initiatives.SingleAsync(x => x.Id == InitiativesCh.GuidEnabledForCollectionCollectingWithoutAddress));
        collection.SetPeriodState(MockedClock.NowDateOnly);
        await Verify(collection);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await MuSgKontrollzeichenerfasserClient.SetCommitteeAddressAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task ShouldThrowAsCt()
    {
        await AssertStatus(
            async () => await CtSgKontrollzeichenerfasserClient.SetCommitteeAddressAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowHasAddressAlready()
    {
        var req = NewValidRequest(x => x.CollectionId = InitiativesCh.IdEnabledForCollectionCollecting);
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.SetCommitteeAddressAsync(req),
            StatusCode.InvalidArgument,
            "ValidationException: Cannot set the address if it is already set.");
    }

    [Fact]
    public async Task ShouldThrowNotFound()
    {
        var req = NewValidRequest(x => x.CollectionId = "e4f1274a-b844-4dbc-8160-29e8765be647");
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.SetCommitteeAddressAsync(req),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionService.CollectionServiceClient(channel)
            .SetCommitteeAddressAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Kontrollzeichenerfasser;
    }

    private SetCommitteeAddressRequest NewValidRequest(
        Action<SetCommitteeAddressRequest>? customizer = null)
    {
        var request = new SetCommitteeAddressRequest
        {
            CollectionId = InitiativesCh.IdEnabledForCollectionCollectingWithoutAddress,
            Address = new CollectionAddress
            {
                CommitteeOrPerson = "Test Committee",
                StreetOrPostOfficeBox = "Test Street",
                ZipCode = "12345",
                Locality = "Test City",
            },
        };

        customizer?.Invoke(request);
        return request;
    }
}
