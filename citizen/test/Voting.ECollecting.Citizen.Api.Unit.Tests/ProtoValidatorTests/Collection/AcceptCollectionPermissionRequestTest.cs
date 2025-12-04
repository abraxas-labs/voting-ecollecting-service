// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Common;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class AcceptCollectionPermissionRequestTest : ProtoValidatorBaseTest<AcceptCollectionPermissionRequest>
{
    protected override IEnumerable<AcceptCollectionPermissionRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<AcceptCollectionPermissionRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Token = string.Empty);
        yield return NewValidRequest(x => x.Token = "foo bar");
    }

    private AcceptCollectionPermissionRequest NewValidRequest(Action<AcceptCollectionPermissionRequest>? modifier = null)
    {
        var req = new AcceptCollectionPermissionRequest { Token = UrlToken.New() };
        modifier?.Invoke(req);
        return req;
    }
}
