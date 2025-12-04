// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Common;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class GetPendingCollectionPermissionByTokenRequestTest : ProtoValidatorBaseTest<GetPendingCollectionPermissionByTokenRequest>
{
    protected override IEnumerable<GetPendingCollectionPermissionByTokenRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<GetPendingCollectionPermissionByTokenRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Token = string.Empty);
        yield return NewValidRequest(x => x.Token = "foo bar");
    }

    private GetPendingCollectionPermissionByTokenRequest NewValidRequest(Action<GetPendingCollectionPermissionByTokenRequest>? modifier = null)
    {
        var req = new GetPendingCollectionPermissionByTokenRequest { Token = UrlToken.New() };
        modifier?.Invoke(req);
        return req;
    }
}
