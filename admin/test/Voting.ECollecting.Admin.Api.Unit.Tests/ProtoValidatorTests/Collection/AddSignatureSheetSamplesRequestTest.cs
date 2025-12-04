// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class AddSignatureSheetSamplesRequestTest : ProtoValidatorBaseTest<AddSignatureSheetSamplesRequest>
{
    protected override IEnumerable<AddSignatureSheetSamplesRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<AddSignatureSheetSamplesRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.SignatureSheetsCount = 0);
        yield return NewValidRequest(x => x.SignatureSheetsCount = -10);
    }

    private static AddSignatureSheetSamplesRequest NewValidRequest(Action<AddSignatureSheetSamplesRequest>? customizer = null)
    {
        var request = new AddSignatureSheetSamplesRequest
        {
            CollectionId = "8086e709-74b9-4b6e-807d-c32e92ce34ba",
            SignatureSheetsCount = 10,
        };

        customizer?.Invoke(request);
        return request;
    }
}
