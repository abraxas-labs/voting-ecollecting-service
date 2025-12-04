// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class CreateCollectionPermissionRequestTest : ProtoValidatorBaseTest<CreateCollectionPermissionRequest>
{
    protected override IEnumerable<CreateCollectionPermissionRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.LastName = RandomStringUtil.GenerateComplexSingleLineText(1));
        yield return NewValidRequest(x => x.LastName = RandomStringUtil.GenerateComplexSingleLineText(100));
        yield return NewValidRequest(x => x.FirstName = RandomStringUtil.GenerateComplexSingleLineText(1));
        yield return NewValidRequest(x => x.FirstName = RandomStringUtil.GenerateComplexSingleLineText(100));
    }

    protected override IEnumerable<CreateCollectionPermissionRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.LastName = string.Empty);
        yield return NewValidRequest(x => x.LastName = RandomStringUtil.GenerateComplexSingleLineText(101));
        yield return NewValidRequest(x => x.LastName = "Te\nst");
        yield return NewValidRequest(x => x.FirstName = string.Empty);
        yield return NewValidRequest(x => x.FirstName = RandomStringUtil.GenerateComplexSingleLineText(101));
        yield return NewValidRequest(x => x.FirstName = "Te\nst");
        yield return NewValidRequest(x => x.Email = "Test");
        yield return NewValidRequest(x => x.Role = CollectionPermissionRole.Unspecified);
        yield return NewValidRequest(x => x.Role = (CollectionPermissionRole)(-1));
    }

    private static CreateCollectionPermissionRequest NewValidRequest(Action<CreateCollectionPermissionRequest>? customizer = null)
    {
        var request = new CreateCollectionPermissionRequest
        {
            CollectionId = "1ca7e955-33d5-4327-a3ee-c40b97118759",
            LastName = "Muster",
            FirstName = "Hans",
            Email = "hans.muster@example.com",
            Role = CollectionPermissionRole.Reader,
        };

        customizer?.Invoke(request);
        return request;
    }
}
