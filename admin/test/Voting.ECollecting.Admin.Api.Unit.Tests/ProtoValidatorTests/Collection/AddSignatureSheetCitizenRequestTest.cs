// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class AddSignatureSheetCitizenRequestTest : ProtoValidatorBaseTest<AddSignatureSheetCitizenRequest>
{
    protected override IEnumerable<AddSignatureSheetCitizenRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<AddSignatureSheetCitizenRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "invalid-guid");
        yield return NewValidRequest(x => x.CollectionType = CollectionType.Unspecified);
        yield return NewValidRequest(x => x.CollectionType = (CollectionType)(-1));
        yield return NewValidRequest(x => x.PersonRegisterId = string.Empty);
        yield return NewValidRequest(x => x.PersonRegisterId = "invalid-guid");
        yield return NewValidRequest(x => x.SignatureSheetId = string.Empty);
        yield return NewValidRequest(x => x.SignatureSheetId = "invalid-guid");
    }

    private static AddSignatureSheetCitizenRequest NewValidRequest(
        Action<AddSignatureSheetCitizenRequest>? customizer = null)
    {
        var request = new AddSignatureSheetCitizenRequest
        {
            CollectionId = "6d452445-97a4-497e-bef2-09a48c95304a",
            CollectionType = CollectionType.Initiative,
            PersonRegisterId = "f25f30f2-2906-47d4-86fe-60d76c80d7c4",
            SignatureSheetId = "39c224b7-bfff-48ee-9727-2f8ae461447f",
        };
        customizer?.Invoke(request);
        return request;
    }
}
