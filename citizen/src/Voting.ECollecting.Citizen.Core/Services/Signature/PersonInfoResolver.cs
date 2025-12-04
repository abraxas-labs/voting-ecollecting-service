// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.ECollecting.Citizen.Abstractions.Adapter.ELogin;
using Voting.ECollecting.Citizen.Abstractions.Adapter.VotingStimmregister;
using Voting.ECollecting.Shared.Domain.Entities;
using Voting.ECollecting.Shared.Domain.Enums;

namespace Voting.ECollecting.Citizen.Core.Services.Signature;

public class PersonInfoResolver
{
    private readonly IPermissionService _permissionService;
    private readonly IVotingStimmregisterAdapter _stimmregister;

    public PersonInfoResolver(IPermissionService permissionService, IVotingStimmregisterAdapter stimmregister)
    {
        _permissionService = permissionService;
        _stimmregister = stimmregister;
    }

    internal async Task<IVotingStimmregisterPersonInfo?> GetPersonInfo(
        DomainOfInfluenceType doiType,
        string bfs)
    {
        var userSocialSecurityNumber = await _permissionService.GetSocialSecurityNumber();
        if (!_permissionService.IsAuthenticated
            || userSocialSecurityNumber == null)
        {
            return null;
        }

        try
        {
            return await _stimmregister.GetPersonInfo(
                userSocialSecurityNumber,
                doiType,
                bfs);
        }
        catch (PersonOrVotingRightNotFoundException)
        {
            return null;
        }
    }
}
