// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Riok.Mapperly.Abstractions;
using Voting.Stimmregister.Proto.V1.Services.Responses;

namespace Voting.ECollecting.Citizen.Adapter.VotingStimmregister;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
internal static partial class ResponseMapper
{
    internal static partial PersonInfo Map(EcollectingServiceGetPersonIdByAhvn13Response response);
}
