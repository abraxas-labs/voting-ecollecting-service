// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Citizen.Services.V1;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.Lib.Grpc;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.AuthTests;

public class AuthTrackLoginTest : BaseGrpcTest<AuthService.AuthServiceClient>
{
    private readonly AuthService.AuthServiceClient _client;

    public AuthTrackLoginTest(TestApplicationFactory factory)
        : base(factory)
    {
        _client = CreateCitizenClient(
            userId: MockedDataSeederContext.Default.User.CitizenCreator.Id,
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            email: "creator-updated@example.com");
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation));
    }

    [Fact]
    public async Task ShouldUpdateIamInfo()
    {
        await _client.TrackLoginAsync(ProtobufEmpty.Instance);

        var permission = await RunOnDb(db =>
            db.CollectionPermissions.FirstAsync(x => x.CollectionId == InitiativesCtStGallen.GuidLegislativeInPreparation
            && x.IamUserId == MockedDataSeederContext.Default.User.CitizenCreator.Id));
        permission.Email.Should().Be("creator-updated@example.com");
        await Verify(permission).ScrubUrlTokens();
    }

    [Fact]
    public async Task ShouldNotUpdateIamInfoIfEmailNotVerified()
    {
        var client = CreateCitizenClient(
            userId: MockedDataSeederContext.Default.User.CitizenCreator.Id,
            acrValue: CitizenAuthMockDefaults.AcrValue400,
            email: "not-verified@example.com",
            emailVerified: false);

        await client.TrackLoginAsync(ProtobufEmpty.Instance);

        var permission = await RunOnDb(db =>
            db.CollectionPermissions.FirstAsync(x => x.CollectionId == InitiativesCtStGallen.GuidLegislativeInPreparation
            && x.IamUserId == MockedDataSeederContext.Default.User.CitizenCreator.Id));

        permission.Email.Should().NotBe("not-verified@example.com");
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await _client.TrackLoginAsync(ProtobufEmpty.Instance);
            await Verify(await GetAuditTrailEntries());
        });
    }
}
