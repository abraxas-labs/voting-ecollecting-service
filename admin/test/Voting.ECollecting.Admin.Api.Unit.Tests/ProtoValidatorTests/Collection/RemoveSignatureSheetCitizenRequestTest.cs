// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class RemoveSignatureSheetCitizenRequestTest : ProtoValidatorBaseTest<RemoveSignatureSheetCitizenRequest>
{
    protected override IEnumerable<RemoveSignatureSheetCitizenRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<RemoveSignatureSheetCitizenRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "invalid-guid");
        yield return NewValidRequest(x => x.PersonRegisterId = string.Empty);
        yield return NewValidRequest(x => x.PersonRegisterId = "invalid-guid");
        yield return NewValidRequest(x => x.SignatureSheetId = string.Empty);
        yield return NewValidRequest(x => x.SignatureSheetId = "invalid-guid");
    }

    private static RemoveSignatureSheetCitizenRequest NewValidRequest(
        Action<RemoveSignatureSheetCitizenRequest>? customizer = null)
    {
        var request = new RemoveSignatureSheetCitizenRequest
        {
            CollectionId = "6d452445-97a4-497e-bef2-09a48c95304a",
            PersonRegisterId = "f25f30f2-2906-47d4-86fe-60d76c80d7c4",
            SignatureSheetId = "39c224b7-bfff-48ee-9727-2f8ae461447f",
        };
        customizer?.Invoke(request);
        return request;
    }
}
