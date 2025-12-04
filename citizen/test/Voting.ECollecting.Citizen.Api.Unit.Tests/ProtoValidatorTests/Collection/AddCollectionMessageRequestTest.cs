// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class AddCollectionMessageRequestTest : ProtoValidatorBaseTest<AddCollectionMessageRequest>
{
    protected override IEnumerable<AddCollectionMessageRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.Content = RandomStringUtil.GenerateComplexMultiLineText(1000));
    }

    protected override IEnumerable<AddCollectionMessageRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.CollectionId = string.Empty);
        yield return NewValidRequest(x => x.CollectionId = "not a guid");
        yield return NewValidRequest(x => x.Content = RandomStringUtil.GenerateComplexMultiLineText(1001));
    }

    private static AddCollectionMessageRequest NewValidRequest(Action<AddCollectionMessageRequest>? customizer = null)
    {
        var request = new AddCollectionMessageRequest
        {
            CollectionId = "0d80e147-ca25-4188-be48-c829243909b3",
            Content = "foo bar",
        };

        customizer?.Invoke(request);
        return request;
    }
}
