// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Referendum;

public class ListReferendumDecreesRequestTest : ProtoValidatorBaseTest<ListReferendumDecreesRequest>
{
    protected override IEnumerable<ListReferendumDecreesRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.Types_.Clear());
        yield return NewValidRequest(x => x.Bfs = RandomStringUtil.GenerateAlphanumericWhitespace(8));
        yield return NewValidRequest(x => x.Bfs = string.Empty);
    }

    protected override IEnumerable<ListReferendumDecreesRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Types_.Add(DomainOfInfluenceType.Unspecified));
        yield return NewValidRequest(x => x.Types_.Add((DomainOfInfluenceType)(-1)));
        yield return NewValidRequest(x => x.Bfs = RandomStringUtil.GenerateAlphanumericWhitespace(9));
    }

    private ListReferendumDecreesRequest NewValidRequest(Action<ListReferendumDecreesRequest>? customizer = null)
    {
        var request = new ListReferendumDecreesRequest
        {
            Types_ = { DomainOfInfluenceType.Mu },
            Bfs = "3203",
        };

        customizer?.Invoke(request);
        return request;
    }
}
