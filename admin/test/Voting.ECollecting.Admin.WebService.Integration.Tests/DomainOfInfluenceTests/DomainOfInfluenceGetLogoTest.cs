// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net;
using FluentAssertions;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.DomainOfInfluenceTests;

public class DomainOfInfluenceGetLogoTest : BaseRestTest
{
    public DomainOfInfluenceGetLogoTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.DomainOfInfluences);
    }

    [Fact]
    public async Task ShouldGetAsCtOnCt()
    {
        var data = await CtStammdatenverwalterClient.GetByteArrayAsync(BuildUrl(Bfs.CantonStGallen));
        data.Should().BeEquivalentTo(Files.PlaceholderLogoPng);
    }

    [Fact]
    public async Task ShouldGetAsMuOnMu()
    {
        var data = await MuSgStammdatenverwalterClient.GetByteArrayAsync(BuildUrl(Bfs.MunicipalityStGallen));
        data.Should().BeEquivalentTo(Files.PlaceholderLogoPng);
    }

    [Fact]
    public async Task ShouldGetAsCtOnMu()
    {
        var data = await CtStammdatenverwalterClient.GetByteArrayAsync(BuildUrl(Bfs.MunicipalityStGallen));
        data.Should().BeEquivalentTo(Files.PlaceholderLogoPng);
    }

    [Fact]
    public async Task ShouldGetAsMuOnCt()
    {
        var data = await MuSgStammdatenverwalterClient.GetByteArrayAsync(BuildUrl(Bfs.CantonStGallen));
        data.Should().BeEquivalentTo(Files.PlaceholderLogoPng);
    }

    [Fact]
    public async Task ShouldThrowAsMuOnOtherMu()
    {
        await AssertStatus(
            async () => await MuGoldachStammdatenverwalterClient.GetAsync(BuildUrl(Bfs.MunicipalityStGallen)),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task TestNotFound()
    {
        await AssertStatus(
            async () => await CtStammdatenverwalterClient.GetAsync(BuildUrl("foobarbaz")),
            HttpStatusCode.NotFound);
    }

    protected override async Task<HttpResponseMessage> AuthorizationTestCall(HttpClient httpClient)
        => await httpClient.GetAsync(BuildUrl(Bfs.CantonStGallen));

    protected override IEnumerable<string> AuthorizedRoles() => [Roles.Stammdatenverwalter];

    private static string BuildUrl(string bfs)
        => $"v1/api/domain-of-influences/{bfs}/logo";
}
