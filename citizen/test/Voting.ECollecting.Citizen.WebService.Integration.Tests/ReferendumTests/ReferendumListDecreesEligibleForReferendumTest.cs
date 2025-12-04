// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.ReferendumTests;

public class ReferendumListDecreesEligibleForReferendumTest : BaseGrpcTest<ReferendumService.ReferendumServiceClient>
{
    public ReferendumListDecreesEligibleForReferendumTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Referendums);

        // set referendums which are in collection to a random user
        await RunOnDb(db => db.Referendums
            .Where(x => x.DecreeId == DecreesCh.GuidInCollection || x.DecreeId == DecreesCtStGallen.GuidInCollectionWithReferendum)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.AuditInfo.CreatedById, "some-user")));

        // add more referendums to decree which is in collection
        await RunOnDb(db => db.Referendums
            .Where(x => x.DecreeId == DecreesCh.GuidFutureMultipleReferendum)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.DecreeId, DecreesCh.GuidInCollection)));
    }

    [Fact]
    public async Task ListAsCreator()
    {
        var response = await AuthenticatedClient.ListDecreesEligibleForReferendumAsync(NewValidRequest());
        await Verify(response);
    }

    [Fact]
    public async Task ListAsDeputy()
    {
        var response = await DeputyClient.ListDecreesEligibleForReferendumAsync(NewValidRequest());
        await Verify(response);
    }

    [Fact]
    public async Task ListAsReader()
    {
        var response = await ReaderClient.ListDecreesEligibleForReferendumAsync(NewValidRequest());
        await Verify(response);
    }

    [Fact]
    public async Task NoPermissionsShouldReturnOnlyInCollectionAndSignatureSheetsSubmitted()
    {
        var response = await AuthenticatedNoPermissionClient.ListDecreesEligibleForReferendumAsync(NewValidRequest());
        response.Groups
            .SelectMany(x => x.Decrees)
            .SelectMany(x => x.Collections)
            .Should().AllSatisfy(x =>
                x.Collection.State.Should().BeOneOf(CollectionState.EnabledForCollection, CollectionState.SignatureSheetsSubmitted));
    }

    [Fact]
    public async Task UnauthenticatedShouldFail()
    {
        await AssertStatus(
            async () => await Client.ListDecreesEligibleForReferendumAsync(NewValidRequest()),
            StatusCode.Unauthenticated);
    }

    [Fact]
    public async Task TestOnlyCtEnabled()
    {
        await WithOnlyCtDomainOfInfluenceTypeEnabled(async () =>
        {
            var response = await AuthenticatedClient.ListDecreesEligibleForReferendumAsync(NewValidRequest());
            await Verify(response);
        });
    }

    [Fact]
    public async Task TestOnlyCtFiltered()
    {
        var response = await AuthenticatedClient.ListDecreesEligibleForReferendumAsync(NewValidRequest(x => x.Types_.Add(DomainOfInfluenceType.Ct)));
        await Verify(response);
    }

    [Fact]
    public async Task TestOnlyBfsFiltered()
    {
        var response = await AuthenticatedClient.ListDecreesEligibleForReferendumAsync(NewValidRequest(x => x.Bfs = Bfs.MunicipalityStGallen));
        await Verify(response);
    }

    [Fact]
    public async Task ShouldWorkWithoutReferendums()
    {
        var response = await AuthenticatedClient.ListDecreesEligibleForReferendumAsync(NewValidRequest(x => x.IncludeReferendums = false));
        await Verify(response);
    }

    private ListDecreesEligibleForReferendumRequest NewValidRequest(Action<ListDecreesEligibleForReferendumRequest>? customizer = null)
    {
        var request = new ListDecreesEligibleForReferendumRequest
        {
            IncludeReferendums = true,
        };

        customizer?.Invoke(request);
        return request;
    }
}
