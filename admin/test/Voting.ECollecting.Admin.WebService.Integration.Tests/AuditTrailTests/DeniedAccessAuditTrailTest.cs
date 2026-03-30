// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.AuditTrailTests;

public class DeniedAccessAuditTrailTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public DeniedAccessAuditTrailTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(InitiativesCtStGallen.GuidLegislativeInPreparation));
    }

    [Fact]
    public async Task TestDeniedAccessAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await AssertStatus(
                async () => await CtSgZertifikatsverwalterClient.GetCommitteeAsync(new GetInitiativeCommitteeRequest()),
                StatusCode.PermissionDenied);

            var auditEntries = await GetAuditTrailEntries();
            await Verify(auditEntries);
        });
    }

    [Fact]
    public async Task TestSuccessfulNoAuditTrail()
    {
        await CtSgStammdatenverwalterClient.GetCommitteeAsync(new GetInitiativeCommitteeRequest
        {
            Id = InitiativesCtStGallen.IdLegislativeInPreparation,
        });
        var auditEntries = await GetAuditTrailEntries();
        auditEntries.AuditTrailEntries.Should().BeEmpty();
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new InitiativeService.InitiativeServiceClient(channel).GetCommitteeAsync(new GetInitiativeCommitteeRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }
}
