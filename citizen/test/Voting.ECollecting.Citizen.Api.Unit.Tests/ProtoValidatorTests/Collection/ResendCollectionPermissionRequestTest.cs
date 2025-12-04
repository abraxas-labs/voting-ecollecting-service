// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class ResendCollectionPermissionRequestTest : ProtoValidatorBaseTest<ResendCollectionPermissionRequest>
{
    protected override IEnumerable<ResendCollectionPermissionRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<ResendCollectionPermissionRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static ResendCollectionPermissionRequest NewValidRequest(Action<ResendCollectionPermissionRequest>? customizer = null)
    {
        var request = new ResendCollectionPermissionRequest
        {
            Id = "676bb961-7a31-404a-85ff-9f4095c83858",
        };

        customizer?.Invoke(request);
        return request;
    }
}
