// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class FlagInitiativeForReviewRequestTest : ProtoValidatorBaseTest<FlagInitiativeForReviewRequest>
{
    protected override IEnumerable<FlagInitiativeForReviewRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<FlagInitiativeForReviewRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static FlagInitiativeForReviewRequest NewValidRequest(Action<FlagInitiativeForReviewRequest>? customizer = null)
    {
        var request = new FlagInitiativeForReviewRequest
        {
            Id = "27a0eb26-2397-4540-a4ff-6507fe3641c9",
        };

        customizer?.Invoke(request);
        return request;
    }
}
