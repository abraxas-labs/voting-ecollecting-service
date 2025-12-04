// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Ecollecting.Shared.V1.Models;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Decree;

public class CameAboutDecreeRequestTest : ProtoValidatorBaseTest<CameAboutDecreeRequest>
{
    protected override IEnumerable<CameAboutDecreeRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<CameAboutDecreeRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.DecreeId = string.Empty);
        yield return NewValidRequest(x => x.DecreeId = "not a guid");
        yield return NewValidRequest(x => x.SensitiveDataExpiryDate = null);
    }

    private static CameAboutDecreeRequest NewValidRequest(Action<CameAboutDecreeRequest>? customizer = null)
    {
        var request = new CameAboutDecreeRequest
        {
            DecreeId = "2f5cc187-05dc-4b76-8c40-68916629f293",
            SensitiveDataExpiryDate = new Date { Day = 10, Month = 12, Year = 2020, },
        };

        customizer?.Invoke(request);
        return request;
    }
}
