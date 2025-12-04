// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Grpc.Net.Client;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.Certificates;

public class CertificatesListTest : BaseGrpcTest<CertificateService.CertificateServiceClient>
{
    public CertificatesListTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Certificates);
    }

    [Fact]
    public async Task ShouldWork()
    {
        var resp = await CtSgZertifikatsverwalterClient.ListAsync(new ListCertificatesRequest());
        await Verify(resp);
    }

    [Fact]
    public async Task ShouldThrowAsMu()
    {
        await AssertStatus(
            async () => await MuSgZertifikatsverwalterClient.ListAsync(new ListCertificatesRequest()),
            StatusCode.PermissionDenied);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CertificateService.CertificateServiceClient(channel)
            .ListAsync(new ListCertificatesRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Zertifikatsverwalter;
    }
}
