// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Ecollecting.Shared.V1.Models;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.Lib.Testing.Utils;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Decree;

public class UpdateDecreeRequestTest : ProtoValidatorBaseTest<UpdateDecreeRequest>
{
    protected override IEnumerable<UpdateDecreeRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateComplexMultiLineText(1));
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateComplexMultiLineText(1_000));
        yield return NewValidRequest(x => x.Link = string.Empty);
        yield return NewValidRequest(x => x.Link = "https://example.com");
        yield return NewValidRequest(x => x.Link = RandomStringUtil.GenerateHttpsUrl(2_000));
    }

    protected override IEnumerable<UpdateDecreeRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
        yield return NewValidRequest(x => x.Description = string.Empty);
        yield return NewValidRequest(x => x.Description = RandomStringUtil.GenerateComplexMultiLineText(1_001));
        yield return NewValidRequest(x => x.CollectionStartDate = null);
        yield return NewValidRequest(x => x.CollectionEndDate = null);
        yield return NewValidRequest(x => x.Link = "http://example.com");
        yield return NewValidRequest(x => x.Link = "https://example\n.com");
        yield return NewValidRequest(x => x.Link = RandomStringUtil.GenerateHttpsUrl(2_001));
        yield return NewValidRequest(x => x.DomainOfInfluenceType = DomainOfInfluenceType.Unspecified);
        yield return NewValidRequest(x => x.DomainOfInfluenceType = (DomainOfInfluenceType)(-1));
    }

    private static UpdateDecreeRequest NewValidRequest(Action<UpdateDecreeRequest>? customizer = null)
    {
        var request = new UpdateDecreeRequest
        {
            Id = "8c732f36-e1cc-44fe-a0fd-38c66490ceca",
            Description = "Erlass XY",
            CollectionStartDate = new Date { Day = 10, Month = 12, Year = 2020, },
            CollectionEndDate = new Date { Day = 12, Month = 12, Year = 2020, },
            Link = "https://www.example.com",
            DomainOfInfluenceType = DomainOfInfluenceType.Ct,
        };

        customizer?.Invoke(request);
        return request;
    }
}
