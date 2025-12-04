// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.DomainOfInfluence;

public class RemoveDomainOfInfluenceLogoRequestTest : ProtoValidatorBaseTest<RemoveDomainOfInfluenceLogoRequest>
{
    protected override IEnumerable<RemoveDomainOfInfluenceLogoRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.Bfs = RandomStringUtil.GenerateAlphabetic(8));
    }

    protected override IEnumerable<RemoveDomainOfInfluenceLogoRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Bfs = string.Empty);
        yield return NewValidRequest(x => x.Bfs = RandomStringUtil.GenerateAlphabetic(9));
    }

    private RemoveDomainOfInfluenceLogoRequest NewValidRequest(Action<RemoveDomainOfInfluenceLogoRequest>? customizer = null)
    {
        var request = new RemoveDomainOfInfluenceLogoRequest
        {
            Bfs = "1234",
        };

        customizer?.Invoke(request);
        return request;
    }
}
