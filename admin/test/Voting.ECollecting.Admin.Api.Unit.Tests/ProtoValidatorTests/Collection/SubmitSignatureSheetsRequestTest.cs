// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class SubmitSignatureSheetsRequestTest : ProtoValidatorBaseTest<SubmitSignatureSheetsRequest>
{
    protected override IEnumerable<SubmitSignatureSheetsRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<SubmitSignatureSheetsRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
    }

    private static SubmitSignatureSheetsRequest NewValidRequest(Action<SubmitSignatureSheetsRequest>? customizer = null)
    {
        var request = new SubmitSignatureSheetsRequest
        {
            CollectionId = "1229e2e2-ae39-42e5-9f57-c5ecc7f1b4a4",
        };

        customizer?.Invoke(request);
        return request;
    }
}
