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

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.DomainOfInfluenceTests;

public class DomainOfInfluenceUpdateTest : BaseGrpcTest<DomainOfInfluenceService.DomainOfInfluenceServiceClient>
{
    public DomainOfInfluenceUpdateTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.DomainOfInfluences);
    }

    [Fact]
    public async Task ShouldWorkAsCtOnCt()
    {
        await CtSgStammdatenverwalterClient.UpdateAsync(NewValidRequest());

        var doi = await CtSgStammdatenverwalterClient.GetAsync(
            new GetDomainOfInfluenceRequest { Bfs = Bfs.CantonStGallen });
        await Verify(doi);
    }

    [Fact]
    public async Task ShouldWorkAsMuOnMu()
    {
        var req = NewValidRequest(x => x.Bfs = Bfs.MunicipalityStGallen);
        await MuSgStammdatenverwalterClient.UpdateAsync(req);

        var doi = await MuSgStammdatenverwalterClient.GetAsync(
            new GetDomainOfInfluenceRequest { Bfs = Bfs.MunicipalityStGallen });
        await Verify(doi);
    }

    [Fact]
    public async Task ShouldWorkOnNewMu()
    {
        var req = NewValidRequest(x => x.Bfs = Bfs.MunicipalityGoldach);
        await MuGoldachStammdatenverwalterClient.UpdateAsync(req);

        var doi = await MuGoldachStammdatenverwalterClient.GetAsync(
            new GetDomainOfInfluenceRequest { Bfs = Bfs.MunicipalityGoldach });
        await Verify(doi);
    }

    [Fact]
    public async Task ShouldThrowAsCtOnMu()
    {
        var req = NewValidRequest(x => x.Bfs = Bfs.MunicipalityStGallen);
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.UpdateAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsMuOnCt()
    {
        var req = NewValidRequest();
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.UpdateAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsMuOnOtherMu()
    {
        var req = NewValidRequest(x => x.Bfs = Bfs.MunicipalityStGallen);
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.UpdateAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowUnknownBfs()
    {
        var req = NewValidRequest(x => x.Bfs = "9999");
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.UpdateAsync(req),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new DomainOfInfluenceService.DomainOfInfluenceServiceClient(channel)
            .UpdateAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles() => [Roles.Stammdatenverwalter];

    private UpdateDomainOfInfluenceRequest NewValidRequest(Action<UpdateDomainOfInfluenceRequest>? customizer = null)
    {
        var req = new UpdateDomainOfInfluenceRequest
        {
            Bfs = Bfs.CantonStGallen,
            Email = "canton-sg-edited@example.com",
            Locality = "St.Gallen-updated",
            Phone = "+41 79 123 99 99",
            Street = "Edited street 99",
            Webpage = "https://www.sg-updated.example.ch",
            ZipCode = "9099",
            Name = "St. Gallen-updated",
        };
        customizer?.Invoke(req);
        return req;
    }
}
