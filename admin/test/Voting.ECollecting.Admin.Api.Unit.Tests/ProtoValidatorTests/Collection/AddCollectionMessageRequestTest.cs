// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class AddCollectionMessageRequestTest : ProtoValidatorBaseTest<AddCollectionMessageRequest>
{
    protected override IEnumerable<AddCollectionMessageRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.Content = RandomStringUtil.GenerateComplexMultiLineText(1));
        yield return NewValidRequest(x => x.Content = RandomStringUtil.GenerateComplexMultiLineText(1_000));
    }

    protected override IEnumerable<AddCollectionMessageRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.Content = string.Empty);
        yield return NewValidRequest(x => x.Content = RandomStringUtil.GenerateComplexMultiLineText(1_001));
    }

    private static AddCollectionMessageRequest NewValidRequest(Action<AddCollectionMessageRequest>? customizer = null)
    {
        var request = new AddCollectionMessageRequest
        {
            CollectionId = "6f251777-d7c8-4d72-bb47-3f81b1065831",
            Content = "Message",
        };

        customizer?.Invoke(request);
        return request;
    }
}
