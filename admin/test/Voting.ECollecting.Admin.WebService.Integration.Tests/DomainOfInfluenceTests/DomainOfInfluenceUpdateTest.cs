// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
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

    [Fact]
    public async Task SettingInitiativeMinSignatureCountOnCtShouldFail()
    {
        var req = NewValidRequest(x =>
        {
            x.Bfs = Bfs.CantonStGallen;
            x.Settings = new UpdateDomainOfInfluenceSettings { InitiativeMinSignatureCount = 1000 };
        });

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await CtSgStammdatenverwalterClient.UpdateAsync(req));
        ex.StatusCode.Should().Be(StatusCode.InvalidArgument);
        ex.Status.Detail.Should().Be("ValidationException: InitiativeMinSignatureCount is not supported for Ct");
    }

    [Fact]
    public async Task SettingInitiativeMaxElectronicSignaturePercentOnMuShouldFail()
    {
        var req = NewValidRequest(x =>
        {
            x.Bfs = Bfs.MunicipalityStGallen;
            x.Settings = new UpdateDomainOfInfluenceSettings { InitiativeMaxElectronicSignaturePercent = 50 };
        });

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await MuSgStammdatenverwalterClient.UpdateAsync(req));
        ex.StatusCode.Should().Be(StatusCode.InvalidArgument);
        ex.Status.Detail.Should().Be("ValidationException: InitiativeMaxElectronicSignaturePercent is not supported for Mu");
    }

    [Fact]
    public async Task SettingReferendumMaxElectronicSignaturePercentOnMuShouldFail()
    {
        var req = NewValidRequest(x =>
        {
            x.Bfs = Bfs.MunicipalityStGallen;
            x.Settings = new UpdateDomainOfInfluenceSettings { ReferendumMaxElectronicSignaturePercent = 50 };
        });

        var ex = await Assert.ThrowsAsync<RpcException>(async () => await MuSgStammdatenverwalterClient.UpdateAsync(req));
        ex.StatusCode.Should().Be(StatusCode.InvalidArgument);
        ex.Status.Detail.Should().Be("ValidationException: ReferendumMaxElectronicSignaturePercent is not supported for Mu");
    }

    [Fact]
    public async Task SettingInitiativeMinSignatureCountOnMuShouldWork()
    {
        var req = NewValidRequest(x =>
        {
            x.Bfs = Bfs.MunicipalityStGallen;
            x.Settings = new UpdateDomainOfInfluenceSettings { InitiativeMinSignatureCount = 9999 };
        });

        await MuSgStammdatenverwalterClient.UpdateAsync(req);

        var doi = await RunOnDb(db => db.DomainOfInfluences.SingleAsync(x => x.Bfs == Bfs.MunicipalityStGallen));
        doi.InitiativeMinSignatureCount.Should().Be(9999);
    }

    [Fact]
    public async Task SettingElectronicSignaturePercentOnCtShouldWork()
    {
        var req = NewValidRequest(x =>
        {
            x.Bfs = Bfs.CantonStGallen;
            x.Settings = new UpdateDomainOfInfluenceSettings
            {
                InitiativeMaxElectronicSignaturePercent = 42,
                ReferendumMaxElectronicSignaturePercent = 43,
            };
        });

        await CtSgStammdatenverwalterClient.UpdateAsync(req);

        var doi = await RunOnDb(db => db.DomainOfInfluences.SingleAsync(x => x.Bfs == Bfs.CantonStGallen));
        doi.InitiativeMaxElectronicSignaturePercent.Should().Be(42);
        doi.ReferendumMaxElectronicSignaturePercent.Should().Be(43);
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
            AddressName = "St. Gallen-updated",
        };
        customizer?.Invoke(req);
        return req;
    }
}
