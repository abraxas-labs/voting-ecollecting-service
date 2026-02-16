// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Abraxas.Voting.Ecollecting.Shared.V1.Models;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Initiative;

public class SetCollectionPeriodInitiativeRequestTest : ProtoValidatorBaseTest<SetCollectionPeriodInitiativeRequest>
{
    protected override IEnumerable<SetCollectionPeriodInitiativeRequest> OkMessages()
    {
        yield return NewValidRequest();
    }

    protected override IEnumerable<SetCollectionPeriodInitiativeRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Id = string.Empty);
        yield return NewValidRequest(x => x.Id = "not a guid");
        yield return NewValidRequest(x => x.CollectionStartDate = null);
        yield return NewValidRequest(x => x.CollectionEndDate = null);
    }

    private static SetCollectionPeriodInitiativeRequest NewValidRequest(Action<SetCollectionPeriodInitiativeRequest>? customizer = null)
    {
        var request = new SetCollectionPeriodInitiativeRequest
        {
            Id = "65179ad6-8707-44ca-bff5-9376f91620cd",
            CollectionStartDate = new Date { Day = 10, Month = 12, Year = 2020, },
            CollectionEndDate = new Date { Day = 12, Month = 12, Year = 2020, },
        };

        customizer?.Invoke(request);
        return request;
    }
}
