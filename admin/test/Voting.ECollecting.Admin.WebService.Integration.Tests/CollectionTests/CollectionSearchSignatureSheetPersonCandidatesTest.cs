// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.ECollecting.Shared.Test.MockedData;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.CollectionTests;

public class
    CollectionSearchSignatureSheetPersonCandidatesTest : BaseGrpcTest<CollectionSignatureSheetService.CollectionSignatureSheetServiceClient>
{
    private static readonly Guid _referendumSheetId = CollectionSignatureSheets.BuildGuid(
        CollectionMunicipalities.BuildGuid(
            ReferendumsCh.GuidInCollection,
            Bfs.MunicipalityStGallen),
        1);

    private static readonly Guid _initiativeSheetId = CollectionSignatureSheets.BuildGuid(
        CollectionMunicipalities.BuildGuid(
            InitiativesCh.GuidEnabledForCollectionCollecting,
            Bfs.MunicipalityStGallen),
        1);

    public CollectionSearchSignatureSheetPersonCandidatesTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Collections
                    .WithInitiatives(InitiativesCh.GuidEnabledForCollectionCollecting)
                    .WithReferendums(ReferendumsCh.GuidInCollection)
                with
            {
                SeedInitiativeSignatureSheets = true,
                SeedInitiativeCitizens = true,
                SeedReferendumSignatureSheets = true,
                SeedReferendumCitizens = true,
            });
    }

    [Fact]
    public async Task ShouldWorkReferendumSignedSameSheet()
    {
        var req = NewValidRequest(x =>
        {
            x.OfficialName = "BolligerT";
            x.FirstName = "LarsT";
        });
        var resp = await MuSgKontrollzeichenerfasserClient.SearchPersonCandidatesAsync(req);
        resp.Candidates.Should().HaveCount(1);
        resp.Candidates.All(c => c.Person.IsVotingAllowed).Should().BeTrue();
        resp.Candidates.All(c => c.ExistingSignature!.Electronic).Should().BeFalse();
        resp.Candidates.All(c => c.ExistingSignature!.IsInSameMunicipality).Should().BeTrue();
        resp.Candidates.All(c => c.ExistingSignature!.IsOnSameSheet).Should().BeTrue();
        await Verify(resp);
    }

    [Fact]
    public async Task ShouldWorkReferendumSignedElectronic()
    {
        var req = NewValidRequest(x =>
        {
            x.OfficialName = "GeigerT";
            x.FirstName = "PeterT";
        });
        var resp = await MuSgKontrollzeichenerfasserClient.SearchPersonCandidatesAsync(req);
        resp.Candidates.Should().HaveCount(1);
        resp.Candidates.All(c => c.Person.IsVotingAllowed).Should().BeTrue();
        resp.Candidates.All(c => c.ExistingSignature!.Electronic).Should().BeTrue();
        resp.Candidates.All(c => c.ExistingSignature!.IsInSameMunicipality).Should().BeTrue();
        resp.Candidates.All(c => c.ExistingSignature!.IsOnSameSheet).Should().BeFalse();
        await Verify(resp);
    }

    [Fact]
    public async Task ShouldWorkReferendumSignedOtherMunicipality()
    {
        var req = NewValidRequest(x =>
        {
            x.OfficialName = "SchaubT";
            x.FirstName = "Ang";
        });
        var resp = await MuSgKontrollzeichenerfasserClient.SearchPersonCandidatesAsync(req);
        resp.Candidates.Should().HaveCount(1);
        resp.Candidates.All(c => c.Person.IsVotingAllowed).Should().BeTrue();
        resp.Candidates.All(c => c.ExistingSignature!.Electronic).Should().BeFalse();
        resp.Candidates.All(c => c.ExistingSignature!.IsInSameMunicipality).Should().BeFalse();
        resp.Candidates.All(c => c.ExistingSignature!.IsOnSameSheet).Should().BeFalse();
        await Verify(resp);
    }

    [Fact]
    public async Task ShouldWorkReferendumNoVotingRight()
    {
        var req = NewValidRequest(x =>
        {
            x.OfficialName = "Abicht";
            x.FirstName = "Alicia";
            x.DateOfBirth = DateTime.SpecifyKind(new DateTime(1975, 05, 20), DateTimeKind.Utc).ToTimestamp();
        });
        var resp = await MuSgKontrollzeichenerfasserClient.SearchPersonCandidatesAsync(req);
        resp.Candidates.Should().HaveCount(1);
        resp.Candidates.All(c => c.Person.IsVotingAllowed).Should().BeFalse();
        resp.Candidates.All(c => c.ExistingSignature == null).Should().BeTrue();
        await Verify(resp);
    }

    [Fact]
    public async Task ShouldWorkInitiative()
    {
        var req = NewValidRequest(x =>
        {
            x.CollectionId = InitiativesCh.GuidEnabledForCollectionCollecting.ToString();
            x.SignatureSheetId = _initiativeSheetId.ToString();
            x.CollectionType = CollectionType.Initiative;
        });
        var resp = await MuSgKontrollzeichenerfasserClient.SearchPersonCandidatesAsync(req);
        await Verify(resp);
    }

    [Fact]
    public async Task ShouldThrowSheetOtherMunicipality()
    {
        await AssertStatus(
            async () => await MuGoldachKontrollzeichenerfasserClient.SearchPersonCandidatesAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowCollectionNotFound()
    {
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.SearchPersonCandidatesAsync(NewValidRequest(x => x.CollectionId = "572b38f7-fe0d-4fbb-923b-9edf2d347807")),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowSheetNotFound()
    {
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.SearchPersonCandidatesAsync(NewValidRequest(x => x.SignatureSheetId = "ac75823e-4ba4-4553-9eb6-cf85dcfe9c94")),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowSheetUnrelated()
    {
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.SearchPersonCandidatesAsync(NewValidRequest(x => x.SignatureSheetId = _initiativeSheetId.ToString())),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowAsCanton()
    {
        await AssertStatus(
            async () => await CtSgKontrollzeichenerfasserClient.SearchPersonCandidatesAsync(NewValidRequest()),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowCollectionTypeMismatch()
    {
        await AssertStatus(
            async () => await MuSgKontrollzeichenerfasserClient.SearchPersonCandidatesAsync(NewValidRequest(x => x.CollectionType = CollectionType.Initiative)),
            StatusCode.NotFound);
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new CollectionSignatureSheetService.CollectionSignatureSheetServiceClient(channel)
            .SearchPersonCandidatesAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Kontrollzeichenerfasser;
        yield return Roles.Stichprobenverwalter;
    }

    private SearchSignatureSheetPersonCandidatesRequest NewValidRequest(
        Action<SearchSignatureSheetPersonCandidatesRequest>? customizer = null)
    {
        var request = new SearchSignatureSheetPersonCandidatesRequest
        {
            CollectionId = ReferendumsCh.IdInCollection,
            SignatureSheetId = _referendumSheetId.ToString(),
            CollectionType = CollectionType.Referendum,
            OfficialName = "Bo",
        };
        customizer?.Invoke(request);
        return request;
    }
}
