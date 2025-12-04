// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class UpdateCommitteeMemberSortRequestTest : ProtoValidatorBaseTest<UpdateCommitteeMemberSortRequest>
{
    protected override IEnumerable<UpdateCommitteeMemberSortRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.NewIndex = int.MaxValue);
    }

    protected override IEnumerable<UpdateCommitteeMemberSortRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.InitiativeId = string.Empty);
        yield return NewValidRequest(x => x.InitiativeId = "not a guid");
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
        yield return NewValidRequest(x => x.NewIndex = -1);
    }

    private static UpdateCommitteeMemberSortRequest NewValidRequest(Action<UpdateCommitteeMemberSortRequest>? customizer = null)
    {
        var request = new UpdateCommitteeMemberSortRequest
        {
            InitiativeId = "ae0fff5c-60bf-436b-973d-ee84b8108d62",
            Id = "4ef87283-43c7-4b88-83ec-9c47ae869a8d",
            NewIndex = 12,
        };

        customizer?.Invoke(request);
        return request;
    }
}
