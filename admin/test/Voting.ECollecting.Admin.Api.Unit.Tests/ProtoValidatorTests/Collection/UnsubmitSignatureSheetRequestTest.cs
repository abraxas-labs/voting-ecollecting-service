// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class UnsubmitSignatureSheetRequestTest : ProtoValidatorBaseTest<UnsubmitSignatureSheetRequest>
{
    protected override IEnumerable<UnsubmitSignatureSheetRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<UnsubmitSignatureSheetRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.SignatureSheetId = string.Empty);
        yield return NewValidRequest(x => x.SignatureSheetId = "not a guid");
    }

    private static UnsubmitSignatureSheetRequest NewValidRequest(Action<UnsubmitSignatureSheetRequest>? customizer = null)
    {
        var request = new UnsubmitSignatureSheetRequest
        {
            CollectionId = "b2ec9cf8-f027-4229-af3c-e1b8ab46d359",
            SignatureSheetId = "2fbdfbf5-bae2-45d3-8c1a-3ef89818b46e",
        };

        customizer?.Invoke(request);
        return request;
    }
}
