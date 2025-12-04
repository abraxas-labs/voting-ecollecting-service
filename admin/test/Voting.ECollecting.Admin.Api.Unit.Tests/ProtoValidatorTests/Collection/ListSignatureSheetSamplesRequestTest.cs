// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class ListSignatureSheetSamplesRequestTest : ProtoValidatorBaseTest<ListSignatureSheetSamplesRequest>
{
    protected override IEnumerable<ListSignatureSheetSamplesRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<ListSignatureSheetSamplesRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
    }

    private static ListSignatureSheetSamplesRequest NewValidRequest(Action<ListSignatureSheetSamplesRequest>? customizer = null)
    {
        var request = new ListSignatureSheetSamplesRequest
        {
            CollectionId = "8086e709-74b9-4b6e-807d-c32e92ce34ba",
        };

        customizer?.Invoke(request);
        return request;
    }
}
