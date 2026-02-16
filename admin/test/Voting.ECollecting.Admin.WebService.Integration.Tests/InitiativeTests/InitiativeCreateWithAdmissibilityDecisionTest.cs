// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using FluentAssertions;
using Grpc.Core;
using Grpc.Net.Client;
using Voting.ECollecting.Admin.Core.Exceptions;
using Voting.ECollecting.Admin.Domain.Authorization;
using Voting.ECollecting.DataSeeder.Data;
using Voting.ECollecting.DataSeeder.Data.DataSets;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.ModelBuilders;
using Voting.ECollecting.Shared.Test.MockedData;
using AdmissibilityDecisionState = Voting.ECollecting.Proto.Admin.Services.V1.Models.AdmissibilityDecisionState;
using CollectionAddress = Voting.ECollecting.Proto.Admin.Services.V1.Models.CollectionAddress;
using InitiativeService = Voting.ECollecting.Proto.Admin.Services.V1.InitiativeService;

namespace Voting.ECollecting.Admin.WebService.Integration.Tests.InitiativeTests;

public class InitiativeCreateWithAdmissibilityDecisionTest : BaseGrpcTest<InitiativeService.InitiativeServiceClient>
{
    public InitiativeCreateWithAdmissibilityDecisionTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await MockedDataSeeder.Seed(
            RunScoped,
            SeederArgs.Initiatives.WithInitiatives(
                InitiativesCtStGallen.GuidLegislativeSubmitted,
                InitiativesMuStGallen.GuidSubmitted));
    }

    [Fact]
    public async Task ShouldWorkAsMu()
    {
        var id = await MuSgStammdatenverwalterClient.CreateWithAdmissibilityDecisionAsync(NewValidRequest());
        var initiative = await MuSgStammdatenverwalterClient.GetAsync(new GetInitiativeRequest { Id = id.Id });
        initiative.Collection.State.Should().Be(CollectionState.PreRecorded);
        initiative.Collection.SecureIdNumber.Should().NotBeNullOrEmpty();
        await Verify(initiative);
    }

    [Fact]
    public async Task ShouldWorkAsMuValidSubjectToConditions()
    {
        var req = NewValidRequest(x => x.AdmissibilityDecisionState = AdmissibilityDecisionState.ValidButSubjectToConditions);
        var id = await MuSgStammdatenverwalterClient.CreateWithAdmissibilityDecisionAsync(req);
        var initiative = await MuSgStammdatenverwalterClient.GetAsync(new GetInitiativeRequest { Id = id.Id });
        initiative.Collection.State.Should().Be(CollectionState.PreRecorded);
        initiative.Collection.SecureIdNumber.Should().NotBeNullOrEmpty();
        await Verify(initiative);
    }

    [Fact]
    public async Task ShouldThrowWithoutWordingAsMu()
    {
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.CreateWithAdmissibilityDecisionAsync(NewValidRequest(x => x.Wording = string.Empty)),
            StatusCode.InvalidArgument,
            "Wording is required for non-federal initiatives");
    }

    [Fact]
    public async Task ShouldThrowWithoutAddressAsMu()
    {
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.CreateWithAdmissibilityDecisionAsync(NewValidRequest(x => x.Address = null)),
            StatusCode.InvalidArgument,
            "Address is required for non-federal initiatives");
    }

    [Fact]
    public async Task ShouldWorkAsMuRejected()
    {
        var id = await MuSgStammdatenverwalterClient.CreateWithAdmissibilityDecisionAsync(NewValidRequest(x => x.AdmissibilityDecisionState = AdmissibilityDecisionState.Rejected));
        var initiative = await MuSgStammdatenverwalterClient.GetAsync(new GetInitiativeRequest { Id = id.Id });
        initiative.Collection.State.Should().Be(CollectionState.PreRecorded);
        initiative.Collection.SecureIdNumber.Should().NotBeNullOrEmpty();
        initiative.AdmissibilityDecisionState.Should().Be(AdmissibilityDecisionState.Rejected);
        await Verify(initiative);
    }

    [Fact]
    public async Task ShouldThrowOnDuplicateGovernmentDecisionNumber()
    {
        const string existingGdn = "existing-789";
        await ModifyDbEntities((InitiativeEntity e) => e.Id == InitiativesMuStGallen.GuidSubmitted, x => x.GovernmentDecisionNumber = existingGdn);

        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.CreateWithAdmissibilityDecisionAsync(NewValidRequest(x => x.GovernmentDecisionNumber = existingGdn)),
            StatusCode.InvalidArgument,
            nameof(DuplicatedGovernmentDecisionNumberException));
    }

    [Fact]
    public async Task ShouldThrowOnDuplicateGovernmentDecisionNumberCaseInsensitive()
    {
        const string existingGdn = "existing-789";
        await ModifyDbEntities((InitiativeEntity e) => e.Id == InitiativesMuStGallen.GuidSubmitted, x => x.GovernmentDecisionNumber = existingGdn.ToUpperInvariant());

        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.CreateWithAdmissibilityDecisionAsync(NewValidRequest(x => x.GovernmentDecisionNumber = existingGdn)),
            StatusCode.InvalidArgument,
            nameof(DuplicatedGovernmentDecisionNumberException));
    }

    [Fact]
    public async Task ShouldWorkAsCt()
    {
        var id = await CtSgStammdatenverwalterClient.CreateWithAdmissibilityDecisionAsync(NewValidRequest(x =>
        {
            x.DomainOfInfluenceType = DomainOfInfluenceType.Ct;
            x.SubTypeId = InitiativeModelBuilder.LegislativeId.ToString();
        }));
        var initiative = await CtSgStammdatenverwalterClient.GetAsync(new GetInitiativeRequest { Id = id.Id });
        initiative.Collection.State.Should().Be(CollectionState.PreRecorded);
        initiative.Collection.SecureIdNumber.Should().NotBeNullOrEmpty();
        await Verify(initiative);
    }

    [Fact]
    public async Task ShouldThrowWithoutSubTypeAsCt()
    {
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.CreateWithAdmissibilityDecisionAsync(NewValidRequest(x => x.DomainOfInfluenceType = DomainOfInfluenceType.Ct)),
            StatusCode.InvalidArgument,
            "SubType is required for cantonal initiatives");
    }

    [Fact]
    public async Task ShouldThrowWithUnknownSubType()
    {
        var req = NewValidRequest(x =>
        {
            x.DomainOfInfluenceType = DomainOfInfluenceType.Ct;
            x.SubTypeId = "ef55b127-6fbf-4d02-a083-caad437445bb";
        });
        await AssertStatus(
            async () => await CtSgStammdatenverwalterClient.CreateWithAdmissibilityDecisionAsync(req),
            StatusCode.NotFound);
    }

    [Fact]
    public async Task ShouldThrowWithSubTypeOfOtherDoiType()
    {
        var req = NewValidRequest(x =>
        {
            x.DomainOfInfluenceType = DomainOfInfluenceType.Ct;
            x.SubTypeId = InitiativeModelBuilder.FederalId.ToString();
        });
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.CreateWithAdmissibilityDecisionAsync(req),
            StatusCode.InvalidArgument,
            "Expected exactly one item for doi type Ct but found none or more than one.");
    }

    [Fact]
    public async Task ShouldWorkAsCh()
    {
        var id = await CtSgStammdatenverwalterClient.CreateWithAdmissibilityDecisionAsync(NewValidRequest(x => x.DomainOfInfluenceType = DomainOfInfluenceType.Ch));
        var initiative = await CtSgStammdatenverwalterClient.GetAsync(new GetInitiativeRequest { Id = id.Id });
        initiative.Collection.State.Should().Be(CollectionState.PreRecorded);
        initiative.Collection.SecureIdNumber.Should().NotBeNullOrEmpty();
        await Verify(initiative);
    }

    [Fact]
    public async Task ShouldWorkAsChWithoutOptionalFields()
    {
        var id = await CtSgStammdatenverwalterClient.CreateWithAdmissibilityDecisionAsync(NewValidRequest(x =>
        {
            x.DomainOfInfluenceType = DomainOfInfluenceType.Ch;
            x.Address = null;
            x.Wording = string.Empty;
        }));
        var initiative = await CtSgStammdatenverwalterClient.GetAsync(new GetInitiativeRequest { Id = id.Id });
        initiative.Collection.State.Should().Be(CollectionState.PreRecorded);
        initiative.Collection.SecureIdNumber.Should().NotBeNullOrEmpty();
        await Verify(initiative);
    }

    [Fact]
    public async Task ShouldThrowAsMuDuplicatedDescription()
    {
        await MuSgStammdatenverwalterClient.CreateWithAdmissibilityDecisionAsync(NewValidRequest());
        await AssertStatus(
            async () => await MuSgStammdatenverwalterClient.CreateWithAdmissibilityDecisionAsync(NewValidRequest()),
            StatusCode.FailedPrecondition,
            nameof(CollectionAlreadyExistsException));
    }

    [Fact]
    public async Task TestAuditTrail()
    {
        await RunInAuditTrailTestScope(async () =>
        {
            await MuSgStammdatenverwalterClient.CreateWithAdmissibilityDecisionAsync(NewValidRequest());
            await Verify(await GetAuditTrailEntries())
                .ScrubMember(nameof(CollectionBaseEntity.SecureIdNumber));
        });
    }

    protected override async Task AuthorizationTestCall(GrpcChannel channel)
    {
        await new InitiativeService.InitiativeServiceClient(channel)
            .CreateWithAdmissibilityDecisionAsync(NewValidRequest());
    }

    protected override IEnumerable<string> AuthorizedRoles()
    {
        yield return Roles.Stammdatenverwalter;
    }

    private CreateInitiativeWithAdmissibilityDecisionRequest NewValidRequest(Action<CreateInitiativeWithAdmissibilityDecisionRequest>? customizer = null)
    {
        var req = new CreateInitiativeWithAdmissibilityDecisionRequest
        {
            AdmissibilityDecisionState = AdmissibilityDecisionState.Open,
            Description = "foo bar baz",
            DomainOfInfluenceType = DomainOfInfluenceType.Mu,
            GovernmentDecisionNumber = "12345",
            Address = new CollectionAddress
            {
                CommitteeOrPerson = "Hans Muster",
                StreetOrPostOfficeBox = "Postfach 12",
                ZipCode = "9001",
                Locality = "St.Gallen",
            },
            Wording = "Für mehr gerechtigkeit!",
        };

        customizer?.Invoke(req);
        return req;
    }
}
