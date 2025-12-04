// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class PrepareDeleteInitiativeRequestTest : ProtoValidatorBaseTest<PrepareDeleteInitiativeRequest>
{
    protected override IEnumerable<PrepareDeleteInitiativeRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<PrepareDeleteInitiativeRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.InitiativeId = string.Empty);
        yield return NewValidRequest(x => x.InitiativeId = "not a guid");
    }

    private static PrepareDeleteInitiativeRequest NewValidRequest(Action<PrepareDeleteInitiativeRequest>? customizer = null)
    {
        var request = new PrepareDeleteInitiativeRequest
        {
            InitiativeId = "fdbe661b-1ce7-4b25-9f9a-46671e01f970",
        };

        customizer?.Invoke(request);
        return request;
    }
}
