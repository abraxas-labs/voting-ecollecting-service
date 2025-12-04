// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class DeleteCollectionPermissionRequestTest : ProtoValidatorBaseTest<DeleteCollectionPermissionRequest>
{
    protected override IEnumerable<DeleteCollectionPermissionRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<DeleteCollectionPermissionRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static DeleteCollectionPermissionRequest NewValidRequest(Action<DeleteCollectionPermissionRequest>? customizer = null)
    {
        var request = new DeleteCollectionPermissionRequest
        {
            Id = "0ae75645-de54-42ae-b9db-8bcf03078144",
        };

        customizer?.Invoke(request);
        return request;
    }
}
