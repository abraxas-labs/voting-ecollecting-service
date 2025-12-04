// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Referendum;

public class CreateReferendumRequestTest : ProtoValidatorBaseTest<CreateReferendumRequest>
{
    protected override IEnumerable<CreateReferendumRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateSimpleSingleLineText(1));
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateSimpleSingleLineText(200));
        yield return NewValidRequest(x => x.DecreeId = string.Empty);
    }

    protected override IEnumerable<CreateReferendumRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.DecreeId = "invalid-guid");
        yield return NewValidRequest(x => x.Description = string.Empty);
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateSimpleSingleLineText(201));
        yield return NewValidRequest(x => x.Description = "Te\nst");
    }

    private CreateReferendumRequest NewValidRequest(Action<CreateReferendumRequest>? customizer = null)
    {
        var request = new CreateReferendumRequest
        {
            Description = "Referendum gegen Autobahnausbau",
            DecreeId = "f715f5bf-728d-4109-b681-1191817e71b2",
        };

        customizer?.Invoke(request);
        return request;
    }
}
