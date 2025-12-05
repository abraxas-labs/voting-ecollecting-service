// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.DecreeTests;

public class DecreeCreateTest : BaseGrpcTest<DecreeService.DecreeServiceClient>
{
    public DecreeCreateTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Acl);
    }

    [Fact]
    public async Task TestAsCtTenantShouldWork()
    {
        var response = await CtSgStammdatenverwalterClient.CreateAsync(NewValidRequest());
        var decree = await RunOnDb(db => db.Decrees.IgnoreQueryFilters().FirstAsync(x => x.Id == Guid.Parse(response.Id)));
        decree.SetPeriodState(GetService<TimeProvider>().GetUtcNowDateTime());
        await Verify(new { response, decree });
    }

    [Fact]
    public async Task TestAsCtTenantWhenInvalidDoiTypeShouldThrow()
    {
        var request = NewValidRequest(r => r.DomainOfInfluenceType = DomainOfInfluenceType.Mu);
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.CreateAsync(request),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task TestAsCtTenantWhenInvalidStartDateShouldThrow()
    {
        var request = NewValidRequest(r =>
            r.CollectionStartDate = new DateTime(2000, 05, 05, 0, 0, 0, DateTimeKind.Utc).ToTimestamp());
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.CreateAsync(request),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task TestAsCtTenantWhenInvalidEndDateShouldThrow()
    {
        var request = NewValidRequest(r =>
            r.CollectionEndDate = new DateTime(2024, 05, 05, 0, 0, 0, DateTimeKind.Utc).ToTimestamp());
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.CreateAsync(request),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task TestAsCtTenantWhenInvalidLinkShouldThrow()
    {
        var request = NewValidRequest(r => r.DomainOfInfluenceType = DomainOfInfluenceType.Ch);
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.CreateAsync(request),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task TestAsMuTenantShouldWork()
    {
        var request = NewValidRequest(r => r.DomainOfInfluenceType = DomainOfInfluenceType.Mu);
        var response = await MuSgStammdatenverwalterClient.CreateAsync(request);
        var decree = await RunOnDb(db => db.Decrees.IgnoreQueryFilters().FirstAsync(x => x.Id == Guid.Parse(response.Id)));
        decree.SetPeriodState(GetService<TimeProvider>().GetUtcNowDateTime());
        await Verify(new { response, decree });
    }

    [Fact]
    public async Task TestAsMuTenantWhenInvalidDoiTypeShouldThrow()
    {
        var request = NewValidRequest();
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.CreateAsync(request),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStammdatenverwalterClient.CreateAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
        => await new DecreeService.DecreeServiceClient(channel)
            .CreateAsync(NewValidRequest());

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private CreateDecreeRequest NewValidRequest(Action<CreateDecreeRequest>? customizer = null)
    {
        var request = new CreateDecreeRequest
        {
            DomainOfInfluenceType = DomainOfInfluenceType.Ct,
            Description = "Kantonsratsbeschluss Revision Wassergesetz (33-43.34)",
            CollectionStartDate = new DateTime(2024, 05, 05, 0, 0, 0, DateTimeKind.Utc).ToTimestamp(),
            CollectionEndDate = new DateTime(2024, 07, 05, 0, 0, 0, DateTimeKind.Utc).ToTimestamp(),
            Link = "https://www.ratsinfo.sg.ch/geschaefte/4754",
        };
        customizer?.Invoke(request);
        return request;
    }
}
