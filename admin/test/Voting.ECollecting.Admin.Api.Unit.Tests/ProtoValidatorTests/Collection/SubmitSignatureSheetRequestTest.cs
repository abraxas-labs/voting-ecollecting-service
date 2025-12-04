// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class SubmitSignatureSheetRequestTest : ProtoValidatorBaseTest<SubmitSignatureSheetRequest>
{
    protected override IEnumerable<SubmitSignatureSheetRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<SubmitSignatureSheetRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.SignatureSheetId = string.Empty);
        yield return NewValidRequest(x => x.SignatureSheetId = "not a guid");
    }

    private static SubmitSignatureSheetRequest NewValidRequest(Action<SubmitSignatureSheetRequest>? customizer = null)
    {
        var request = new SubmitSignatureSheetRequest
        {
            CollectionId = "450f5b71-2d45-4c7c-ab94-48e5e64e0c12",
            SignatureSheetId = "d92172e8-5d4d-4b5b-9da5-2c57ca8a0e7b",
        };

        customizer?.Invoke(request);
        return request;
    }
}
