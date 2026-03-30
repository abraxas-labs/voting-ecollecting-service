// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Net.Client;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.AuditTrailTests;

public class DecryptionAuditTrailTest : BaseGrpcTest<CollectionSignatureSheetService.CollectionSignatureSheetServiceClient>
{
    private static readonly Guid _initiativeSgSheet1Guid = CollectionSignatureSheets.BuildGuid(
        CollectionMunicipalities.BuildGuid(
            InitiativesCh.GuidEnabledForCollectionCollecting,
            Bfs.MunicipalityStGallen),
        1);

    public DecryptionAuditTrailTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Default.WithInitiatives(InitiativesCh.GuidEnabledForCollectionCollecting));
    }

    [Fact]
    public async Task TestAuditTrailDecryption()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await MuSgKontrollzeichenerfasserClient.ListCitizensAsync(NewValidRequest());
            var auditEntries = await GetAuditTrailEntries();
            await Verify(auditEntries);
        });
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionSignatureSheetService.CollectionSignatureSheetServiceClient(channel)
            .ListCitizensAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Kontrollzeichenerfasser;
        yield return Roles.Stichprobenverwalter;
    }

    private ListSignatureSheetCitizensRequest NewValidRequest(Action<ListSignatureSheetCitizensRequest>? customizer = null)
    {
        var request = new ListSignatureSheetCitizensRequest
        {
            CollectionId = InitiativesCh.IdEnabledForCollectionCollecting,
            SignatureSheetId = _initiativeSgSheet1Guid.ToString(),
        };

        customizer?.Invoke(request);
        return request;
    }
}
