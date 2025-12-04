// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class UpdateRequestInformalReviewRequestTest : ProtoValidatorBaseTest<UpdateRequestInformalReviewRequest>
{
    protected override IEnumerable<UpdateRequestInformalReviewRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<UpdateRequestInformalReviewRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static UpdateRequestInformalReviewRequest NewValidRequest(Action<UpdateRequestInformalReviewRequest>? customizer = null)
    {
        var request = new UpdateRequestInformalReviewRequest
        {
            Id = "a7f417f1-e5b3-4102-bed9-f81de072967e",
            RequestInformalReview = true,
        };

        customizer?.Invoke(request);
        return request;
    }
}
