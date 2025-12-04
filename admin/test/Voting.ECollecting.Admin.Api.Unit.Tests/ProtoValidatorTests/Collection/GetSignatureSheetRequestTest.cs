// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class GetSignatureSheetRequestTest : ProtoValidatorBaseTest<GetSignatureSheetRequest>
{
    protected override IEnumerable<GetSignatureSheetRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<GetSignatureSheetRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.SignatureSheetId = "not a guid");
        yield return NewValidRequest(x => x.SignatureSheetId = string.Empty);
    }

    private static GetSignatureSheetRequest NewValidRequest(Action<GetSignatureSheetRequest>? customizer = null)
    {
        var request = new GetSignatureSheetRequest
        {
            SignatureSheetId = "ac5a1df3-e546-42c3-a064-91558a5197d7",
            CollectionId = "d2543748-3855-431c-b064-00695508eeeb",
        };

        customizer?.Invoke(request);
        return request;
    }
}
