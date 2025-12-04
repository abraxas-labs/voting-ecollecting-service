// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Decree;

public class PrepareDeleteDecreeRequestTest : ProtoValidatorBaseTest<PrepareDeleteDecreeRequest>
{
    protected override IEnumerable<PrepareDeleteDecreeRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<PrepareDeleteDecreeRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.DecreeId = string.Empty);
        yield return NewValidRequest(x => x.DecreeId = "not a guid");
    }

    private static PrepareDeleteDecreeRequest NewValidRequest(Action<PrepareDeleteDecreeRequest>? customizer = null)
    {
        var request = new PrepareDeleteDecreeRequest
        {
            DecreeId = "fdbe661b-1ce7-4b25-9f9a-46671e01f970",
        };

        customizer?.Invoke(request);
        return request;
    }
}
