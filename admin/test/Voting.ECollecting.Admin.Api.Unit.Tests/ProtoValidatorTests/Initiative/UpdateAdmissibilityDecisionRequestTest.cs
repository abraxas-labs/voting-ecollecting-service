// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;
using AdmissibilityDecisionState = Voting.ECollecting.Proto.Admin.Services.V1.Models.AdmissibilityDecisionState;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class UpdateAdmissibilityDecisionRequestTest : ProtoValidatorBaseTest<UpdateAdmissibilityDecisionRequest>
{
    protected override IEnumerable<UpdateAdmissibilityDecisionRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.GovernmentDecisionNumber = RandomStringUtil.GenerateComplexSingleLineText(50));
    }

    protected override IEnumerable<UpdateAdmissibilityDecisionRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.InitiativeId = "foo bar");
        yield return NewValidRequest(x => x.InitiativeId = string.Empty);
        yield return NewValidRequest(x => x.AdmissibilityDecisionState = AdmissibilityDecisionState.Unspecified);
        yield return NewValidRequest(x => x.AdmissibilityDecisionState = (AdmissibilityDecisionState)(-1));
        yield return NewValidRequest(x => x.GovernmentDecisionNumber = RandomStringUtil.GenerateComplexSingleLineText(51));
    }

    private UpdateAdmissibilityDecisionRequest NewValidRequest(Action<UpdateAdmissibilityDecisionRequest>? customizer = null)
    {
        var req = new UpdateAdmissibilityDecisionRequest
        {
            AdmissibilityDecisionState = AdmissibilityDecisionState.ValidButSubjectToConditions,
            InitiativeId = "8fe5aab0-8f28-44b7-8d1d-9e3099082d8f",
        };

        customizer?.Invoke(req);
        return req;
    }
}
