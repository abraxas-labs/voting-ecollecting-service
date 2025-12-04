// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class FinishInformalReviewRequestTest : ProtoValidatorBaseTest<FinishInformalReviewRequest>
{
    protected override IEnumerable<FinishInformalReviewRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<FinishInformalReviewRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
    }

    private static FinishInformalReviewRequest NewValidRequest(Action<FinishInformalReviewRequest>? customizer = null)
    {
        var request = new FinishInformalReviewRequest
        {
            CollectionId = "e18d9e25-4862-4514-b603-2f25ada28f13",
        };

        customizer?.Invoke(request);
        return request;
    }
}
