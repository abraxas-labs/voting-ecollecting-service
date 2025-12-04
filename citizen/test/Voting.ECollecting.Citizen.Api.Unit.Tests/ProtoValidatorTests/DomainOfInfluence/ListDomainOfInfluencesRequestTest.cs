// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Citizen.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Citizen.Api.Unit.Tests.ProtoValidatorTests.DomainOfInfluence;

public class ListDomainOfInfluencesRequestTest : ProtoValidatorBaseTest<ListDomainOfInfluencesRequest>
{
    protected override IEnumerable<ListDomainOfInfluencesRequest> OkMessages()
    {
        yield return new ListDomainOfInfluencesRequest();
        yield return new ListDomainOfInfluencesRequest
        {
            Types_ = { DomainOfInfluenceType.Mu },
            ECollectingEnabled = true,
        };
    }

    protected override IEnumerable<ListDomainOfInfluencesRequest> NotOkMessages()
    {
        yield return new ListDomainOfInfluencesRequest
        {
            Types_ = { (DomainOfInfluenceType)999 },
        };
    }
}
