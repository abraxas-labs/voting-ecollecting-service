// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Decree;

public class DeletePublishedDecreeRequestTest : ProtoValidatorBaseTest<DeletePublishedDecreeRequest>
{
    protected override IEnumerable<DeletePublishedDecreeRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<DeletePublishedDecreeRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
    }

    private static DeletePublishedDecreeRequest NewValidRequest(Action<DeletePublishedDecreeRequest>? customizer = null)
    {
        var request = new DeletePublishedDecreeRequest
        {
            Id = "8a191755-18f7-4fc5-b48b-dea278bfa5dd",
        };

        customizer?.Invoke(request);
        return request;
    }
}
