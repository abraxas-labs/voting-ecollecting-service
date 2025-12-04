// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class DeleteCommitteeListRequestTest : ProtoValidatorBaseTest<DeleteCommitteeListRequest>
{
    protected override IEnumerable<DeleteCommitteeListRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<DeleteCommitteeListRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.InitiativeId = string.Empty);
        yield return NewValidRequest(x => x.InitiativeId = "not a guid");
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static DeleteCommitteeListRequest NewValidRequest(Action<DeleteCommitteeListRequest>? customizer = null)
    {
        var request = new DeleteCommitteeListRequest
        {
            InitiativeId = "97e70bfc-0251-4ad7-9f16-c8b5cc4d9078",
            Id = "ee44be71-1ea8-4e90-853d-b534744de123",
        };

        customizer?.Invoke(request);
        return request;
    }
}
