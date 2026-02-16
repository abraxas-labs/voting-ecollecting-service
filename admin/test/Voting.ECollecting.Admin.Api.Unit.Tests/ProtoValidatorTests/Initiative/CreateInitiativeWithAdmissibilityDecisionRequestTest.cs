// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;
using Voting.ECollecting.Proto.Admin.Services.V1.Models;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class CreateInitiativeWithAdmissibilityDecisionRequestTest : ProtoValidatorBaseTest<CreateInitiativeWithAdmissibilityDecisionRequest>
{
    protected override IEnumerable<CreateInitiativeWithAdmissibilityDecisionRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.Address = null);
        yield return NewValidRequest(x => x.SubTypeId = string.Empty);
        yield return NewValidRequest(x => x.Wording = string.Empty);
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateComplexSingleLineText(200));
        yield return NewValidRequest(x => x.Wording = RandomStringUtil.GenerateComplexMultiLineText(10_000));
        yield return NewValidRequest(x => x.GovernmentDecisionNumber = RandomStringUtil.GenerateComplexSingleLineText(50));
    }

    protected override IEnumerable<CreateInitiativeWithAdmissibilityDecisionRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Address = CollectionAddressTest.NewInvalidRequest());
        yield return NewValidRequest(x => x.DomainOfInfluenceType = DomainOfInfluenceType.Unspecified);
        yield return NewValidRequest(x => x.DomainOfInfluenceType = (DomainOfInfluenceType)(-1));
        yield return NewValidRequest(x => x.SubTypeId = "foobar");
        yield return NewValidRequest(x => x.Description = string.Empty);
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateComplexSingleLineText(201));
        yield return NewValidRequest(x => x.Wording = RandomStringUtil.GenerateComplexMultiLineText(10_001));
        yield return NewValidRequest(x => x.GovernmentDecisionNumber = string.Empty);
        yield return NewValidRequest(x => x.GovernmentDecisionNumber = RandomStringUtil.GenerateComplexSingleLineText(51));
        yield return NewValidRequest(x => x.AdmissibilityDecisionState = AdmissibilityDecisionState.Unspecified);
        yield return NewValidRequest(x => x.AdmissibilityDecisionState = (AdmissibilityDecisionState)(-1));
    }

    private CreateInitiativeWithAdmissibilityDecisionRequest NewValidRequest(Action<CreateInitiativeWithAdmissibilityDecisionRequest>? customizer = null)
    {
        var req = new CreateInitiativeWithAdmissibilityDecisionRequest
        {
            AdmissibilityDecisionState = AdmissibilityDecisionState.Open,
            GovernmentDecisionNumber = "1234",
            Address = CollectionAddressTest.NewValidRequest(),
            Description = "foo bar baz",
            DomainOfInfluenceType = DomainOfInfluenceType.Ct,
            SubTypeId = "0f5d915e-ae92-4b4a-9120-8f841addcf3a",
            Wording = "Für mehr Gerechtigkeit!",
        };

        customizer?.Invoke(req);
        return req;
    }
}
