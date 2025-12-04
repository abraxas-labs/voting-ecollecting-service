// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.DomainOfInfluence;

public class UpdateDomainOfInfluenceRequestTest : ProtoValidatorBaseTest<UpdateDomainOfInfluenceRequest>
{
    protected override IEnumerable<UpdateDomainOfInfluenceRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.Bfs = RandomStringUtil.GenerateAlphabetic(8));
        yield return NewValidRequest(x => x.Name = RandomStringUtil.GenerateSimpleSingleLineText(100));
        yield return NewValidRequest(x => x.Street = RandomStringUtil.GenerateSimpleSingleLineText(150));
        yield return NewValidRequest(x => x.ZipCode = RandomStringUtil.GenerateSimpleSingleLineText(15));
        yield return NewValidRequest(x => x.Locality = RandomStringUtil.GenerateSimpleSingleLineText(150));
        yield return NewValidRequest(x => x.Webpage = RandomStringUtil.GenerateSimpleSingleLineText(10_000));
        yield return NewValidRequest(x => x.Phone = string.Empty);
        yield return NewValidRequest(x => x.Email = string.Empty);
        yield return NewValidRequest(x => x.Webpage = string.Empty);
    }

    protected override IEnumerable<UpdateDomainOfInfluenceRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Bfs = string.Empty);
        yield return NewValidRequest(x => x.Bfs = RandomStringUtil.GenerateAlphabetic(9));
        yield return NewValidRequest(x => x.Name = RandomStringUtil.GenerateSimpleSingleLineText(101));
        yield return NewValidRequest(x => x.Street = RandomStringUtil.GenerateSimpleSingleLineText(151));
        yield return NewValidRequest(x => x.ZipCode = RandomStringUtil.GenerateSimpleSingleLineText(16));
        yield return NewValidRequest(x => x.Locality = RandomStringUtil.GenerateSimpleSingleLineText(151));
        yield return NewValidRequest(x => x.Webpage = RandomStringUtil.GenerateSimpleSingleLineText(10_001));
        yield return NewValidRequest(x => x.Phone = "not a phone");
        yield return NewValidRequest(x => x.Email = "not an email");
    }

    private UpdateDomainOfInfluenceRequest NewValidRequest(Action<UpdateDomainOfInfluenceRequest>? customizer = null)
    {
        var request = new UpdateDomainOfInfluenceRequest
        {
            Name = "New Name",
            Bfs = "1234",
            Email = "hans@example.com",
            Locality = "foo",
            Phone = "+41 12 123 12 12",
            Street = "streez",
            Webpage = "word.com",
            ZipCode = "1234",
        };

        customizer?.Invoke(request);
        return request;
    }
}
