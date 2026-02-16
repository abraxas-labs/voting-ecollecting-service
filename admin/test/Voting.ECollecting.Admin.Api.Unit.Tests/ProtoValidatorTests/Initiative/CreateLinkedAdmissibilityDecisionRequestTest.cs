// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Models;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class CreateLinkedAdmissibilityDecisionRequestTest : ProtoValidatorBaseTest<CreateLinkedAdmissibilityDecisionRequest>
{
    protected override IEnumerable<CreateLinkedAdmissibilityDecisionRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.GovernmentDecisionNumber = RandomStringUtil.GenerateComplexSingleLineText(50));
    }

    protected override IEnumerable<CreateLinkedAdmissibilityDecisionRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.AdmissibilityDecisionState = AdmissibilityDecisionState.Unspecified);
        yield return NewValidRequest(x => x.AdmissibilityDecisionState = (AdmissibilityDecisionState)(-1));
        yield return NewValidRequest(x => x.GovernmentDecisionNumber = string.Empty);
        yield return NewValidRequest(x => x.GovernmentDecisionNumber = RandomStringUtil.GenerateComplexSingleLineText(51));
        yield return NewValidRequest(x => x.InitiativeId = "foo");
        yield return NewValidRequest(x => x.InitiativeId = string.Empty);
    }

    private CreateLinkedAdmissibilityDecisionRequest NewValidRequest(Action<CreateLinkedAdmissibilityDecisionRequest>? customizer = null)
    {
        var req = new CreateLinkedAdmissibilityDecisionRequest
        {
            AdmissibilityDecisionState = AdmissibilityDecisionState.Open,
            GovernmentDecisionNumber = "123",
            InitiativeId = "8fe5aab0-8f28-44b7-8d1d-9e3099082d8f",
        };

        customizer?.Invoke(req);
        return req;
    }
}
