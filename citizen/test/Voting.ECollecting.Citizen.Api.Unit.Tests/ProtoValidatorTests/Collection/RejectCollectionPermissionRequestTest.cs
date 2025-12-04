// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Common;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class RejectCollectionPermissionRequestTest : ProtoValidatorBaseTest<RejectCollectionPermissionRequest>
{
    protected override IEnumerable<RejectCollectionPermissionRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<RejectCollectionPermissionRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Token = string.Empty);
        yield return NewValidRequest(x => x.Token = "foo bar");
    }

    private RejectCollectionPermissionRequest NewValidRequest(Action<RejectCollectionPermissionRequest>? modifier = null)
    {
        var req = new RejectCollectionPermissionRequest { Token = UrlToken.New() };
        modifier?.Invoke(req);
        return req;
    }
}
