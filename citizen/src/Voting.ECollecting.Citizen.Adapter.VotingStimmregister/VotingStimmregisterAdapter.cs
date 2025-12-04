// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Grpc.Core;
using Voting.ECollecting.Citizen.Abstractions.Adapter.VotingStimmregister;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;
using Voting.Stimmregister.Proto.V1.Services;
using Voting.Stimmregister.Proto.V1.Services.Requests;

namespace Voting.ECollecting.Citizen.Adapter.VotingStimmregister;

public class VotingStimmregisterAdapter : IVotingStimmregisterAdapter
{
    private readonly EcollectingService.EcollectingServiceClient _client;

    public VotingStimmregisterAdapter(EcollectingService.EcollectingServiceClient client)
    {
        _client = client;
    }

    public async Task<bool> HasVotingRight(string socialSecurityNumber, DomainOfInfluenceType doiType, string bfs)
    {
        try
        {
            await GetPersonInfo(socialSecurityNumber, doiType, bfs);
            return true;
        }
        catch (PersonOrVotingRightNotFoundException)
        {
            return false;
        }
    }

    public async Task<IVotingStimmregisterPersonInfo> GetPersonInfo(string socialSecurityNumber, DomainOfInfluenceType doiType, string bfs)
    {
        socialSecurityNumber = socialSecurityNumber.Trim().Replace(".", string.Empty, StringComparison.Ordinal);
        if (!long.TryParse(socialSecurityNumber, out var vn))
        {
            throw new PersonOrVotingRightNotFoundException();
        }

        if (!int.TryParse(bfs, out var bfsInt))
        {
            throw new PersonOrVotingRightNotFoundException();
        }

        var req = new EcollectingServiceGetPersonIdByAhvn13Request { Vn = vn };
        switch (doiType)
        {
            case DomainOfInfluenceType.Ch:
            case DomainOfInfluenceType.Ct:
                req.CantonBfs = bfsInt;
                break;
            case DomainOfInfluenceType.Mu:
                req.MunicipalityId = bfsInt;
                break;
        }

        try
        {
            var info = await _client.EcollectingServiceGetPersonIdByAhvn13Async(req);
            return ResponseMapper.Map(info);
        }
        catch (RpcException e) when (e.StatusCode == StatusCode.NotFound)
        {
            throw new PersonOrVotingRightNotFoundException();
        }
    }
}
