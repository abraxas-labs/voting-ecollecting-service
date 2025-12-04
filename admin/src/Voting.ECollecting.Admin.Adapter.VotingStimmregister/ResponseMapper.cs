// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Google.Protobuf.WellKnownTypes;
using Riok.Mapperly.Abstractions;
using Voting.ECollecting.Admin.Domain.Models;
using Voting.Lib.Database.Models;
using Voting.Stimmregister.Proto.V1.Services.Responses;

namespace Voting.ECollecting.Admin.Adapter.VotingStimmregister;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class ResponseMapper
{
    internal static IReadOnlyList<PersonInfo> MapToList(EcollectingServiceGetPeopleResponse response)
        => Map(response.People).ToList();

    internal static Page<IVotingStimmregisterPersonInfo> MapToPage(EcollectingServiceGetPeopleResponse response)
        => new(Map(response.People).ToList<IVotingStimmregisterPersonInfo>(), response.PageInfo.TotalCount, response.PageInfo.PageIndex + 1, response.PageInfo.PageSize);

    private static partial IEnumerable<PersonInfo> Map(IEnumerable<EcollectingServicePersonModel> personModel);

    private static partial PersonInfo Map(EcollectingServicePersonModel personModel);

    private static DateOnly MapToDateTime(Timestamp timestamp)
    {
        return DateOnly.FromDateTime(timestamp.ToDateTime());
    }
}
