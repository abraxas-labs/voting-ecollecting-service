// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Referendum;

public class GetReferendumRequestTest : ProtoValidatorBaseTest<GetReferendumRequest>
{
    protected override IEnumerable<GetReferendumRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<GetReferendumRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static GetReferendumRequest NewValidRequest(Action<GetReferendumRequest>? customizer = null)
    {
        var request = new GetReferendumRequest
        {
            Id = "ba8735fa-e9b6-4d3a-a28f-92f2de55bb14",
        };

        customizer?.Invoke(request);
        return request;
    }
}
