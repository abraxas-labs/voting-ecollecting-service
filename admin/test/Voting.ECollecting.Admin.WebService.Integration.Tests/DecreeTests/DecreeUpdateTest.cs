// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.ComponentModel.DataAnnotations;
using Abraxas.Voting.Ecollecting.Shared.V1.Models;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.DecreeTests;

public class DecreeUpdateTest : BaseGrpcTest<DecreeService.DecreeServiceClient>
{
    public DecreeUpdateTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Decrees);
    }

    [Fact]
    public async Task TestAsCtTenantShouldWork()
    {
        await CtSgStammdatenverwalterClient.UpdateAsync(NewValidRequest());
        var decree = await RunOnDb(db => db.Decrees.FirstAsync(x => x.Id == DecreesCtStGallen.GuidFutureNoReferendum));
        decree.SetPeriodState(GetService<TimeProvider>().GetUtcTodayDateOnly());
        await Verify(decree);
    }

    [Fact]
    public async Task TestAsCtTenantWhenInCollectionShouldThrow()
    {
        var request = NewValidRequest(r => r.Id = DecreesCtStGallen.IdInCollectionWithReferendum);
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.UpdateAsync(request),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestAsCtTenantWhenNoPermissionOnOldDoiTypeShouldThrow()
    {
        var request = NewValidRequest(r => r.Id = DecreesMuStGallen.IdFutureNoReferendum);
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.UpdateAsync(request),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestAsCtTenantWhenNoPermissionOnNewDoiTypeShouldThrow()
    {
        var request = NewValidRequest(r => r.DomainOfInfluenceType = DomainOfInfluenceType.Mu);
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.UpdateAsync(request),
            StatusCode.InvalidArgument,
            $"{nameof(ValidationException)}: Expected exactly one item for doi type Mu but found none or more than one.");
    }

    [Fact]
    public async Task TestAsCtTenantWhenInvalidIdShouldThrowNotFound()
    {
        var request = NewValidRequest(r => r.Id = "0421cc96-0b93-439e-8715-c7ed984d99f9");
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.UpdateAsync(request),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestAsCtTenantWhenInvalidStartDateShouldThrow()
    {
        var request = NewValidRequest(r =>
            r.CollectionStartDate = new Date { Day = 05, Month = 05, Year = 2000 });
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.UpdateAsync(request),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestAsCtTenantWhenInvalidEndDateShouldThrow()
    {
        var request = NewValidRequest(r =>
            r.CollectionEndDate = new Date { Day = 05, Month = 05, Year = 2024 });
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.UpdateAsync(request),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task TestAsCtTenantWhenInvalidLinkShouldThrow()
    {
        var request = NewValidRequest(r => r.DomainOfInfluenceType = DomainOfInfluenceType.Ch);
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.UpdateAsync(request),
            StatusCode.InvalidArgument);
    }

    [Fact]
    public async Task TestAsMuTenantShouldWork()
    {
        var request = NewValidRequest(r =>
        {
            r.Id = DecreesMuStGallen.IdFutureNoReferendum;
            r.DomainOfInfluenceType = DomainOfInfluenceType.Mu;
        });
        await MuSgStammdatenverwalterClient.UpdateAsync(request);
        var decree = await RunOnDb(db => db.Decrees.FirstAsync(x => x.Id == DecreesMuStGallen.GuidFutureNoReferendum));
        decree.SetPeriodState(GetService<TimeProvider>().GetUtcTodayDateOnly());
        await Verify(decree);
    }

    [Fact]
    public async Task TestAsMuTenantWhenInCollectionShouldThrow()
    {
        var request = NewValidRequest(r => r.Id = DecreesMuStGallen.IdInCollectionWithReferendum);
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.UpdateAsync(request),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestAsMuTenantWhenNoPermissionOnOldDoiTypeShouldThrow()
    {
        var request = NewValidRequest();
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.UpdateAsync(request),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestAsMuTenantWhenNoPermissionOnOldMunicipalityShouldThrowNotFound()
    {
        var request = NewValidRequest(r => r.Id = DecreesMuGoldach.IdFutureNoReferendum);
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.UpdateAsync(request),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task TestAsMuTenantWhenNoPermissionOnNewDoiTypeShouldThrow()
    {
        var request = NewValidRequest(r => r.Id = DecreesMuStGallen.IdFutureNoReferendum);
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.UpdateAsync(request),
            StatusCode.InvalidArgument,
            $"{nameof(ValidationException)}: Expected exactly one item for doi type Ct but found none or more than one.");
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await CtSgStammdatenverwalterClient.UpdateAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries());
        });
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
        => await new DecreeService.DecreeServiceClient(channel)
            .UpdateAsync(NewValidRequest());

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private UpdateDecreeRequest NewValidRequest(Action<UpdateDecreeRequest>? customizer = null)
    {
        var request = new UpdateDecreeRequest
        {
            Id = DecreesCtStGallen.IdFutureNoReferendum,
            DomainOfInfluenceType = DomainOfInfluenceType.Ct,
            Description = "Kantonsratsbeschluss Revision Wassergesetz (33-43.34) UPDATED",
            CollectionStartDate = new Date { Day = 05, Month = 05, Year = 2024 },
            CollectionEndDate = new Date { Day = 05, Month = 07, Year = 2024 },
            Link = "https://www.ratsinfo.sg.ch/geschaefte/4754-updated",
        };
        customizer?.Invoke(request);
        return request;
    }
}
