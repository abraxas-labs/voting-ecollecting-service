// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Proto.Admin.Services.V1.Enums;
using Voting.ECollecting.Proto.Admin.Services.V1.Requests;
using Voting.ECollecting.Proto.Shared.V1.Enums;
using Voting.Lib.Testing.Validation;

namespace Voting.ECollecting.Admin.Api.Unit.Tests.ProtoValidatorTests.Collection;

public class ListCollectionsForDeletionRequestTest : ProtoValidatorBaseTest<ListCollectionsForDeletionRequest>
{
    protected override IEnumerable<ListCollectionsForDeletionRequest> OkMessages()
    {
        yield return NewValidRequest();
        yield return NewValidRequest(x => x.Types_.Clear());
        yield return NewValidRequest(x => x.Bfs = string.Empty);
    }

    protected override IEnumerable<ListCollectionsForDeletionRequest> NotOkMessages()
    {
        yield return NewValidRequest(x => x.Bfs = "123456789");
        yield return NewValidRequest(x =>
        {
            x.Types_.Clear();
            x.Types_.Add((DomainOfInfluenceType)(-1));
        });
        yield return NewValidRequest(x => x.Filter = CollectionControlSignFilter.Unspecified);
        yield return NewValidRequest(x => x.Filter = (CollectionControlSignFilter)(-1));
    }

    private static ListCollectionsForDeletionRequest NewValidRequest(Action<ListCollectionsForDeletionRequest>? customizer = null)
    {
        var request = new ListCollectionsForDeletionRequest
        {
            Bfs = "1234",
            Filter = CollectionControlSignFilter.ReadyToDelete,
            Types_ = { DomainOfInfluenceType.Ch },
        };

        customizer?.Invoke(request);
        return request;
    }
}
