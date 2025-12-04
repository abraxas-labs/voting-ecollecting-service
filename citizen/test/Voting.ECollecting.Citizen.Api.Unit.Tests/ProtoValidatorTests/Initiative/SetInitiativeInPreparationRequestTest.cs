// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class SetInitiativeInPreparationRequestTest : ProtoValidatorBaseTest<SetInitiativeInPreparationRequest>
{
    protected override IEnumerable<SetInitiativeInPreparationRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.GovernmentDecisionNumber = RandomStringUtil.GenerateSimpleSingleLineText(1));
        yield return NewValidRequest(x => x.GovernmentDecisionNumber = RandomStringUtil.GenerateSimpleSingleLineText(50));
    }

    protected override IEnumerable<SetInitiativeInPreparationRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.GovernmentDecisionNumber = string.Empty);
        yield return NewValidRequest(x => x.GovernmentDecisionNumber = RandomStringUtil.GenerateSimpleSingleLineText(51));
        yield return NewValidRequest(x => x.GovernmentDecisionNumber = "Te\nst");
    }

    private static SetInitiativeInPreparationRequest NewValidRequest(Action<SetInitiativeInPreparationRequest>? customizer = null)
    {
        var request = new SetInitiativeInPreparationRequest
        {
            GovernmentDecisionNumber = "CH-123.456",
        };

        customizer?.Invoke(request);
        return request;
    }
}
