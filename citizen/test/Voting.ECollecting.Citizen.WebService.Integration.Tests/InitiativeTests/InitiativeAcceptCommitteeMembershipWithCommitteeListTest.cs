// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.ECollecting.Shared.Domain.Extensions;
using Voting.ECollecting.Shared.Test.MockedData;
using Voting.ECollecting.Shared.Test.Utils;
using Voting.Lib.Common;
using Voting.Lib.Testing.Mocks;

namespace Voting.ECollecting.Citizen.WebService.Integration.Tests.InitiativeTests;

public class InitiativeAcceptCommitteeMembershipWithCommitteeListTest : BaseRestTest
{
    private const string Email = "margarita@example.com";

    private static readonly Guid _id =
        InitiativeCommitteeMembers.BuildGuid(
            InitiativesCtStGallen.GuidLegislativeInPreparation,
            Email);

    private static readonly UrlToken _token =
        InitiativeCommitteeMembers.BuildToken(
            InitiativesCtStGallen.GuidLegislativeInPreparation,
            Email);

    public InitiativeAcceptCommitteeMembershipWithCommitteeListTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(RunScoped, SeederArgs.Initiatives.WithInitiatives(
            InitiativesCtStGallen.GuidLegislativeInPreparation,
            InitiativesCtStGallen.GuidLegislativeInPreparationDeputy));
    }

    [Fact]
    public async Task ShouldWork()
    {
        using var content = BuildSimpleContent();

        var resp = await Client.PostAsync(BuildUrl(), content);
        resp.EnsureSuccessStatusCode();

        var membership = await RunOnDb(db => db.InitiativeCommitteeMembers.SingleAsync(x => x.Id == _id));
        await Verify(membership);
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            using var content = BuildSimpleContent();

            var resp = await Client.PostAsync(BuildUrl(), content);
            await Verify(await GetAuditTrailEntries());
        });
    }

    [Fact]
    public async Task NotFound()
    {
        using var content = BuildSimpleContent(UrlToken.New());
        await AssertStatus(
            async () => await Client.PostAsync(BuildUrl(), content),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ExpiredShouldThrow()
    {
        await ModifyDbEntities(
            (InitiativeCommitteeMemberEntity e) => e.Token == _token,
            e => e.TokenExpiry = MockedClock.GetDate(-4));

        using var content = BuildSimpleContent();
        await AssertStatus(
            async () => await Client.PostAsync(BuildUrl(), content),
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OtherInitiativeShouldThrow()
    {
        using var content = BuildSimpleContent();
        await AssertStatus(
            async () => await Client.PostAsync(BuildUrl(InitiativesCtStGallen.IdLegislativeInPreparationDeputy), content),
            HttpStatusCode.NotFound);
    }

    [Theory]
    [EnumData<CollectionState>]
    public async Task CollectionState(CollectionState state)
    {
        await ModifyDbEntities<InitiativeEntity>(
            e => e.Id == InitiativesCtStGallen.GuidLegislativeInPreparation,
            e => e.State = state);

        using var content = BuildSimpleContent();
        var expectedStatus = state.InPreparationOrReturnForCorrection()
            ? HttpStatusCode.OK
            : HttpStatusCode.NotFound;
        await AssertStatus(
            async () => await Client.PostAsync(BuildUrl(), content),
            expectedStatus);
    }

    [Theory]
    [EnumData<InitiativeCommitteeMemberApprovalState>]
    public async Task ApprovalState(InitiativeCommitteeMemberApprovalState state)
    {
        await ModifyDbEntities<InitiativeCommitteeMemberEntity>(
            e => e.Id == _id,
            e => e.ApprovalState = state);

        using var content = BuildSimpleContent();
        var expectedStatus = state == InitiativeCommitteeMemberApprovalState.Requested
            ? HttpStatusCode.OK
            : HttpStatusCode.NotFound;
        await AssertStatus(
            async () => await Client.PostAsync(BuildUrl(), content),
            expectedStatus);
    }

    private static MultipartFormDataContent BuildSimpleContent(UrlToken? token = null, string? contentType = null)
    {
        var imageContent = new ByteArrayContent(Files.PlaceholderCommitteeListPdf);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue(contentType ?? "application/pdf");

        var data = new MultipartFormDataContent();
        data.Add(new StringContent(token ?? _token), "token");
        data.Add(imageContent, "file", Files.PlaceholderCommitteeListPdfName);
        return data;
    }

    private static string BuildUrl(string id = InitiativesCtStGallen.IdLegislativeInPreparation)
        => $"v1/api/initiatives/{id}/committee-members/accept";
}
