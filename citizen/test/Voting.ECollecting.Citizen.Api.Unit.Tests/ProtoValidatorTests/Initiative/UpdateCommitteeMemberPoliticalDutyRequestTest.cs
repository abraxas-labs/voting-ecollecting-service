// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class UpdateCommitteeMemberPoliticalDutyRequestTest : ProtoValidatorBaseTest<UpdateCommitteeMemberPoliticalDutyRequest>
{
    protected override IEnumerable<UpdateCommitteeMemberPoliticalDutyRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.PoliticalDuty = string.Empty);
    }

    protected override IEnumerable<UpdateCommitteeMemberPoliticalDutyRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.InitiativeId = "invalid-guid");
        yield return NewValidRequest(x => x.InitiativeId = string.Empty);
        yield return NewValidRequest(x => x.Id = "invalid-guid");
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.PoliticalDuty = RandomStringUtil.GenerateComplexSingleLineText(51));
    }

    private UpdateCommitteeMemberPoliticalDutyRequest NewValidRequest(Action<UpdateCommitteeMemberPoliticalDutyRequest>? customizer = null)
    {
        var request = new UpdateCommitteeMemberPoliticalDutyRequest
        {
            Id = "74d415cd-916c-4571-9c73-517b94b56cec",
            InitiativeId = "751b2313-e7d3-4b98-bc7e-56097d8b4e9a",
            PoliticalDuty = "Protokollführer",
        };

        customizer?.Invoke(request);
        return request;
    }
}
