// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Referendum;

public class SetReferendumInPreparationRequestTest : ProtoValidatorBaseTest<SetReferendumInPreparationRequest>
{
    protected override IEnumerable<SetReferendumInPreparationRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.ReferendumNumber = RandomStringUtil.GenerateSimpleSingleLineText(1));
        yield return NewValidRequest(x => x.ReferendumNumber = RandomStringUtil.GenerateSimpleSingleLineText(50));
    }

    protected override IEnumerable<SetReferendumInPreparationRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.ReferendumNumber = string.Empty);
        yield return NewValidRequest(x => x.ReferendumNumber = RandomStringUtil.GenerateSimpleSingleLineText(51));
        yield return NewValidRequest(x => x.ReferendumNumber = "Te\nst");
    }

    private SetReferendumInPreparationRequest NewValidRequest(Action<SetReferendumInPreparationRequest>? customizer = null)
    {
        var request = new SetReferendumInPreparationRequest
        {
            ReferendumNumber = "CH-123.456",
        };

        customizer?.Invoke(request);
        return request;
    }
}
